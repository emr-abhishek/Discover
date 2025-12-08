using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AB_EIP.Model
{
    public class Database
    {
        private static readonly Dictionary<string, DataObj> _dataStore = new Dictionary<string, DataObj>();
        private static readonly Dictionary<string, DataCommunication> _dataCommStore = new Dictionary<string, DataCommunication>();

        public static void CreateDatabase(DataObj dataObject)
        {
            if (dataObject == null) return;

            string key = $"{dataObject.FileType}{dataObject.FileNo}:{dataObject.ElementNo}";
            _dataStore[key] = dataObject;
        }

        public static void UpdateDatabase(DataCommunication dataCommObject)
        {
            if (dataCommObject == null) 
                return;

            string key = $"{dataCommObject.FileType}{dataCommObject.FileNo}:{dataCommObject.ElementNo}";

            if (_dataStore.TryGetValue(key, out DataObj existingDataObj))
            {
                existingDataObj.Value = dataCommObject.Value;
            }
        }

        public static int GetValue(string fileType, int fileNumber, int elementNumber)
        {
            string key = $"{fileType}{fileNumber}:{elementNumber}";

            if (_dataStore.TryGetValue(key, out DataObj dataObject))
            {
                // Return the stored value, converting it to an integer
                return (int)dataObject.UpdateValue;
            }
            return 0;
        }

        public static DataObj GetDataObject(int fileNumber, int elementNumber)
        {
            // Search all configured objects to find the one with the matching address.
            foreach (var dataObject in _dataStore.Values)
            {
                if (dataObject.FileNo == fileNumber && dataObject.ElementNo == elementNumber)
                {
                    return dataObject; // Will only ever find one match
                }
            }
            return null;
        }

        public static void UpdateValueFromClient(string fileType, int fileNumber, int elementNumber, int value)
        {
            // Find the correct object using the address, just like in a read
            DataObj dataObject = GetDataObject(fileNumber, elementNumber);

            if (dataObject != null)
            {
                // Update the value. This will automatically trigger the PropertyChanged event
                // and update any UI elements bound to this object.
                dataObject.Value = value.ToString();
            }
        }
    }
}
