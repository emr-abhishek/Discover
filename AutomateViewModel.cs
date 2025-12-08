using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Xml.Linq;
using AB_EIP.Model;

namespace AB_EIP.MainViewModel
{
    public class AutomateViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public RelayCommand AutomateCommand { get; }
        public RelayCommand StopAutomateCommand { get; }
        public RelayCommand UpdateRowCommand { get; }
        public RelayCommand ClearCommand { get; }
        private DataCommunication _selectedItem;
        private ConnectStatusViewModel _connectionStatus;
        private static ObservableCollection<DataCommunication> _objectList = new ObservableCollection<DataCommunication>();
        private readonly Automate _automate;
        private bool _automationrunning = false;

        public AutomateViewModel()
        {
            AutomateCommand = new RelayCommand(AutomateResponse, CanAutomateResponse);
            ClearCommand = new RelayCommand(Clear, CanClear);
            StopAutomateCommand = new RelayCommand(StopAutomateResponse, CanStopAutomateResponse);
            UpdateRowCommand = new RelayCommand(UpdateRow, CanUpdateRow);
            ConnectionStatus = new ConnectStatusViewModel();
            _automate = new Automate();
        }

        public ObservableCollection<string> OutputType { get; } = new ObservableCollection<string>
        {
           " ", "RandomValue", "SineWave", "SquareWave", "Increment", "Decrement", "TriangleWave", "ToggleValue"
        };

        public ObservableCollection<string> Frequency { get; } = new ObservableCollection<string>
        {
           " ", "5 Seconds", "10 Seconds", "15 Seconds", "30 Seconds", "60 Seconds", "90 Seconds", "120 Seconds"
        };

        public ConnectStatusViewModel ConnectionStatus
        {
            get { return _connectionStatus; }
            set
            {
                _connectionStatus = value;
                OnPropertyChanged(nameof(ConnectionStatus));
            }
        }

        public static ObservableCollection<DataCommunication> ObjectList
        {
            get { return _objectList; }
            set
            {
                _objectList = value;
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

        public static void PopulateDataGrid(DataObj dataObject)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                ObjectList.Add(new DataCommunication
                {
                    SerialNo = dataObject.SerialNo,
                    PointName = dataObject.PointName,
                    Description = dataObject.Description,
                    DataType = dataObject.DataType,
                    Commands = dataObject.Commands,
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
        /// Synchronizes the datagrid row values and matches with the Communication.xaml datagrid values
        /// </summary>
        /// <param name="commandInfo"></param>
        public static void SyncDataGrid(DataCommunication commandInfo)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                // Finding the specific rows to update
                var RowsToSync = ObjectList.FirstOrDefault(item => item.Commands == commandInfo.Commands && item.FileNo == commandInfo.FileNo && item.ElementNo == commandInfo.ElementNo);

                if (RowsToSync != null)
                {
                    RowsToSync.DateTime = DateTime.Now;
                    RowsToSync.Value = commandInfo.Value;       // If the item is found, update its value
                }
            });
        }

        public bool CanAutomateResponse()
        {
            if (_automationrunning == false && ConnectionStatus.StatusText == "Connected")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void AutomateResponse()
        {
            _automationrunning = true;
            foreach (var dataObject in ObjectList)
            {
                if (!string.IsNullOrEmpty(dataObject.Frequency))
                {
                    _automate.StartAutomation(dataObject);
                }
                CommViewModel.SyncDataGrid(dataObject);
            }
            ConnectionStatus.Information = "Automation Started";
        }

        public bool CanStopAutomateResponse()
        {
            if (_automationrunning == true)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void StopAutomateResponse()
        {
            _automate.StopAutomation();
            _automationrunning = false;
            ConnectionStatus.Information = "Automation Stopped";
        }

        private async void UpdateRow()
        {
            if (SelectedItem != null)
            {
                Database.UpdateDatabase(SelectedItem);
                SelectedItem.DateTime = DateTime.Now;
                ConnectionStatus.Information = "Selected Row Value Updated";
                CommViewModel.SyncDataGrid(SelectedItem);      //Updates the corresponding row in datagrid of Communication.xaml
                await ConnectionStatus.ClearInformationAfterDelay(3000);
            }
        }

        private bool CanUpdateRow()
        {
            if (_automationrunning == false && ObjectList.Count > 0 && SelectedItem != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void Clear()
        {
            ClearDataGrid();
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

        public static void ClearDataGrid()
        {
            ObjectList.Clear();
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
