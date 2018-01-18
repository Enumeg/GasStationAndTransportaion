using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Data;
using Source;

namespace haies
{
    /// <summary>
    /// Interaction logic for Cars_Dates.xaml
    /// </summary>
    public partial class Cars_Dates : Window
    {
        DataTable Dates = new DataTable();
        public Cars_Dates()
        {
            InitializeComponent();

            Dates.Columns.Add("car_name"); Dates.Columns.Add("car_number"); Dates.Columns.Add("type"); Dates.Columns.Add("end_date", typeof(DateTime));
            Dates.Columns.Add("days", typeof(decimal));
            DB db = new DB();
            db.AddCondition("ED", DateTime.Now.AddMonths(2));
            DataSet ds = db.SelectSet(@"select car_name,car_number,car_application_date from cars where car_application_date<=@ED;
                                        select car_name,car_number,car_insurance_date from cars where car_insurance_date<=@ED;
                                        select car_name,car_number,car_card_date from cars where car_card_date<=@ED;
                                        select car_name,car_number,car_examination_date from cars where car_examination_date<=@ED;");
            foreach (DataRow Row in ds.Tables[0].Rows)
            {
                Dates.Rows.Add(Row[0], Row[1], "إستمارة السيارة", Row[2], Math.Floor((DateTime.Parse(Row[2].ToString()) - DateTime.Now).TotalDays));
            }
            foreach (DataRow Row in ds.Tables[1].Rows)
            {
                Dates.Rows.Add(Row[0], Row[1], "تأمين السيارة", Row[2], Math.Floor((DateTime.Parse(Row[2].ToString()) - DateTime.Now).TotalDays));
            }
            foreach (DataRow Row in ds.Tables[2].Rows)
            {
                Dates.Rows.Add(Row[0], Row[1], "كارت التشغيل", Row[2], Math.Floor((DateTime.Parse(Row[2].ToString()) - DateTime.Now).TotalDays));
            }
            foreach (DataRow Row in ds.Tables[3].Rows)
            {
                Dates.Rows.Add(Row[0], Row[1], "الفحص الدوري", Row[2], Math.Floor((DateTime.Parse(Row[2].ToString()) - DateTime.Now).TotalDays));
            }
            Dates_DG.ItemsSource = Dates.DefaultView;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CPrinting.CPrinting print = new CPrinting.CPrinting();
                print.header.Add("بيان بالسيارات التي تخطت تاريخ إنتهاء أورقها");
                App.Get_Printed_Table(print, Dates_DG);
                print.print();
            }
            catch
            {

            }
        }
    }
}
