using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace AB_EIP.Model
{
    /// <summary>
    /// Class for displaying AB EIP object communication on Communication.xaml & Automation.xaml datagrid.
    /// </summary>

    public class DataCommunication : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _value;
        private double _updateValue;
        private string _commands;
        private string _start;
        private string _end;
        private string _step;
        private double _startPoint;
        private double _endPoint;
        private double _stepPoint;
        private DateTime _dateTime;
        private string _outputType;

        public int SerialNo { get; set; }
        public ushort Index { get; set; }
        public string DataType { get; set; }
        public string FileType { get; set; }
        public string FileNo { get; set; }
        public string ElementNo { get; set; }
        public string File { get; set; }
        public string Bit { get; set; }
        public string PointName { get; set; }
        public string Description { get; set; }
        public string OperationType { get; set; }
        public string OnTime { get; set; }
        public string OffTime { get; set; }
        public string Frequency { get; set; }
        public byte[] DataReceived { get; set; }
        public int DataSize { get; set; }
        public int Size { get; set; }

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
                    MessageBox.Show("Invalid input.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public double UpdateValue
        {
            get { return _updateValue; }
            set
            {
                _updateValue = value;
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

        public string Start
        {
            get { return _start; }
            set
            {
                if (double.TryParse(value, out double val))
                {
                    _start = value;
                    _startPoint = val;
                    OnPropertyChanged();
                }
                else
                {
                    // Handle invalid input
                    MessageBox.Show("Invalid input.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public string End
        {
            get { return _end; }
            set
            {
                if (double.TryParse(value, out double val))
                {
                    _end = value;
                    _endPoint = val;
                    OnPropertyChanged();
                }
                else
                {
                    // Handle invalid input
                    MessageBox.Show("Invalid input.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public string Step
        {
            get { return _step; }
            set
            {
                if (double.TryParse(value, out double val))
                {
                    _step = value;
                    _stepPoint = val;
                    OnPropertyChanged();
                }
                else
                {
                    // Handle invalid input
                    MessageBox.Show("Invalid input.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public double StartPoint
        {
            get { return _startPoint; }
            set
            {
                _startPoint = value;
                OnPropertyChanged();
            }
        }

        public double EndPoint
        {
            get { return _endPoint; }
            set
            {
                _endPoint = value;
                OnPropertyChanged();
            }
        }

        public double StepPoint
        {
            get { return _stepPoint; }
            set
            {
                _stepPoint = value;
                OnPropertyChanged();
            }
        }

        public DateTime DateTime
        {
            get { return _dateTime; }
            set
            {
                _dateTime = value;
                OnPropertyChanged();
            }
        }

        public string OutputType
        {
            get { return _outputType; }
            set
            {
                _outputType = value;
                OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
