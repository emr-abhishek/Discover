using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace AB_EIP.Model
{
    /// <summary>
    /// Data Object class with parameters for database creation and update
    /// </summary>

    public class DataObj : INotifyPropertyChanged
    {
        private int _serialNo;
        private string _dataType;
        private string _fileType;
        private string _bitType;
        private string _element;
        private int _elementNo;
        private string _file;
        private int _fileNo;
        private string _index;
        private ushort _indexValue;
        private string _deadband;
        private double _deadbandValue;
        private string _value;
        private double _updateValue;
        private bool _isSelected;
        private string _pointName;
        private string _description;
        private string _commands;

        public event PropertyChangedEventHandler PropertyChanged;

        public int SerialNo
        {
            get { return _serialNo; }
            set
            {
                _serialNo = value;
                OnPropertyChanged();
            }
        }

        public string DataType
        {
            get { return _dataType; }
            set
            {
                if (_dataType != value)
                {
                    _dataType = value;
                    OnPropertyChanged(nameof(DataType));
                }
            }
        }

        public string FileType
        {
            get { return _fileType; }
            set
            {
                _fileType = value;
                OnPropertyChanged();
            }
        }

        public string BitType
        {
            get { return _bitType; }
            set
            {
                _bitType = value;
                OnPropertyChanged();
            }
        }

        public string Element
        {
            get { return _element; }
            set
            {
                if (int.TryParse(value, out int _eNo))
                {
                    _element = value;
                    _elementNo = _eNo;
                    OnPropertyChanged();
                }
                else
                {
                    // Handle invalid input
                    MessageBox.Show("Invalid input for AreaNo.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        //Using [XmlIgnore] to exclude properties which are not needed for XML serialization and hence will not be exported.
        [XmlIgnore]
        public int ElementNo
        {
            get { return _elementNo; }
            set
            {
                _elementNo = value;
                OnPropertyChanged();
            }
        }

        public string File
        {
            get { return _file; }
            set
            {
                if (int.TryParse(value, out int _fNo))
                {
                    _file = value;
                    _fileNo = _fNo;
                    OnPropertyChanged();
                }
                else
                {
                    // Handle invalid input
                    MessageBox.Show("Invalid input for AreaNo.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        //Using [XmlIgnore] to exclude properties which are not needed for XML serialization and hence will not be exported.
        [XmlIgnore]
        public int FileNo
        {
            get { return _fileNo; }
            set
            {
                _fileNo = value;
                OnPropertyChanged();
            }
        }

        public string Index
        {
            get { return _index; }
            set
            {
                if (ushort.TryParse(value, out ushort idx))
                {
                    _index = value;
                    _indexValue = idx;
                    OnPropertyChanged();
                }
                else
                {
                    // Handle invalid input
                    MessageBox.Show("Invalid input for Index.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        //Using [XmlIgnore] to exclude properties which are not needed for XML serialization and hence will not be exported.
        [XmlIgnore]
        public ushort IndexValue
        {
            get { return _indexValue; }
            set
            {
                _indexValue = value;
                OnPropertyChanged();
            }
        }

        public string Deadband
        {
            get { return _deadband; }
            set
            {
                if (double.TryParse(value, out double deadbnd))
                {
                    _deadband = value;
                    _deadbandValue = deadbnd;
                    OnPropertyChanged();
                }
                else
                {
                    // Handle invalid input
                    MessageBox.Show("Invalid input for Deadband.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        [XmlIgnore]
        public double DeadbandValue
        {
            get { return _deadbandValue; }
            set
            {
                _deadbandValue = value;
                OnPropertyChanged();
            }
        }

        public string Value
        {
            get { return _value; }
            set
            {
                if (double.TryParse(value, out double val))
                {
                    _value = value;
                    _updateValue = val;
                    OnPropertyChanged();
                }
                else
                {
                    // Handle invalid input
                    MessageBox.Show("Invalid Input.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        [XmlIgnore]
        public double UpdateValue
        {
            get { return _updateValue; }
            set
            {
                _updateValue = value;
                OnPropertyChanged();
            }
        }

        [XmlIgnore]
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                OnPropertyChanged();
            }
        }

        public string PointName
        {
            get { return _pointName; }
            set
            {
                _pointName = value;
                OnPropertyChanged();
            }
        }

        public string Description
        {
            get { return _description; }
            set
            {
                _description = value;
                OnPropertyChanged();
            }
        }

        public string Commands
        {
            get { return _commands; }
            set
            {
                _commands = value;
                OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
