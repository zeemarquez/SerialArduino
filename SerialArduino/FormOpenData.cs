using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace SerialArduino
{
    public partial class FormOpenData : Form
    {
        string[] fileRead;
        List<double> xArray;
        List<double> yArray;

        double xminval = double.NaN;
        double xmaxval = double.NaN;
        double yminval = double.NaN;
        double ymaxval = double.NaN;

        public FormOpenData()
        {
            InitializeComponent();
        }

        private void FormOpenData_Load(object sender, EventArgs e)
        {
            fileRead = OpenData.FileRead;

            xArray = new List<double>();
            yArray = new List<double>();

            chart1.Series[0].ChartType = SeriesChartType.Line;
            chart1.Series[0].IsVisibleInLegend = false;
            chart1.ChartAreas[0].AxisY.MinorGrid.Enabled = true;
            chart1.ChartAreas[0].AxisY.MinorGrid.Interval = chart1.ChartAreas[0].AxisY.MajorGrid.Interval / 2;
            chart1.ChartAreas[0].AxisY.MinorGrid.LineColor = Color.LightGray;
            chart1.ChartAreas[0].AxisX.Minimum = 0;
            //chart1.ChartAreas[0].AxisX.Enabled = AxisEnabled.False;

            for (int i = 1; i < fileRead.Length - 1; i++)
            {
                string[] row = fileRead[i].Split(';');

                double x = Convert.ToDouble(row[0]);
                double y = Convert.ToDouble(row[1]);

                xArray.Add(x);
                yArray.Add(y);

                chart1.Series[0].Points.AddXY(x, y);
            }

        }

        private bool validInput(string input)
        {
            bool result = false;
            try
            {
                double a = Convert.ToDouble(input);
                result = true;
            }
            catch
            {
                result = false;
            }

            return result;
        }

        private void xmin_TextChanged(object sender, EventArgs e)
        {

            if (validInput(xmin.Text))
            {
                xminval = Convert.ToDouble(xmin.Text);
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if ((xminval != double.NaN)&(xmaxval != double.NaN))
            {
                if (xminval < xmaxval)
                {
                    chart1.ChartAreas[0].AxisX.Minimum = xminval;
                    chart1.ChartAreas[0].AxisX.Maximum = xmaxval;
                }
            }
            
            
        }

        private void xmax_TextChanged(object sender, EventArgs e)
        {
            if (validInput(xmax.Text))
            {
                xmaxval = Convert.ToDouble(xmax.Text);
            }
        }

        private void ymin_TextChanged(object sender, EventArgs e)
        {
            if (validInput(ymin.Text))
            {
                yminval = Convert.ToDouble(ymin.Text);
            }

        }

        private void ymax_TextChanged(object sender, EventArgs e)
        {
            if (validInput(ymax.Text))
            {
                ymaxval = Convert.ToDouble(ymax.Text);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if ((yminval != double.NaN) & (yminval != double.NaN))
            {
                if (yminval < ymaxval)
                {
                    chart1.ChartAreas[0].AxisY.Minimum = yminval;
                    chart1.ChartAreas[0].AxisY.Maximum = ymaxval;
                }
            }
        }

        private void title_TextChanged(object sender, EventArgs e)
        {
            Title chartTitle = new Title();
            chartTitle.Font = new Font(DefaultFont.FontFamily, 16, FontStyle.Regular);
            chartTitle.Text = title.Text;

            chart1.Titles.Clear();
            chart1.Titles.Add(chartTitle);
        }

        private void yaxis_TextChanged(object sender, EventArgs e)
        {
            chart1.ChartAreas[0].AxisY.Title = yaxis.Text;
            chart1.ChartAreas[0].AxisY.TitleFont = new Font(DefaultFont.FontFamily, 12, FontStyle.Regular);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            chart1.ChartAreas[0].AxisY.Minimum = double.NaN;
            chart1.ChartAreas[0].AxisY.Maximum = double.NaN;
            chart1.ChartAreas[0].AxisX.Minimum = double.NaN;
            chart1.ChartAreas[0].AxisX.Maximum = double.NaN;

            chart1.ChartAreas[0].RecalculateAxesScale();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();

            saveFileDialog.Filter = "png files (*.png)|*.png";
            saveFileDialog.FilterIndex = 1;
            saveFileDialog.RestoreDirectory = true;

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {

                chart1.SaveImage(saveFileDialog.FileName, ChartImageFormat.Png);
            }
        }
    }
}
