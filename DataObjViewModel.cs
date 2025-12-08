using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using AB_EIP.Model;
using AB_EIP.View;

namespace AB_EIP.MainViewModel
{
    /// <summary>
    /// View-Model class for DataObj class.
    /// </summary>

    public class DataObjViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private int _serialno = 0;
        private DataObj _selectedItem;
        private readonly ConfigManager _configManager;      //For export/import of datagrid

        public RelayCommand AddRowCommand { get; }
        public RelayCommand DeleteRowCommand { get; }
        public RelayCommand ApplyCommand { get; }
        public RelayCommand ImportCommand { get; }
        public RelayCommand ExportCommand { get; }
        public RelayCommand ClearCommand { get; }
        public RelayCommand DatabaseCommand { get; }

        public static ObservableCollection<DataObj> DataObjs { get; set; }

        private ConnectStatusViewModel _connectionStatus;

        public ObservableCollection<string> CommandsItems { get; } = new ObservableCollection<string>
        {
           "TypedRead", "TypedWrite", "WordRangeWrite", "WriteTwoAddress", "WriteThreeAddress"
        };

        public ObservableCollection<string> DataTypeItems { get; } = new ObservableCollection<string>
        {
           "Byte", "Bool", "Float", "Int16", "Int32","UByte", "Uint32"
        };

        public ObservableCollection<string> FileTypeItems { get; } = new ObservableCollection<string>
        {
           "Output (O)", "Input (I)", "Status (S)", "Binary (B)", "Timer (T)", "Counter (C)", "Time (T)", "Control (R)", "Integer (N)", "Float (F)", "ASCII (A)", "PD (PD)"
        };

        public ObservableCollection<string> BitItems { get; } = new ObservableCollection<string>
        {
           "NONE", "EN", "TT", "DN", "ACC", "PRE", "CU", "CD", "OV","UN", "EU", "EM", "ER", "UL", "IN", "FD", "CT", "CL", "PVT", "DO", "SWM", "CA", "MO", "PE", "INI", "SPOR", "OLL",
           "OLH", "EWD", "DVNA", "DVPA", "PVLA", "PVHA", "SP", "KP", "KI", "KD", "BIAS", "MAXS", "MINS", "DB", "SO", "MAXO", "MINO", "UPD", "PV", "ERR", "OUT", "PVH", "PVL",
           "DVP", "DVN", "PVDB", "DVDB", "MAXI", "MINI", "TIE", "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15"
        };    

        public DataObjViewModel()
        {
            AddRowCommand = new RelayCommand(AddRow, CanAddRow);
            DeleteRowCommand = new RelayCommand(DeleteRow, CanDeleteRow);
            ApplyCommand = new RelayCommand(Apply, CanApply);
            ImportCommand = new RelayCommand(Import, CanImport);
            ExportCommand = new RelayCommand(Export, CanExport);
            ClearCommand = new RelayCommand(Clear, CanClear);
            DatabaseCommand = new RelayCommand(CreateDatabase, CanCreateDatabase);
            DataObjs = new ObservableCollection<DataObj>();
            ConnectionStatus = new ConnectStatusViewModel();
            _configManager = new ConfigManager("DataGrid.xml");       //Initializing with default file name

        }

        public DataObj SelectedItem             //For deleting the selected row in the datagrid
        {
            get { return _selectedItem; }
            set
            {
                _selectedItem = value;
                OnPropertyChanged(nameof(SelectedItem));
            }
        }

        public ConnectStatusViewModel ConnectionStatus
        {
            get { return _connectionStatus; }
            set
            {
                _connectionStatus = value;
                OnPropertyChanged(nameof(ConnectionStatus));
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

        private void AddRow()                   //Adds new row in datagrid
        {
            if (DataObjs.Count == 0)
            {
                SerialNo = 1;                   //If no row exist in datagrid, serial no=1
            }
            else
            {
                SerialNo = DataObjs.Max(obj => obj.SerialNo) + 1;   //Checking the max value of serial no in existing row and incrementing the new row serialno by 1
            }
            DataObjs.Add(new DataObj()
            {
                SerialNo = SerialNo,
            });
        }

        private bool CanAddRow()
        {
            if (ConnectionStatus.StatusText == "Connected")     //Disabling the button when server is connected
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private void DeleteRow()                //Deletes selected row in datagrid
        {
            if (SelectedItem != null)
            {
                DataObjs.Remove(SelectedItem);
            }
        }

        private bool CanDeleteRow()
        {
            if (ConnectionStatus.StatusText == "Connected")      //Disabling the button when server is connected
            {
                return false;
            }
            else
            {
                return SelectedItem != null;
            }

        }

        private void Apply()                    //Creates database
        {
            CommViewModel.ClearDataGrid();
            AutomateViewModel.ClearDataGrid();
            foreach (var dataObj in DataObjs)
            {
                Database.CreateDatabase(dataObj);
                CommViewModel.PopulateDataGrid(dataObj);
                AutomateViewModel.PopulateDataGrid(dataObj);
                ConnectionStatus.Information = "Database Created";
            }
        }

        private bool CanApply()
        {
            if (ConnectionStatus.StatusText == "Connected")        //Disabling the button when server is connected
            {
                return false;
            }
            else
            {
                return DataObjs.Count > 0;
            }

        }

        private async void Import()             //Imports XML database 
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();       // Prompt user to choose import file location
                openFileDialog.Filter = "XML files (*.xml)|*.xml";
                if (openFileDialog.ShowDialog() == true)
                {
                    string filePath = openFileDialog.FileName;

                    if (!XmlValidator.ValidateXmlAgainstSchema(filePath, "EIPDatabaseSchema.xsd"))       //Validating the import file against database schema.
                    {
                        MessageBox.Show("Invalid or corrupt database. Import aborted.", "Import Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        //error.ErrorLog("Invalid or corrupt database. Import aborted");
                        return;
                    }

                    DataObjs.Clear();
                    ConnectionStatus.Information = "Importing data....";

                    var importedData = await Task.Run(() => _configManager.LoadConfigFromFile(filePath));       // Load and process the data in a background thread for faster import

                    var existingPointNames = new HashSet<string>(DataObjs.Select(x => x.PointName));            // Using HashSets for fast lookup
                    var existingIndexes = new HashSet<(string, string)>(DataObjs.Select(x => (x.FileType, x.Index)));

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        foreach (var item in importedData.EIPDataObjs)
                        {
                            if (!existingPointNames.Contains(item.PointName) && !existingIndexes.Contains((item.FileType, item.Index)))
                            {
                                DataObjs.Add(item);
                                existingPointNames.Add(item.PointName);
                                existingIndexes.Add((item.FileType, item.Index));
                            }
                            else
                            {
                                MessageBox.Show($"Duplicate found. Skipping {item.PointName}.", "Import Warning", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                                return;
                            }
                        }
                    });
                }
                ConnectionStatus.Information = "Data import successful";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error importing data", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                //error.ErrorLog(ex);
            }
        }

        private bool CanImport()
        {
            if (ConnectionStatus.StatusText == "Connected")        //Disabling the button when server is connected
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private void Export()                   //Exports XML database
        {
            try
            {
                ConfigData configData = new ConfigData
                {
                    EIPDataObjs = new List<DataObj>(DataObjs)
                };
                string fileName = "DataGridExport.xml";         //Default file name for export
                _configManager.SaveConfig(configData, fileName);

                ConnectionStatus.Information = "Data export successful";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanExport()
        {
            return DataObjs.Count > 0;
        }

        private void Clear()                    //Clears datagrid and removes all objects
        {
            DataObjs.Clear();
        }

        private bool CanClear()
        {
            if (ConnectionStatus.StatusText == "Connected")     //Disabling the button when server is connected
            {
                return false;
            }
            else
            {
                return DataObjs.Count > 0;
            }
        }

        public void CreateDatabase()            //Creates custom database
        {
            CustomDatabase customDatabaseWindow = new CustomDatabase();
            customDatabaseWindow.Show();
        }

        private bool CanCreateDatabase()
        {
            return true;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
