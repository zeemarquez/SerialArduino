using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;


namespace SerialArduino
{
    public class ArduinoSerial
    {
        public SerialPort SerialPort;
        public Chart SerialChart;
        public Label ValueLabel;
        public Label MaxLabel;
        public Label MinLabel;

        public int RefreshRate = 50;
        public string PortName = "COM4";
        public int BaudRate = 9600;
        public int AxisMseconds = 5000;
        public double RawValue;
        public double Value;
        public bool Connected = false;

        public bool Read = false;

        public List<(double time, double Val)> RecordingArray = new List<(double time, double Val)>();
        public bool Record = false;
        private bool recording = false;


        public double x0 = 0;
        public double x1 = 1;
        public double y0 = 0;
        public double y1 = 1;

        public int AverageSize = 10;
        public int MaxSize = 110;

        public List<double> ValuesArray = new List<double>();

        private DateTime TimeStartRecord;


        public ArduinoSerial(ref Chart chart, ref Label label, ref Label maxlabel, ref Label minlabel)
        {
            SerialPort = new SerialPort();
            SerialChart = chart;
            ValueLabel = label;
            MaxLabel = maxlabel;
            MinLabel = minlabel;

            for (int i = 0; i < MaxSize; i++)
            {
                ValuesArray.Add(0.0);
            }
        }

        public bool OpenSerialPort()
        {
            SerialPort.PortName = PortName;
            SerialPort.BaudRate = BaudRate;

            bool output = false;
            try
            {
                SerialPort.Open();
                output = true;
                Connected = true;
            }
            catch
            {
                output = false;
            }

            return output;
        }

        public void StopRead()
        {
            Read = false;
            SerialPort.Close();
            SerialChart.Invoke((MethodInvoker)(() => SerialChart.Series[0].Points.Clear()));
        }

        public async void StartRead()
        {
            OpenSerialPort();
            if (Connected)
            {
                Read = true;
                await Task.Run(() =>
                {
                    int n = 0;

                    while (Read)
                    {
                        if (SerialPort.IsOpen)
                        {
                            string input = SerialPort.ReadExisting();
                            double val = getSerialRead(input);
                            RawValue = val;
                            Value = mapFunction(val);

                            //Console.WriteLine(Value.ToString());
                            updateChart((double)n, Value);
                            n = n + RefreshRate;
                        }
                        else
                        {
                            OpenSerialPort();
                        }



                        Thread.Sleep(RefreshRate);
                    }
                });
            }
            else
            {
                Console.WriteLine("Port not connected");
            }
            
        }

        private void updateChart(double x, double y)
        {

            if (Record)
            {
                if (!recording)
                {
                    TimeStartRecord = DateTime.Now;
                    RecordingArray.Clear();
                    recording = true;
                }

                TimeSpan time = DateTime.Now - TimeStartRecord;
                RecordingArray.Add((time.TotalSeconds,y));

            }
            else if (recording)
            {
                recording = false;
            }

            ValuesArray.Insert(0, y);
            ValuesArray.RemoveAt(ValuesArray.Count - 1);

            double[] sample = ValuesArray.GetRange(0, AverageSize).ToArray();
            double average =  sample.Sum()/(double)AverageSize;
            double max = sample.Max();
            double min = sample.Min();
            Console.WriteLine();
            ValueLabel.Invoke((MethodInvoker)(() => ValueLabel.Text = Math.Round(average,3).ToString()));
            MaxLabel.Invoke((MethodInvoker)(() => MaxLabel.Text = Math.Round(max, 3).ToString()));
            MinLabel.Invoke((MethodInvoker)(() => MinLabel.Text = Math.Round(min, 3).ToString()));

            if (x > AxisMseconds)
            {
                //SerialChart.Invoke((MethodInvoker)(() => SerialChart.Series[0].Points.RemoveAt(0)));
                SerialChart.Invoke((MethodInvoker)(() => SerialChart.Series[0].Points.AddXY(x, y)));
                SerialChart.Invoke((MethodInvoker)(() => SerialChart.ChartAreas[0].AxisX.Minimum = x-AxisMseconds));
            }
            else
            {
                SerialChart.Invoke((MethodInvoker)(() => SerialChart.Series[0].Points.AddXY(x, y)));
            }
            

        }
        private bool isValid(string input)
        {
            bool result = false;

            if (input.Length > 2)
            {
                string start = input.Substring(0, 1);
                string end = input.Substring(input.Length - 1, 1);
                string val = input.Substring(1, input.Length - 2);

                if ((start == "*") & (end == "|"))
                {
                    try
                    {
                        Convert.ToDouble(val);
                        result = true;
                    }
                    catch
                    {
                        result = false;
                    }
                }
                else
                {
                    result = false;
                }
            }
            else
            {
                result = false;
            }



            return result;
        }
        private double getSerialRead(string serialRead)
        {
            double result = double.NaN;

            string[] lines = serialRead.Split('\n');

            for (int i = lines.Length - 1; i >= 0; i--)
            {
                if (lines[i].Length > 2)
                {
                    string line = lines[i].Remove(lines[i].Length - 1);

                    if (isValid(line))
                    {
                        result = Convert.ToDouble(line.Substring(1, line.Length - 2));
                        break;
                    }

                }

            }



            return result;
        }

        private double mapFunction(double x)
        {
            return ((y1 - y0) / (x1 - x0)) * x - ((y1 - y0) / (x1 - x0)) * x0 + y0;
        }

        public void Calibrate(double tare, double output)
        {
            x0 = tare;
            x1 = RawValue;
            y1 = output;

            SerialChart.Invoke((MethodInvoker)(() => SerialChart.Series[0].Points.Clear()));
            /*
            SerialChart.Invoke((MethodInvoker)(() => SerialChart.ChartAreas[0].AxisY. = 0.0 ));
            SerialChart.Invoke((MethodInvoker)(() => SerialChart.ChartAreas[0].AxisY.Maximum = output));
            */

        }

        public void Calibrate(double x0, double y0, double x1, double y1)
        {
            this.x0 = x0;
            this.y0 = y0;
            this.x1 = x1;
            this.y1 = y1;

            SerialChart.Invoke((MethodInvoker)(() => SerialChart.Series[0].Points.Clear()));
            /*
            SerialChart.Invoke((MethodInvoker)(() => SerialChart.ChartAreas[0].AxisY. = 0.0 ));
            SerialChart.Invoke((MethodInvoker)(() => SerialChart.ChartAreas[0].AxisY.Maximum = output));
            */

        }

        public void SetZeroOffset()
        {
            this.y0 = this.y0 - Value;

            SerialChart.Invoke((MethodInvoker)(() => SerialChart.Series[0].Points.Clear()));
            /*
            SerialChart.Invoke((MethodInvoker)(() => SerialChart.ChartAreas[0].AxisY. = 0.0 ));
            SerialChart.Invoke((MethodInvoker)(() => SerialChart.ChartAreas[0].AxisY.Maximum = output));
            */

        }
    }

}
