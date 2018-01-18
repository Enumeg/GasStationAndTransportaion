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
using Source;

namespace haies
{
    /// <summary>
    /// Interaction logic for Driver_Search.xaml
    /// </summary>
    public partial class Driver_Search : Window
    {
        public object Driver_Id;
        public Driver_Search()
        {
            InitializeComponent();
            Get_Drivers();
        }
        private void Get_Drivers()
        {
            int mobile = 0;
            try
            {
                DB db = new DB();
                if (int.TryParse(Driver_Name_TB.Text.Trim(), out mobile))
                {
                    db.AddCondition("per_mobile", Driver_Name_TB.Text.Trim(), false, " like ");
                }
                else
                {
                    db.AddCondition("per_name", Driver_Name_TB.Text.Trim(), false, " like ");
                }
                Driver_DG.ItemsSource = db.SelectTableView("select dri_id, per_name,per_mobile from persons join drivers on dri_per_id=per_id");
            }
            catch
            {

            }
        }
        private void Driver_DG_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Driver_Id = ((System.Data.DataRowView)Driver_DG.SelectedItem)[0];
                this.Close();
            }
            catch
            {

            }
        }
        private void Driver_Name_TB_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                Get_Drivers();
            }
            catch
            {

            }
        }

    }
}
