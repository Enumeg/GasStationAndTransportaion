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
using System.Data;

namespace haies
{
    /// <summary>
    /// Interaction logic for places.xaml
    /// </summary>
    public partial class Gas : Window
    {

        object GasId = null;

        public Gas()
        {
            InitializeComponent();
            fill_gas_listbox();
        }
        private void fill_gas_listbox()
        {

            try
            {

                DB db2 = new DB("gas");

                // search by name
                db2.AddCondition("gas_name", "%" + Gas_TB.Text + "%", false, " like ");

                db2.Fill(LB, "gas_id", "gas_name", "select * from gas");

            }
            catch
            {

            }

        }
        public static void Get_All_Gas(ComboBox CB, string All = "")
        {
            try
            {

                DB db2 = new DB("gas");
                db2.Fill(CB, "gas_id", "gas_name", "select * from gas", All);

            }
            catch
            {

            }
        }
        private void EP_Add(object sender, EventArgs e)
        {
            try
            {
                GasId = null;
                Main_GD.RowDefinitions[1].Height = new GridLength(35);
            }
            catch
            {

            }
        }
        private void EP_Edit(object sender, EventArgs e)
        {
            try
            {

                if (LB.SelectedIndex != -1)
                {
                    GasId = LB.SelectedValue;
                    Gas_TB.Text = ((DataRowView)LB.SelectedItem)[1].ToString();
                    Main_GD.RowDefinitions[1].Height = new GridLength(35);
                }
            }
            catch
            {

            }
        }
        private void EP_Delete(object sender, EventArgs e)
        {
            try
            {
                if (LB.SelectedIndex != -1)
                {
                    if (Message.Show("هل تريد حذف هذا الصنف", MessageBoxButton.YesNoCancel, 10) == MessageBoxResult.Yes)
                    {
                        GasId = LB.SelectedValue;
                        DB db = new DB("gas");
                        db.AddCondition("gas_id", GasId);
                        if (db.Delete())
                        {
                            var log = new Log();
                            log.Columns.Add(new Column("الإسم", Gas_TB.Text));
                            log.CreateLog("المحروقات");
                            fill_gas_listbox();

                        }
                    }
                }
            }
            catch
            {

            }
        }
        private void add_update_outcome_Click(object sender, RoutedEventArgs e)
        {
            try
            {


                if (Notify.validate("من فضلك ادخل اسم المحروق", Gas_TB.Text, this))
                {
                    return;
                }


                if (Add_Update())
                {
                    var log = new Log();
                    log.Columns.Add(new Column("الإسم", Gas_TB.Text));
                    log.CreateLog("المحروقات", this.GasId == null);
                    GasId = null;
                    fill_gas_listbox();
                    Main_GD.RowDefinitions[1].Height = new GridLength(0);

                    // yesafar
                    Gas_TB.Text = "";
                }
            }
            catch
            {
                return;
            }
        }
        public bool Add_Update()
        {
            try
            {

                DB DataBase = new DB("gas");

                DataBase.AddColumn("gas_name", Gas_TB.Text);

                if (this.GasId == null)
                {
                    if (DataBase.IsNotExist("gas_id", "gas_name"))
                    {
                        return Confirm.Check(DataBase.Insert());
                    }
                    else
                    {
                        Message.Show("لقد تم تسجيل هذا النوع من قبل", MessageBoxButton.OK, 5);
                        return false;
                    }


                }
                else
                {
                    DataBase.AddCondition("gas_id", this.GasId);
                    return Confirm.Check(DataBase.Update());
                }
            }
            catch
            {
                //MessageBox.Show("kiki_method");
                return false;
            }
        }
        private void LB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            try
            {

            }
            catch
            {

            }

        }
        private void Place_TB_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                fill_gas_listbox();
            }
            catch
            {

            }
        }
    }
}
