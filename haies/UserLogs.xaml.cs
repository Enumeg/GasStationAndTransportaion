using System;
using System.Data;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Source;

namespace haies
{
    /// <summary>
    /// Interaction logic for daily_report.xaml
    /// </summary>
    public partial class UserLogs : Page
    {
        string[] actions;
        public UserLogs()
        {
            InitializeComponent();
            FillPages();
            actions = new string[] { "إضافة", "تعديل", "حذف" };
            Users.GetAllUsers(Users_CB, "الكل");
        }

        private void FillPages()
        {
            var db = new DB();
            db.Fill(Pages_CB, "name", "name", "select log_entity name from users_log group by log_entity order by log_entity ", "الكل");
            Pages_CB.SelectedIndex = 0;
        }


        private void Fill_DG()
        {
            DB db = new DB();
            try
            {
                db.AddCondition("log_time", From_DTP.Value.Value.Date, false, ">=", "SD");
                db.AddCondition("log_time", To_DTP.Value.Value.Date, false, "<=", "ED");
                db.AddCondition("log_entity", Pages_CB.SelectedValue, Pages_CB.SelectedIndex < 1);
                db.AddCondition("log_User_Id", Users_CB.SelectedValue, Users_CB.SelectedIndex < 1);
                db.AddCondition("log_action", Actions_CB.SelectedIndex - 1, Actions_CB.SelectedIndex < 1);
                var table = db.SelectTable(@"select * ,'' action,user_name user from users_log join users on log_user_id = user_id ");
                foreach (DataRow row in table.Rows)
                {
                    row["action"] = actions[int.Parse(row["log_action"].ToString())];
                }
                Daily_DG.ItemsSource = table.DefaultView;

            }
            catch
            {

            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (this.IsLoaded)
                    Fill_DG();
            }
            catch
            {

            }
        }

        private void From_DTP_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            try
            {
                if (this.IsLoaded)
                    Fill_DG();
            }
            catch
            {

            }
        }

        private void Print_BTN_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CPrinting.CPrinting Printer = new CPrinting.CPrinting();
                Printer.Get_Printed_Table(Daily_DG);
                Printer.PrintDocument.DefaultPageSettings.Landscape = true;
                Printer.print();
            }
            catch
            {

            }
        }


    }
}
