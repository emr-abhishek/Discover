import json
from sentence_transformers import SentenceTransformer
from sklearn.metrics.pairwise import cosine_similarity
import pandas as pd
import math
import numpy as np

# --- Embedding Model ---
embedding_model_path = "/Users/abhishekprasad/Documents/RAG/Embedding_Models/E5-Large"
model = SentenceTransformer(embedding_model_path)

def load_chunks_from_json(json_file_path):
    """
    Load chunk data from a pre-generated JSON file.
    """
    with open(json_file_path, 'r') as f:
        chunks = json.load(f)
    return chunks

def load_ground_truth(json_file_path):
    """
    Load the ground truth query/answer set.
    """
    with open(json_file_path, 'r') as f:
        ground_truth = json.load(f)
    return ground_truth

def precompute_embeddings(chunks):
    """
    Precompute embeddings for all chunks and add to the chunk metadata.
    """
    print("Precomputing embeddings for all chunks...")
    
    # ------------------  Fields for Embedding  ------------------
    FIELDS_TO_EMBED = ["Title", "Description", "Work Item Type", "Severity"]
    # --------------------------------------------------------

    chunk_contents = []
    for chunk in chunks:
        text_to_embed = ""
        for field in FIELDS_TO_EMBED:
            field_value = chunk.get(field)
            if field_value is None or (isinstance(field_value, float) and np.isnan(field_value)):
                field_value = "None"
    
            text_to_embed += f"{field}: {str(field_value)}\n"
        chunk_contents.append("passage: " + text_to_embed)

    print(f"Example text to be embedded:\n---\n{chunk_contents[0]}\n---")
    
    all_embeddings = model.encode(chunk_contents)
    
    for chunk, embedding in zip(chunks, all_embeddings):
        chunk["embedding"] = embedding.tolist()
    return chunks

def compute_metrics(retrieved_ids, relevant_chunks):
    """
    'retrieved_ids' is the list of K chunk_ids returned by the search.
    'relevant_chunks' is the list of "correct" chunk_ids from the ground truth.
    """

    retrieved_ids = [int(x) for x in retrieved_ids]
    relevant_chunks = [int(x) for x in relevant_chunks]

    # Precision@K
    retrieved_and_relevant = set(retrieved_ids) & set(relevant_chunks)
    precision = len(retrieved_and_relevant) / len(retrieved_ids) if retrieved_ids else 0
    
    # Recall@K
    recall = len(retrieved_and_relevant) / len(relevant_chunks) if relevant_chunks else 0
    
    # F1-Score@K
    f1 = (2 * precision * recall) / (precision + recall) if precision + recall > 0 else 0

    def compute_mrr():
        for rank, chunk_id in enumerate(retrieved_ids, start=1):
            if chunk_id in relevant_chunks:
                return 1 / rank
        return 0

    def compute_ndcg():
        relevance_map = {chunk_id: (1 if chunk_id in relevant_chunks else 0) for chunk_id in retrieved_ids}
        dcg = 0
        for i, chunk_id in enumerate(retrieved_ids):
            rank = i + 1
            if relevance_map[chunk_id] == 1:
                dcg += 1 / math.log2(rank + 1)
        
        ideal_retrieved_count = min(len(retrieved_ids), len(relevant_chunks))
        idcg = sum(1 / math.log2(rank + 1) for rank in range(1, ideal_retrieved_count + 1))
        
        return dcg / idcg if idcg > 0 else 0

    return {
        "Precision": precision,
        "Recall": recall,
        "F1-Score": f1,
        "MRR": compute_mrr(),
        "nDCG": compute_ndcg(),
    }

def process_query(chunks, query_text, ground_truth_ids, top_n=5):
    """
    Process a single query against the chunks and score against ground truth.
    """
    query_embedding = model.encode('query: ' + query_text)
    
    UNIQUE_ID_FIELD = "ID"  #Unique identifier field

    similarities = [
        (chunk[UNIQUE_ID_FIELD], cosine_similarity([query_embedding], [chunk["embedding"]])[0][0])
        for chunk in chunks
    ]
    
    # Sort chunks by similarity score
    similarities.sort(key=lambda x: x[1], reverse=True)
    
    # Retrieve top N chunks
    retrieved_chunks_with_scores = similarities[:top_n]
    retrieved_ids = [chunk_id for chunk_id, _ in retrieved_chunks_with_scores]

    metrics = compute_metrics(retrieved_ids, ground_truth_ids)

    metrics["Query"] = query_text
    metrics["Ground Truth"] = ground_truth_ids
    metrics["Retrieved"] = retrieved_ids
    metrics["Retrieved with Scores"] = retrieved_chunks_with_scores
    
    return metrics

chunk_file_path = "SDR_TestData.json"
ground_truth_file_path = "SDR_Groundtruth.json" 
K_VALUE = 5     #How many results to retrieve.

# 1. Load data
chunks = load_chunks_from_json(chunk_file_path)
try:
    ground_truth = load_ground_truth(ground_truth_file_path)
except FileNotFoundError:
    print(f"ERROR: Ground truth file not found at '{ground_truth_file_path}'")
    exit()

# 2. Precompute embeddings
chunks = precompute_embeddings(chunks)

# 3. Run all test queries
all_metrics = []
print(f"\nRunning {len(ground_truth)} test queries...")

for i, test in enumerate(ground_truth):
    query = test["query_text"] 
    ground_truth_ids = test["relevant_doc_ids"]
    
    # Process the query
    result_metrics = process_query(chunks, query, ground_truth_ids, top_n=K_VALUE)
    all_metrics.append(result_metrics)
    print(f"Processed query {i+1}/{len(ground_truth)}")

# 4. Display results
print("\n--- Individual Query Results ---")
for i, result in enumerate(all_metrics):
    print(f"\nQuery {i+1}: {result['Query']}")
    print(f"  > MRR: {result['MRR']:.4f}, Precision: {result['Precision']:.4f}, Recall: {result['Recall']:.4f}")
    print(f"  > Retrieved: {result['Retrieved']}")
    print(f"  > Ground Truth: {result['Ground Truth']}")

# 5. Calculate and display average metrics
if all_metrics:
    metrics_df = pd.DataFrame(all_metrics)
    average_metrics = metrics_df[["Precision", "Recall", "F1-Score", "MRR", "nDCG"]].mean()
    print("\n--- Average Metrics (K={}) ---".format(K_VALUE))
    print(average_metrics.to_markdown(floatfmt=".4f"))
else:
    print("\nNo metrics were calculated. Check ground truth file.")