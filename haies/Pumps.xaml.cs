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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Source;
using System.IO;
using Microsoft.Win32;


namespace haies
{
    /// <summary>
    /// Interaction logic for Product.xaml
    /// </summary>
    public partial class Pumps : Page
    {
        public Pumps()
        {
            InitializeComponent();
            fill_Pumps_listbox();
            Gas.Get_All_Gas(Gas_CB);
            Gas.Get_All_Gas(Type_Search_CB, "الكل");
        }

        public static void Get_All_Pumps(ComboBox CB, object Gas_Id = null, string All = "")
        {
            try
            {


                DB db2 = new DB("pumps");

                // search by type
                db2.AddCondition("pum_gas_id", Gas_Id, Gas_Id == null);


                db2.Fill(CB, "pum_id", "pum_number", "select * from pumps", All);

            }
            catch
            {

            }
        }

        private void fill_Pumps_listbox()
        {

            DB db2 = new DB("pumps");

            // search by name
            db2.AddCondition("pum_number", "%" + Number_Search_TB.Text + "%", false, " like ");

            // search by type
            db2.AddCondition("pum_gas_id", Type_Search_CB.SelectedValue, Type_Search_CB.SelectedIndex < 1);


            db2.Fill(LB, "pum_id", "pum_number", "select * from pumps");


        }

        private void Name_TB_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                fill_Pumps_listbox();
            }
            catch
            {

            }
        }

        private void Type_Search_CB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                fill_Pumps_listbox();
            }
            catch
            {

            }
        }

        private bool Add_Update()
        {


            try
            {
                //object product_id = null;


                DB DataBase = new DB("pumps");

                DataBase.AddColumn("pum_gas_id", Gas_CB.SelectedValue);
                DataBase.AddColumn("pum_number", Number_TB.Text);

                if (LB.SelectedIndex == -1)
                {

                    if (DataBase.IsNotExist("pum_id", "pum_gas_id", "pum_number"))
                    {
                        return Confirm.Check(DataBase.Insert());

                    }
                    else
                    {
                        Message.Show("هذا المحروق مستخدم من قبل", MessageBoxButton.OK, 5);
                        return false;
                    }


                }

                else
                {
                    DataBase.AddCondition("pum_id", LB.SelectedValue);
                    return Confirm.Check(DataBase.Update());
                }




            }
            catch
            {
                return false;
            }

        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Add_Update())
                {
                    var log = new Log();
                    log.Columns.Add(new Column("رقم الانبوبه  ", Number_TB.Text));
                    log.Columns.Add(new Column("الغاز", Gas_CB.Text));
                    log.CreateLog("انابيب المحروق", LB.SelectedIndex == -1);

                    App.Set_Style(Main_Grid, Operations.View);
                    Save.Visibility = Cancel.Visibility = System.Windows.Visibility.Collapsed;
                    LB.IsEnabled = true;

                    int i = LB.SelectedIndex;
                    fill_Pumps_listbox();
                    LB.SelectedIndex = i;
                }
            }
            catch
            {

            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                App.Set_Style(Main_Grid, Operations.View);
                Save.Visibility = Cancel.Visibility = System.Windows.Visibility.Collapsed;
                LB.IsEnabled = true;
            }
            catch
            {

            }
        }

        private void EditPanel_Add(object sender, EventArgs e)
        {
            try
            {
                App.Set_Style(Main_Grid, Operations.Add);
                Save.Visibility = Cancel.Visibility = System.Windows.Visibility.Visible;
                LB.IsEnabled = false;
                LB.SelectedIndex = -1;
            }
            catch
            {

            }
        }

        private void EditPanel_Edit(object sender, EventArgs e)
        {
            try
            {
                App.Set_Style(Main_Grid, Operations.Edit);
                Save.Visibility = Cancel.Visibility = System.Windows.Visibility.Visible;
                LB.IsEnabled = false;
            }
            catch
            {

            }
        }

        private void EditPanel_Delete(object sender, EventArgs e)
        {
            try
            {
                if (Message.Show("هل تريد الحذف", MessageBoxButton.YesNoCancel, 5) == MessageBoxResult.Yes)
                {
                    DB d = new DB("pumps");
                    d.AddCondition("pum_id", LB.SelectedValue);
                    if (d.Delete())
                    {
                        var log = new Log();
                        log.Columns.Add(new Column("رقم الانبوبه  ", Number_TB.Text));
                        log.Columns.Add(new Column("الغاز", Gas_CB.Text));
                        log.CreateLog("انابيب المحروق");

                        fill_Pumps_listbox();
                    }
                }
            }
            catch
            {

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
    }
}
