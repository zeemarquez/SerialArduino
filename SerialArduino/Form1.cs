using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace SerialArduino
{
    public partial class Form1 : Form
    {
        ArduinoSerial arduinoSerial;
        double Tare = 0;
        double CalibrateOutput = 1;
        bool Reading = false;
        int nPorts = 0;
        bool recording = false;

        double config_x0;
        double config_y0;
        double config_x1;
        double config_y1;

        int SampleSize = 10;

        public Form1()
        {
            InitializeComponent();


            groupBox1.Visible = false;
            groupBox2.Visible = false;
            groupBox3.Visible = false;
            groupBox4.Visible = false;
            groupBox5.Visible = false;


            chart1.Series[0].ChartType = SeriesChartType.Line;
            chart1.Series[0].IsVisibleInLegend = false;
            chart1.ChartAreas[0].AxisY.MinorGrid.Enabled = true;
            chart1.ChartAreas[0].AxisY.MinorGrid.Interval = chart1.ChartAreas[0].AxisY.MajorGrid.Interval / 2;
            chart1.ChartAreas[0].AxisY.MinorGrid.LineColor = Color.LightGray;
            chart1.ChartAreas[0].AxisX.Minimum = 0;
            chart1.ChartAreas[0].AxisX.Minimum = 0;
            chart1.ChartAreas[0].AxisX.Enabled = AxisEnabled.False;

            button3.Enabled = false;

            trackBar1.Maximum = 100;
            trackBar1.Minimum = 10;
            trackBar1.Value = 50;
            label2.Text = trackBar1.Value.ToString();

            string[] ports = SerialPort.GetPortNames();
            nPorts = ports.Length;


            comboBox1.Items.AddRange(ports);
            comboBox2.SelectedIndex = 0;

            comboBoxAverage.SelectedIndex = 1;

            if (ports.Contains<string>("COM4"))
            {
                comboBox1.SelectedItem = "COM4";
            }

            button5.Enabled = false;

            config_x0 = (double)Properties.Settings.Default["x0"];
            config_y0 = (double)Properties.Settings.Default["y0"];
            config_x1 = (double)Properties.Settings.Default["x1"];
            config_y1 = (double)Properties.Settings.Default["y1"];



        }


        private void button1_Click(object sender, EventArgs e)
        {
            if (!Reading)
            {
                arduinoSerial = new ArduinoSerial(ref chart1, ref label1, ref labelMax, ref labelMin);
                arduinoSerial.RefreshRate = trackBar1.Value;
                arduinoSerial.PortName = comboBox1.Text;
                arduinoSerial.BaudRate = Convert.ToInt32(comboBox2.Text);
                arduinoSerial.AverageSize = SampleSize;

                arduinoSerial.x0 = config_x0;
                arduinoSerial.y0 = config_y0;
                arduinoSerial.x1 = config_x1;
                arduinoSerial.y1 = config_y1;

                arduinoSerial.StartRead();

                if (arduinoSerial.Connected)
                {
                    Reading = true;
                    button1.Text = "Stop Monitoring";
                }
                else
                {
                    MessageBox.Show("Could not connect to port");
                }
                
            }
            else
            {
                arduinoSerial.StopRead();
                Reading = false;
                button1.Text = "Connect";
            }
            

           
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Tare = arduinoSerial.RawValue;
            button3.Enabled = true;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            CalibrateOutput = Convert.ToDouble(textBox1.Text);
        }

        private void button3_Click(object sender, EventArgs e)
        {
   
            arduinoSerial.Calibrate(Tare, CalibrateOutput);
            button3.Enabled = false;

            Properties.Settings.Default["x0"] = arduinoSerial.x0;
            Properties.Settings.Default["y0"] = arduinoSerial.y0;
            Properties.Settings.Default["x1"] = arduinoSerial.x1;
            Properties.Settings.Default["y1"] = arduinoSerial.y1;
            Properties.Settings.Default.Save();

        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            label2.Text = trackBar1.Value.ToString();
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
          
        }

        private void button4_Click(object sender, EventArgs e)
        {
            button4.Enabled = false;
            button5.Enabled = true;

            arduinoSerial.Record = true;
            button7.Enabled = false;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            button4.Enabled = true;
            button5.Enabled = false;
            button7.Enabled = true;

            arduinoSerial.Record = false;
            List<string> lines = new List<string>();

            lines.Add("Seconds;Value");

            foreach ((double time, double val) i in arduinoSerial.RecordingArray)
            {
                lines.Add( i.time.ToString() + ";" + i.val.ToString());
            }

            SaveFileDialog saveFileDialog1 = new SaveFileDialog();

            saveFileDialog1.Filter = "csv files (*.csv)|*.csv";
            saveFileDialog1.FilterIndex = 1;
            saveFileDialog1.RestoreDirectory = true;

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                StreamWriter writer = new StreamWriter(saveFileDialog1.OpenFile());

                for (int i = 0; i < lines.Count; i++)

                {

                    writer.WriteLine(lines[i].ToString());

                }

                writer.Dispose();
                writer.Close();

            }

        }

        private void button6_Click(object sender, EventArgs e)
        {
            chart1.ChartAreas[0].AxisY.Minimum = double.NaN;
            chart1.ChartAreas[0].AxisY.Maximum = double.NaN;
            chart1.Series[0].Points.Clear();
            

        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {

            int[] chartScales = { -6, -5, -4, -3, -2, -1, 0, 1, 2, 3, 4, 5, 6 };

            int selectedScale = chartScales[comboBox3.SelectedIndex];

            double upper = Math.Pow(10, selectedScale);
            double lower = -1* upper;

            chart1.ChartAreas[0].AxisY.Minimum = lower;
            chart1.ChartAreas[0].AxisY.Maximum = upper;

        }

        private void button7_Click(object sender, EventArgs e)
        {
            arduinoSerial.SetZeroOffset();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.Filter = "csv files (*.csv)|*.csv";
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;
            openFileDialog.Multiselect = false;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                StreamReader stream = new StreamReader(openFileDialog.OpenFile());

                string read = stream.ReadToEnd().Replace("\r", String.Empty);

                OpenData.FileRead = read.Split('\n');

                FormOpenData formOpenData = new FormOpenData();
                formOpenData.Show();
            }


        }

        private void Form1_Load(object sender, EventArgs e)
        {
            groupBox1.Visible = true;
            groupBox2.Visible = true;
            groupBox3.Visible = true;
            groupBox4.Visible = true;
            groupBox5.Visible = true;
        }

        private void comboBoxAverage_SelectedIndexChanged(object sender, EventArgs e)
        {
            SampleSize = Convert.ToInt32(comboBoxAverage.SelectedItem);

            if (Reading)
            {
                arduinoSerial.AverageSize = SampleSize;
            }
            
        }
    }
}
