using AB_EIP.Model;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;

namespace AB_EIP.MainViewModel
{
    /// <summary>
    /// View-Model class for handling commands and logs.
    /// </summary>

    public class CommViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public RelayCommand StartCommand { get; }
        public RelayCommand StopCommand { get; }
        public RelayCommand UpdateCommand { get; }
        public RelayCommand UpdateRowCommand { get; }
        public RelayCommand ClearCommand { get; }
        private int _serialno = 0;
        private DataCommunication _selectedItem;
        private ConnectStatusViewModel _connectionStatus;

        TCPViewModel tcpViewModel = new TCPViewModel();             //Using the objects for Start Server permit
        EIPViewModel eipViewModel = new EIPViewModel();    //Using the objects for Start Server permit
        private ObservableCollection<DataCommunication> _commandList = new ObservableCollection<DataCommunication>();
        private static ObservableCollection<DataCommunication> _objectList = new ObservableCollection<DataCommunication>();

        public CommViewModel()
        {
            StartCommand = new RelayCommand(Start, CanStart);
            StopCommand = new RelayCommand(Stop, CanStop);
            UpdateCommand = new RelayCommand(Update, CanUpdate);
            UpdateRowCommand = new RelayCommand(UpdateRow, CanUpdateRow);
            ClearCommand = new RelayCommand(Clear, CanClear);
            CommandHandler.ValueWritten += HandleCommandReceived;
            ConnectionStatus = new ConnectStatusViewModel();
        }

        public ObservableCollection<DataCommunication> CommandList
        {
            get { return _commandList; }
            set
            {
                _commandList = value;
                OnPropertyChanged(nameof(CommandList));
            }
        }

        public static ObservableCollection<DataCommunication> ObjectList
        {
            get { return _objectList; }
            set
            {
                _objectList = value;
                //OnPropertyChanged(nameof(ObjectList));
            }
        }

        public int SerialNo
        {
            get { return _serialno; }
            set
            {
                _serialno = value;
                OnPropertyChanged(nameof(SerialNo));
            }
        }

        public DataCommunication SelectedItem             //For updating the selected row in the datagrid
        {
            get { return _selectedItem; }
            set
            {
                _selectedItem = value;
                OnPropertyChanged(nameof(SelectedItem));
            }
        }

        private void HandleCommandReceived(int fileNumber, int elementNumber, string fileType, int value)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                // Find the item in the grid that corresponds to the address
                var itemToUpdate = ObjectList.FirstOrDefault(d => d.File == fileNumber.ToString() && d.ElementNo == elementNumber.ToString());

                if (itemToUpdate != null)
                {
                    // Update its value. The UI will refresh automatically.
                    itemToUpdate.Value = value.ToString();
                    itemToUpdate.UpdateValue = value;
                }
            });
        }

        //private void ProcessDataArea(string commands, DataCommunication commandInfo)
        //{
        //    int offset = 0;
        //    while (offset < commandInfo.Size)
        //    {
        //        Application.Current.Dispatcher.Invoke(() =>
        //        {
        //            if (commands == "TypedRead")
        //            {
        //                var output = ObjectList.FirstOrDefault(item => item.Commands == commands && item.Index == commandInfo.Index + offset);
        //                if (output != null)
        //                {
        //                    ProcessData(output, commandInfo, ref offset);
        //                }
        //            }

        //            else if (commands == "TypedWrite")
        //            {
        //                var input = ObjectList.FirstOrDefault(item => item.Commands == commands && item.Index == commandInfo.Index + offset);
        //                if (input != null)
        //                {
        //                    ProcessData(input, commandInfo, ref offset);
        //                }
        //            }

        //            else if (commands == "WordRangeWrite")
        //            {
        //                var input = ObjectList.FirstOrDefault(item => item.Commands == commands && item.Index == commandInfo.Index + offset);
        //                if (input != null)
        //                {
        //                    ProcessData(input, commandInfo, ref offset);
        //                }
        //            }

        //            else if (commands == "WriteTwoAddress")
        //            {
        //                var input = ObjectList.FirstOrDefault(item => item.Commands == commands && item.Index == commandInfo.Index + offset);
        //                if (input != null)
        //                {
        //                    ProcessData(input, commandInfo, ref offset);
        //                }
        //            }

        //            else if (commands == "WriteThreeAddress")
        //            {
        //                var input = ObjectList.FirstOrDefault(item => item.Commands == commands && item.Index == commandInfo.Index + offset);
        //                if (input != null)
        //                {
        //                    ProcessData(input, commandInfo, ref offset);
        //                }
        //            }
        //        });
        //    }
        //}

        //private void ProcessData(DataCommunication item, DataCommunication commandInfo, ref int offset)
        //{
        //    switch (item.DataType)
        //    {
        //        case "Int16":
        //            if (offset + 2 > commandInfo.Size) break;
        //            Int16 int16value = BitConverter.ToInt16(commandInfo.DataReceived, offset);
        //            if (BitConverter.IsLittleEndian)
        //            {
        //                int16value = BitConverter.ToInt16(commandInfo.DataReceived.Skip(offset).Take(2).Reverse().ToArray(), 0);
        //            }
        //            item.Value = int16value.ToString();
        //            ConnectionStatus.Information = "Value Updated";
        //            offset += 2;
        //            break;

        //        case "Int32":
        //            if (offset + 4 > commandInfo.Size) break;
        //            int int32value = BitConverter.ToInt32(commandInfo.DataReceived, offset);
        //            if (BitConverter.IsLittleEndian)
        //            {
        //                int32value = BitConverter.ToInt32(commandInfo.DataReceived.Skip(offset).Take(4).Reverse().ToArray(), 0);
        //            }
        //            item.Value = int32value.ToString();
        //            ConnectionStatus.Information = "Value Updated";
        //            offset += 4;
        //            break;

        //        case "Uint32":
        //            if (offset + 4 > commandInfo.Size) break;
        //            uint uint32value = BitConverter.ToUInt32(commandInfo.DataReceived, offset);
        //            if (BitConverter.IsLittleEndian)
        //            {
        //                uint32value = BitConverter.ToUInt32(commandInfo.DataReceived.Skip(offset).Take(4).Reverse().ToArray(), 0);
        //            }
        //            item.Value = uint32value.ToString();
        //            ConnectionStatus.Information = "Value Updated";
        //            offset += 4;
        //            break;

        //        case "Float":
        //            if (offset + 4 > commandInfo.Size) break;
        //            float floatvalue = BitConverter.ToSingle(commandInfo.DataReceived, offset);
        //            if (BitConverter.IsLittleEndian)
        //            {
        //                floatvalue = BitConverter.ToSingle(commandInfo.DataReceived.Skip(offset).Take(4).Reverse().ToArray(), 0);
        //            }
        //            item.Value = floatvalue.ToString();
        //            ConnectionStatus.Information = "Value Updated";
        //            offset += 4;
        //            break;

        //        case "Byte":
        //        case "UByte":
        //            if (offset + 1 > commandInfo.Size) break;
        //            byte byteValue = commandInfo.DataReceived[offset];
        //            item.Value = byteValue.ToString();
        //            ConnectionStatus.Information = "Value Updated";
        //            offset += 1;
        //            break;

        //        case "Bool":
        //            if (offset + 1 > commandInfo.Size) break;
        //            bool boolValue = BitConverter.ToBoolean(commandInfo.DataReceived, offset);
        //            item.Value = boolValue ? "1" : "0";
        //            ConnectionStatus.Information = "Value Updated";
        //            offset += 1;
        //            break;
        //    }
        //}

        public ConnectStatusViewModel ConnectionStatus
        {
            get { return _connectionStatus; }
            set
            {
                _connectionStatus = value;
                OnPropertyChanged(nameof(ConnectionStatus));
            }
        }

        public static void PopulateDataGrid(DataObj dataObject)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                ObjectList.Add(new DataCommunication
                {
                    SerialNo = dataObject.SerialNo,
                    PointName = dataObject.PointName,
                    Description = dataObject.Description,
                    Commands = dataObject.Commands,
                    DataType = dataObject.DataType,
                    FileType = dataObject.FileType,
                    File = dataObject.File,
                    ElementNo = dataObject.Element,
                    Bit = dataObject.BitType,
                    Index = dataObject.IndexValue,
                    Value = dataObject.Value,
                    UpdateValue = dataObject.UpdateValue,
                    DateTime = DateTime.Now,
                });
            });
        }

        /// <summary>
        /// Synchronizes the datagrid row and matches with the Automation.xaml datagrid values
        /// </summary>
        /// <param name="commandInfo"></param>
        public static void SyncDataGrid(DataCommunication commandInfo)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                // Finding the specific rows to update
                var RowsToSync = ObjectList.FirstOrDefault(item => item.Commands == commandInfo.Commands && item.Index == commandInfo.Index && item.ElementNo == commandInfo.ElementNo);

                if (RowsToSync != null)
                {
                    RowsToSync.DateTime = DateTime.Now;
                    RowsToSync.Value = commandInfo.Value;   // If the item is found, update its value
                }
            });
        }

        public static void ClearDataGrid()
        {
            ObjectList.Clear();
        }

        private async void Start()
        {
            try
            {
                await Server.Instance.StartServer();
                Server.Instance.EIPServerRunning = true;
                ConnectionStatus.Information = "Server Started.";
                await ConnectionStatus.ClearInformationAfterDelay(3000);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error starting: {ex.Message}");
            }
        }

        private bool CanStart()
        {
            if (Server.Instance.EIPServerRunning == false && tcpViewModel.CanApply() == true && eipViewModel.CanApply() == true)     //Enabling server start only after successful configuration.
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private async void Stop()
        {
            if (Server.Instance.EIPServerRunning == true)
            {
                try
                {
                    await Server.Instance.StopServer();
                    ConnectionStatus.Information = "Server Stopped";
                    await ConnectionStatus.ClearInformationAfterDelay(3000);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error stopping server: {ex.Message}", "Failed Operation", MessageBoxButton.OK, MessageBoxImage.Error);
                    // Optionally update UI with error information
                }
            }
        }

        private bool CanStop()
        {
            if (Server.Instance.EIPServerRunning == true)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private async void Update()
        {
            foreach (var dataObject in ObjectList)
            {
                Database.UpdateDatabase(dataObject);
                var masterDataObj = DataObjViewModel.DataObjs.FirstOrDefault(d => d.SerialNo == dataObject.SerialNo);
                if (masterDataObj != null)
                {
                    // Update the value in the master object.
                    masterDataObj.Value = dataObject.Value;
                }
                dataObject.DateTime = DateTime.Now;
                ConnectionStatus.Information = "Entire Table Value Updated";
                AutomateViewModel.SyncDataGrid(dataObject);     //Updates the corresponding row in datagrid of Automation.xaml
                await ConnectionStatus.ClearInformationAfterDelay(3000);
            }
        }

        private bool CanUpdate()
        {
            if (Server.Instance.EIPServerRunning == true && ObjectList.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private async void UpdateRow()
        {
            if (SelectedItem != null)
            {
                Database.UpdateDatabase(SelectedItem);
                var masterDataObj = DataObjViewModel.DataObjs.FirstOrDefault(d => d.SerialNo == SelectedItem.SerialNo);
                if (masterDataObj != null)
                {
                    masterDataObj.Value = SelectedItem.Value;
                }
                SelectedItem.DateTime = DateTime.Now;
                ConnectionStatus.Information = "Selected Row Value Updated";
                AutomateViewModel.SyncDataGrid(SelectedItem);       //Updates the corresponding row in datagrid of Automation.xaml
                await ConnectionStatus.ClearInformationAfterDelay(3000);
            }
        }

        private bool CanUpdateRow()
        {
            if (Server.Instance.EIPServerRunning == true && ObjectList.Count > 0 && SelectedItem != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private async void Clear()
        {
            ClearDataGrid();
            ConnectionStatus.Information = "Table Cleared.";
            await ConnectionStatus.ClearInformationAfterDelay(3000);
        }

        private bool CanClear()
        {
            if (ConnectionStatus.StatusText == "Connected")        //Disabling the button when server is connected
            {
                return false;
            }
            else
            {
                return ObjectList.Count > 0;
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
