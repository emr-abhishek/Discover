using System;
using System.Text;

namespace AB_EIP.Model
{
    public class CommandHandler
    {
        public static event Action<int, int, string, int> ValueWritten;

        public byte[] DataProcess(byte[] request, int length)
        {
            if (length < 2) return Encoding.ASCII.GetBytes("ERROR: Invalid request");

            byte RegisterCommand = request[0];
            byte DataCommand = request[63];

            //if (isBatteryLow)
            //{
            //    return Encoding.ASCII.GetBytes("Low Battery");
            //}
            //HandleUnsolicitedMessage(request, length);
            switch (RegisterCommand)
            {
                case 0x65: // Register Session
                    return HandleRegisterSession(request, length);

                case 0x70:
                    //    if (request[63] == 0x00)    // Word-Range Write
                    //    {
                    //        return HandleWordRangeWrite(request, length);
                    //    }
                    if (request[63] == 0x67)   // Typed Write
                    {
                        return HandleTypedWrite(request, length);
                    }
                    //    else if (request[63] == 0xA9)   // Protected Typed Write Two Address
                    //    {
                    //        return HandleProtectedTypedWriteTwoAddress(request, length);
                    //    }
                    //    else if (request[63] == 0xAA)   // Protected Typed Write Three Address
                    //    {
                    //        return HandleProtectedTypedWriteThreeAddress(request, length);
                    //    }
                    else if (request[63] == 0x68)   // Typed Read
                    {
                        return HandleTypedRead(request, length);
                    }
                    else
                        return Encoding.ASCII.GetBytes("ERROR: Unsupported command");

                case 0x6F: // Send RR Data
                    return HandleSendRRData(request, length);

                default:
                    return Encoding.ASCII.GetBytes("ERROR: Unsupported command");
            }
        }

        static byte[] HandleRegisterSession(byte[] request, int length)
        {
            // Register Session response structure
            byte[] response = new byte[28];

            // Command (Register Session Response)
            response[0] = 0x65;
            response[1] = 0x00;

            // Length (4 bytes in command-specific part)
            response[2] = 0x04;
            response[3] = 0x00;

            // Session Handle (assigned by server, non-zero)
            response[4] = 0x01;
            response[5] = 0x00;
            response[6] = 0x00;
            response[7] = 0x00;

            // Status (success)
            response[8] = 0x00;
            response[9] = 0x00;
            response[10] = 0x00;
            response[11] = 0x00;

            // Sender Context (copied from request)
            Array.Copy(request, 12, response, 12, 8);

            // Options
            response[20] = 0x00;
            response[21] = 0x00;
            response[22] = 0x00;
            response[23] = 0x00;

            // Protocol Version
            response[24] = 0x01;
            response[25] = 0x00;

            // Option Flags
            response[26] = 0x00;
            response[27] = 0x00;

            return response;
        }

        static byte[] HandleSendRRData(byte[] request, int length)
        {
            // Forward Open request and response
            byte[] response = new byte[70]; // Adjusted to the length expected

            // Encapsulation Header
            response[0] = 0x6F; // Command: Send RR Data
            response[1] = 0x00; // Reserved
            response[2] = 0x2E; // Length: 46 bytes (0x2E)
            response[3] = 0x00;
            Array.Copy(request, 4, response, 4, 4); // Session Handle
            response[8] = 0x00; // Status: Success
            response[9] = 0x00;
            response[10] = 0x00;
            response[11] = 0x00;
            Array.Copy(request, 12, response, 12, 8); // Sender Context
            response[20] = 0x00; // Options
            response[21] = 0x00;
            response[22] = 0x00;
            response[23] = 0x00;

            // Command Specific Data
            response[24] = 0x00; // Interface Handle: CIP
            response[25] = 0x00;
            response[26] = 0x00;
            response[27] = 0x00;
            response[28] = 0x00; // Timeout
            response[29] = 0x00;
            response[30] = 0x02; // Item Count: 2
            response[31] = 0x00;

            // Null Address Item
            response[32] = 0x00; // Type ID: Null Address Item
            response[33] = 0x00;
            response[34] = 0x00; // Length
            response[35] = 0x00;

            // Unconnected Data Item
            response[36] = 0xB2; // Type ID: Unconnected Data Item
            response[37] = 0x00;
            response[38] = 0x1E; // Length: 30 bytes (0x1E)
            response[39] = 0x00;

            // CIP Connection Manager
            response[40] = 0xD4; // Service: Forward Open
            response[41] = 0x00; // Response
            response[42] = 0x00; // Status: Success
            response[43] = 0x00;

            // CIP Connection Manager Command Specific Data
            response[47] = 0x34; // O->T Network Connection ID
            response[46] = 0x00;
            response[45] = 0x01;
            response[44] = 0x00;

            response[51] = 0x80; // T->O Network Connection ID
            response[50] = 0x00;
            response[49] = 0x00;
            response[48] = 0x07;

            response[53] = 0x00; // Connection Serial Number
            response[52] = 0x39;

            response[55] = 0x03; // Originator Vendor ID          
            response[54] = 0x87;

            response[59] = 0x12; // Originator Serial Number
            response[58] = 0x34;
            response[57] = 0x56;
            response[56] = 0x78;

            response[60] = 0x80; // O->T API
            response[61] = 0x96;
            response[62] = 0x98;
            response[63] = 0x00;

            response[64] = 0x80; // T->O API
            response[65] = 0x96;
            response[66] = 0x98;
            response[67] = 0x00;

            response[68] = 0x00; // Reserved
            response[69] = 0x00;

            return response;
        }

        private int GetFileNumber(byte[] request, int length)
        {
            int FileNumber = 0;
            int StartIndex = 64;
            int BytesToCopy = length - StartIndex;

            byte[] FunctionDataBytes = new byte[BytesToCopy];
            Array.Copy(request, StartIndex, FunctionDataBytes, 0, BytesToCopy);

            byte[] FileNumberDataBytes = new byte[2];

            if (FunctionDataBytes[6] != 255)      //Can print only upto File number 254
            {
                Array.Copy(FunctionDataBytes, 6, FileNumberDataBytes, 0, 1);
                FileNumber = BitConverter.ToInt16(FileNumberDataBytes, 0);
            }
            else if (FunctionDataBytes[6] == 255)      //Can print from 255 upto File number 999
            {
                Array.Copy(FunctionDataBytes, 7, FileNumberDataBytes, 0, 2);
                FileNumber = BitConverter.ToInt16(FileNumberDataBytes, 0);
            }
            return FileNumber;
        }

        private int GetElementNumber(byte[] request, int length)
        {
            int ElementNumber = 0;
            int StartIndex = 64;
            int BytesToCopy = length - StartIndex;

            byte[] FunctionDataBytes = new byte[BytesToCopy];
            Array.Copy(request, StartIndex, FunctionDataBytes, 0, BytesToCopy);

            int PointCount = FunctionDataBytes[2];
            byte[] ElementNumberDataBytes = new byte[2];

            //Case1: File number and Element number are between 0-254
            if (FunctionDataBytes[6] != 255 && FunctionDataBytes[7] != 255)      //Can print only upto element number 254
            {
                Array.Copy(FunctionDataBytes, 7, ElementNumberDataBytes, 0, 1);
                for (int i = 0; i < PointCount; i++)
                {
                    ElementNumber = BitConverter.ToInt16(ElementNumberDataBytes, 0);
                    ElementNumber = ElementNumber + i;
                }
            }

            //Case2: File number between 0-254 and Element number between 255-999
            else if (FunctionDataBytes[6] != 255 && FunctionDataBytes[7] == 255)      //Can print from 255 upto element number 999
            {
                Array.Copy(FunctionDataBytes, 8, ElementNumberDataBytes, 0, 2);
                for (int i = 0; i < PointCount; i++)
                {
                    ElementNumber = BitConverter.ToInt16(ElementNumberDataBytes, 0);
                    ElementNumber = ElementNumber + i;
                }
            }

            //Case3: File number between 255-999 and Element number between 0-254
            else if (FunctionDataBytes[6] == 255 && FunctionDataBytes[9] != 255)      //Can print only upto element number 254
            {
                Array.Copy(FunctionDataBytes, 9, ElementNumberDataBytes, 0, 1);
                for (int i = 0; i < PointCount; i++)
                {
                    ElementNumber = BitConverter.ToInt16(ElementNumberDataBytes, 0);
                    ElementNumber = ElementNumber + i;
                }
            }

            //Case4: File number between 255-999 and Element number between 255-999
            else if (FunctionDataBytes[6] == 255 && FunctionDataBytes[9] == 255)      //Can print from 255 upto element number 999
            {
                Array.Copy(FunctionDataBytes, 10, ElementNumberDataBytes, 0, 2);
                for (int i = 0; i < PointCount; i++)
                {
                    ElementNumber = BitConverter.ToInt16(ElementNumberDataBytes, 0);
                    ElementNumber = ElementNumber + i;
                }
            }
            return ElementNumber;
        }

        static string GetFileType(byte[] request, int length)
        {
            string FileType;

            int StartIndex = 64;    //Byte location from 0x70 command byte
            int BytesToCopy = length - StartIndex;

            byte[] FunctionDataBytes = new byte[BytesToCopy];
            Array.Copy(request, StartIndex, FunctionDataBytes, 0, BytesToCopy);

            int PointCount = FunctionDataBytes[2];

            //Finding file type identifier for Integer (N) and Float (F)
            int FileTypeLocation = FunctionDataBytes.Length - ((PointCount * 4) + 1);
            byte[] FileTypeBytes = new byte[2];
            Array.Copy(FunctionDataBytes, FileTypeLocation, FileTypeBytes, 0, 1);

            //Finding file type identifier for Counter (C) and Control (R)
            int CounterFileTypeLocation = FunctionDataBytes.Length - ((PointCount * 6) + 1);
            byte[] CounterFileTypeBytes = new byte[2];
            Array.Copy(FunctionDataBytes, CounterFileTypeLocation, CounterFileTypeBytes, 0, 1);

            //Finding file type identifier for Binary (B)
            int BinaryFileTypeLocation = FunctionDataBytes.Length - ((PointCount * 2) + 1);
            byte[] BinaryFileTypeBytes = new byte[2];
            Array.Copy(FunctionDataBytes, BinaryFileTypeLocation, BinaryFileTypeBytes, 0, 1);

            if (FileTypeBytes[0] == 0x04)
            {
                FileType = "Integer (N)";
                return FileType;
            }
            else if (FileTypeBytes[0] == 0x08)
            {
                FileType = "Float (F)";
                return FileType;
            }
            else if (CounterFileTypeBytes[0] == 0x66)
            {
                FileType = "Counter (C)";
                return FileType;
            }
            else if (CounterFileTypeBytes[0] == 0x76)
            {
                FileType = "Control (R)";
                return FileType;
            }
            else if (BinaryFileTypeBytes[0] == 0x42)
            {
                FileType = "Binary (B)";
                return FileType;
            }
            else
            {
                return "Error";
            }
        }

        static int TypedReadGetPointCount(byte[] request, int length)
        {
            int startIndex = 64;
            if (length <= startIndex + 2) return 0;

            return request[startIndex + 2];
        }

        private WriteRequestData GetWriteData(byte[] request, int length)
        {
            try
            {
                int startIndex = 64;
                int bytesToCopy = length - startIndex;
                byte[] functionData = new byte[bytesToCopy];
                Array.Copy(request, startIndex, functionData, 0, bytesToCopy);

                int pointCount = functionData[2];
                if (pointCount == 0) return null;


                //Parsing file number and element number
                // --- Get FileNumber ---
                int fileNumber = 0;
                if (functionData[6] != 255)
                    fileNumber = functionData[6];
                else
                    fileNumber = BitConverter.ToUInt16(functionData, 7);

                // --- Get ElementNumber ---
                int elementNumber = 0;

                //Case1: File number and Element number are between 0-254
                if (functionData[6] != 255 && functionData[7] != 255)                   //Can print only upto element number 254
                {
                    elementNumber = functionData[7];
                }

                //Case2: File number between 0-254 and Element number between 255-999
                else if (functionData[6] != 255 && functionData[7] == 255)              //Can print from 255 upto element number 999
                {
                    elementNumber = functionData[8];
                }

                //Case3: File number between 255-999 and Element number between 0-254
                else if (functionData[6] == 255 && functionData[9] != 255)      //Can print only upto element number 254
                {
                    elementNumber = functionData[9];
                }

                //Case4: File number between 255-999 and Element number between 255-999
                else if (functionData[6] == 255 && functionData[9] == 255)      //Can print from 255 upto element number 999
                {
                    elementNumber = functionData[10];
                }

                // --- Get Value ---
                int valueDataIndex = functionData.Length - (pointCount * 4);
                //int value = BitConverter.ToInt32(functionData, valueDataIndex);




                
                int value = 0;
                // --- Get FileType for Integer and Float ---
                int intFloatLocation = functionData.Length - ((pointCount * 4) + 1);
                byte intFloatByte = functionData[intFloatLocation];
                string fileType = "Integer (N)"; // Default

                // --- Get FileType for Counter and Control ---
                int CounterControlLocation = functionData.Length - ((pointCount * 6) + 1);
                byte CounterControlBytes = functionData[CounterControlLocation];

                // --- Get FileType for Binary ---
                int BinaryLocation = functionData.Length - ((pointCount * 2) + 1);
                byte BinaryBytes = functionData[BinaryLocation];

                // --- Get FileType for Input ---
                int InputLocation = functionData.Length - ((pointCount * 2) + 1);
                byte InputBytes = functionData[InputLocation];

                // --- Get FileType for Output ---
                int OutputLocation = functionData.Length - ((pointCount * 2) + 1);
                byte OutputBytes = functionData[OutputLocation];

                // --- Get FileType for Status ---
                int StatusLocation = functionData.Length - ((pointCount * 2) + 1);
                byte StatusBytes = functionData[StatusLocation];


                if (intFloatByte == 0x04)
                {
                    fileType = "Integer (N)";
                    int dataIndex = functionData.Length - (pointCount * 4);
                    value = BitConverter.ToInt32(functionData, dataIndex);
                }

                else if (intFloatByte == 0x08)
                {
                    fileType = "Float (F)";
                    int dataIndex = functionData.Length - (pointCount * 4);
                    float floatValue = BitConverter.ToSingle(functionData, dataIndex);
                    value = Convert.ToInt32(floatValue); // Convert float to int for simplicity
                }

                else if (CounterControlBytes == 0x66)
                {
                    fileType = "Counter (C)";
                    int dataIndex = functionData.Length - (pointCount * 6);
                    // For Counters, the client might write PRE or ACC. We'll read ACC.
                    value = BitConverter.ToInt16(functionData, dataIndex + 4); // ACC value
                }

                else if (CounterControlBytes == 0x76)
                {
                    fileType = "Control (R)";
                    int dataIndex = functionData.Length - (pointCount * 6);
                    // For Counters, the client might write LEN or POS. We'll read POS.
                    value = BitConverter.ToInt16(functionData, dataIndex + 4); // POS value
                }

                else if ((OutputBytes == 0x42) && (fileNumber == 0))
                {
                    fileType = "Output (O)";
                    int dataIndex = functionData.Length - (pointCount * 2);
                    value = BitConverter.ToInt16(functionData, dataIndex);
                }

                else if ((InputBytes == 0x42) && (fileNumber==1))
                {
                    fileType = "Input (I)";
                    int dataIndex = functionData.Length - (pointCount * 2);
                    value = BitConverter.ToInt16(functionData, dataIndex);
                }

                else if ((StatusBytes == 0x42) && (fileNumber == 2))
                {
                    fileType = "Status (I)";
                    int dataIndex = functionData.Length - (pointCount * 2);
                    value = BitConverter.ToInt16(functionData, dataIndex);
                }

                else if (BinaryBytes == 0x42)
                {
                    fileType = "Binary (B)";
                    int dataIndex = functionData.Length - (pointCount * 2);
                    bool boolValue = BitConverter.ToBoolean(functionData, dataIndex);
                    value = boolValue ? 1 : 0;
                }
                else
                {
                    return null; // Unknown file type
                }

                return new WriteRequestData
                {
                    FileType = fileType,
                    FileNumber = fileNumber,
                    ElementNumber = elementNumber,
                    Value = value
                };
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        // Helper class to hold parsed write data
        private class WriteRequestData
        {
            public string FileType { get; set; }
            public int FileNumber { get; set; }
            public int ElementNumber { get; set; }
            public int Value { get; set; }
        }

        private byte[] HandleTypedRead(byte[] request, int length)
        {
            int fileNumber = GetFileNumber(request, length);
            int elementNumber = GetElementNumber(request, length);
            int pointCount = TypedReadGetPointCount(request, length);

            DataObj dataObject = Database.GetDataObject(fileNumber, elementNumber);
            string fileType = dataObject.FileType;
            int dataByteCount = (pointCount * 2) + 1;

            // Base size is 65 bytes, plus 2 bytes for each data point requested
            int responseSize = 65 + (pointCount * 2);
            byte[] response = new byte[responseSize];

            // Constructing response 
            response[0] = 0x70; // Command
            response[1] = 0x00;
            //response[2] = (byte)((responseSize - 24) & 0xFF); // Dynamic length
            //response[3] = (byte)(((responseSize - 24) >> 8) & 0xFF);
            response[2] = 0x2b; // Length 43
            response[3] = 0x00;
            Array.Copy(request, 4, response, 4, 4); // Session Handle
            response[8] = 0x00; // Status (Injecting error)
            response[9] = 0x00;
            response[10] = 0x00;
            response[11] = 0x00;

            Array.Copy(request, 12, response, 12, 8); // Sender Context

            response[20] = 0x00; //Options
            response[21] = 0x00;
            response[22] = 0x00;
            response[23] = 0x00;

            // Command Specific Data
            response[24] = 0x00; // Interface Handle: CIP
            response[25] = 0x00;
            response[26] = 0x00;
            response[27] = 0x00;
            response[28] = 0x00; // Timeout
            response[29] = 0x00;
            response[30] = 0x02; // Item Count: 2
            response[31] = 0x00;
            response[32] = 0xa1; // Connected Address Item
            response[33] = 0x00;
            response[34] = 0x04; // Length
            response[35] = 0x00;

            // Connection ID
            Array.Copy(request, 36, response, 36, 4); // Connection ID

            response[40] = 0xb1; // Connected Data Item
            response[41] = 0x00;
            response[42] = 0x11; // Length 17
            response[43] = 0x00;

            Array.Copy(request, 44, response, 44, 2); // CIP Sequence count

            // CIP
            response[46] = 0xcb; //Response
            response[47] = 0x00;
            response[48] = 0x00; //Success(0x00) (Injecting error 0x15)
            response[49] = 0x00; //Additional Status Size

            //CIP PCCC Object
            response[50] = 0x07; //Requestor ID Length
            response[51] = 0x87; //Vendor ID
            response[52] = 0x03;
            response[53] = 0x39; //CIP Serial No
            response[54] = 0x00;
            response[55] = 0x00;
            response[56] = 0x00;

            // PCCC Response
            response[57] = 0x4f; // Response Code
            response[58] = 0x00; // Success

            Array.Copy(request, 61, response, 59, 2); // Transaction Code

            // Function Specific Response Data

            response[61] = 0x99;
            response[62] = 0x09;
            response[63] = Convert.ToByte(dataByteCount); // Number of bytes after this
            response[64] = 0x42; // File Type Identifier (e.g., Integer)
            //response[64] = 0x94; // File Type Identifier (e.g., Float)

            int dataIndex = 65; // Starting position for the data values
            for (int i = 0; i < pointCount; i++)
            {
                int val = Database.GetValue(fileType, fileNumber, elementNumber + i);
                // Adding the value to the response buffer as two bytes (little-endian)
                response[65] = (byte)(val & 0xFF);        // Lower byte
                response[66] = (byte)((val >> 8) & 0xFF); // Lower byte
            }

            return response;

        }

        private byte[] HandleTypedWrite(byte[] request, int length)
        {
            var writeData = GetWriteData(request, length);
            if (writeData != null)
            {
                Database.UpdateValueFromClient(writeData.FileType, writeData.FileNumber, writeData.ElementNumber, writeData.Value);

                ValueWritten?.Invoke(writeData.FileNumber, writeData.ElementNumber, writeData.FileType, writeData.Value);
            }
                        
            // Constructing response 
            byte[] response = new byte[61];
            response[0] = 0x70; // Command
            response[1] = 0x00;
            response[2] = 0x25; // Length
            response[3] = 0x00;
            Array.Copy(request, 4, response, 4, 4); // Session Handle
            response[8] = 0x00; // Status (Injecting error)
            response[9] = 0x00;
            response[10] = 0x00;
            response[11] = 0x00;

            Array.Copy(request, 12, response, 12, 8); // Sender Context

            response[20] = 0x00; //Options
            response[21] = 0x00;
            response[22] = 0x00;
            response[23] = 0x00;

            // Command Specific Data
            response[24] = 0x00; // Interface Handle: CIP
            response[25] = 0x00;
            response[26] = 0x00;
            response[27] = 0x00;
            response[28] = 0x00; // Timeout
            response[29] = 0x00;
            response[30] = 0x02; // Item Count: 2
            response[31] = 0x00;
            response[32] = 0xa1; // Connected Address Item
            response[33] = 0x00;
            response[34] = 0x04; // Length
            response[35] = 0x00;

            // Connection ID
            response[36] = 0x00;
            response[37] = 0x01;
            response[38] = 0x00;
            response[39] = 0x34;
            response[40] = 0xb1; // Connected Data Item
            response[41] = 0x00;

            response[42] = 0x11; // Length 17
            response[43] = 0x00;

            Array.Copy(request, 44, response, 44, 2); // CIP Sequence count

            // CIP
            response[46] = 0xcb; //Response
            response[47] = 0x00;
            response[48] = 0x00; //Success(0x00) (Injecting error 0x15)
            response[49] = 0x00; //Additional Status Size

            //CIP PCCC Object
            response[50] = 0x07; //Requestor ID Length
            response[51] = 0x87; //Vendor ID
            response[52] = 0x03;
            response[53] = 0x39; //CIP Serial No
            response[54] = 0x00;
            response[55] = 0x00;
            response[56] = 0x00;

            // PCCC Response
            response[57] = 0x4f; // Response Code
            response[58] = 0x00; // Success

            Array.Copy(request, 61, response, 59, 2); // Transaction Code

            return response;
        }

    }
}
