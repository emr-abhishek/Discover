using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using AB_EIP.MainViewModel;

namespace AB_EIP.Model
{
    public class Automate
    {
        private readonly DispatcherTimer _timer;
        private static bool _binaryState = false;
        private static readonly Random _random = new Random();

        public Automate()
        {
            _timer = new DispatcherTimer();
        }

        public void StartAutomation(DataCommunication dataObject)
        {
            int frequency = int.Parse(dataObject.Frequency.Split(' ')[0]);
            _timer.Interval = TimeSpan.FromSeconds(frequency);
            _timer.Tick += (sender, e) => Timer_Tick(sender, e, dataObject);
            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e, DataCommunication dataObject)
        {
            double value = 0;
            string outputType = dataObject.OutputType;
            switch (outputType)
            {
                case "RandomValue":
                    value = AutoValueGenerator(dataObject);
                    break;

                case "ToggleValue":
                    value = double.Parse(ToggleValueGenerator(dataObject));
                    break;

                default:
                    throw new NotSupportedException($"Output type {outputType} is not supported.");
            }
            dataObject.Value = value.ToString();
            CommViewModel.SyncDataGrid(dataObject);     //Updates the corresponding row in datagrid of Communication.xaml
            Database.UpdateDatabase(dataObject);
        }

        public void StopAutomation()
        {
            _timer.Stop();
        }

        public static double AutoValueGenerator(DataCommunication dataObject)
        {
            var randomValue = _random.NextDouble() * (dataObject.EndPoint - dataObject.StartPoint) + dataObject.StartPoint;
            return randomValue;
        }

        public static double SineWaveGenerator(DataCommunication dataObject)
        {
            double startPoint = dataObject.StartPoint;
            double endPoint = dataObject.EndPoint;
            int frequencySeconds = int.Parse(dataObject.Frequency.Split(' ')[0]);

            double time = DateTime.Now.TimeOfDay.TotalSeconds;
            double angularFrequency = 2 * Math.PI / frequencySeconds;

            // Calculate the sine value directly without considering phase shift
            double sineValue = Math.Sin(angularFrequency * time);

            // Scale the sine value to fit between start and end points
            double amplitude = Math.Abs(endPoint - startPoint);
            double scaledValue = startPoint + (sineValue + 1) * (amplitude / 2) + dataObject.StartPoint;

            return scaledValue;
        }

        public static double IncrementValueGenerator(DataCommunication dataObject)
        {
            double IntialStartPointValue = dataObject.StartPoint;
            double IncrementValue;

            if (dataObject.StartPoint < dataObject.EndPoint)
            {
                IncrementValue = dataObject.StartPoint;
                dataObject.StartPoint += dataObject.StepPoint;
                return IncrementValue;
            }
            else
            {
                return IntialStartPointValue;
            }
        }

        public static double DecrementValueGenerator(DataCommunication dataObject)
        {
            double IntialEndPointValue = dataObject.EndPoint;
            double DecrementValue;

            if (dataObject.StartPoint > dataObject.EndPoint)
            {
                DecrementValue = dataObject.StartPoint;
                dataObject.StartPoint -= dataObject.StepPoint;
                return DecrementValue;
            }
            else
            {
                return IntialEndPointValue;
            }

        }

        public static double TriangleWaveGenerator(DataCommunication dataObject)
        {
            if (dataObject.StartPoint < dataObject.EndPoint)
            {
                double IncrementValue = IncrementValueGenerator(dataObject);
                return IncrementValue;
            }
            else
            {
                double DecrementValue = DecrementValueGenerator(dataObject);
                return DecrementValue;
            }
        }

        public static string ToggleValueGenerator(DataCommunication dataObject)
        {
            _binaryState = !_binaryState;
            string value = _binaryState ? "1" : "0";
            return value;
        }

    }
}
