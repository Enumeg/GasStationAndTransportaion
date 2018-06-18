using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Source;

namespace haies
{
    /// <summary>
    /// Interaction logic for GaspricesChart.xaml
    /// </summary>
    public partial class GaspricesChart : Window
    {
        public GaspricesChart()
        {
            InitializeComponent();
        }

        private void Report1()
        {
            try
            {
                var chart = wfh.Child as Chart;
                chart.Series.Clear();
                chart.ChartAreas.Clear();
                ChartArea chartArea1 = new ChartArea("ChartArea1");
                chart.ChartAreas.Add(chartArea1);
                chart.ChartAreas[0].AxisX.Title = "التاريخ";
                chart.ChartAreas[0].AxisY.Title = "القيمة";
                chart.ChartAreas[0].AxisX.TitleFont = chart.ChartAreas[0].AxisY.TitleFont = new System.Drawing.Font("Arial", 14);
                Series series1 = new Series("الإيرادات");
                series1.ChartArea = "ChartArea1"; 
                series1.Legend = "Legend1";
                chart.Series.Add(series1);  
                series1.ChartType = SeriesChartType.Area;
                DB db = new DB();
                var ds = db.SelectSet(@"select pur_totalcost/pur_amount p,pur_date from purchases where pur_gas_id = 1");
                

                chart.Series[0].Points.DataBind(ds.Tables[0].DefaultView,  "pur_date","p", "");
                series1.Name += " : " + ds.Tables[0].Rows[0][1].ToString();
            }
            catch
            {

            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Report1();
        }
    }
}
