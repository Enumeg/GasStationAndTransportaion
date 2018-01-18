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
    public partial class Places_prices : Window
    {
        object Per_Id, Place_PriceId = null;

        public Places_prices(object per_Id, string type)
        {
            InitializeComponent();
            if (type != "Customers")
            {
                Main_GD.RowDefinitions[1].Height = new GridLength(0);
            }
            Per_Id = per_Id;

            fill_Places_prices_listbox();
            Units.Get_All_Units(Unit_CB, "إضافة ...");
            fill_Places_combobox();


        }
        private void fill_Places_prices_listbox()
        {

            DB db2 = new DB("person_transportation");
            //DB db2 = new DB();

            // search by Car Owner Id
            db2.AddCondition("ptr_per_id", Per_Id);
            db2.Conditions[0].Operator = Per_Id == null ? " is " : " = ";

            db2.Fill(LB, "ptr_id", "pl_name", "select p.*,ptr.*, COALESCE(CONCAT('- ' , unit_name), '') unit_name from places p join person_transportation ptr on p.pl_id=ptr_pl_id left join units on ptr_unit_id=unit_id");


        }
        private void fill_Places_combobox()
        {

            try
            {
                DB db2 = new DB("places");

                db2.Fill(Place_CB, "pl_id", "pl_name", "select * from places", "إضافة ...");
            }

            catch
            {

            }


        }
        private void EP_Add(object sender, EventArgs e)
        {
            try
            {
                Place_PriceId = null;


                Main_GD.RowDefinitions[3].Height = new GridLength(35);
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

                    Place_PriceId = LB.SelectedValue;

                    DataRowView DR = ((DataRowView)LB.SelectedItem);

                    Place_CB.SelectedValue = DR["pl_id"];
                    Unit_CB.SelectedValue = DR["ptr_unit_id"];
                    Price_TB.Text = DR["ptr_value"].ToString();

                    Main_GD.RowDefinitions[3].Height = new GridLength(35);
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
                    if (Message.Show("هل تريد الحذف", MessageBoxButton.YesNoCancel, 10) == MessageBoxResult.Yes)
                    {
                        var row = LB.SelectedItem as DataRowView;
                        Place_PriceId = LB.SelectedValue;
                        DB db = new DB("person_transportation");
                        db.AddCondition("ptr_id", Place_PriceId);
                        if (db.Delete())
                        {
                            var log = new Log();
                            log.Columns.Add(new Column("المنطقه", row["pl_name"]));
                            log.Columns.Add(new Column("الوحدة", row["unit_name"]));
                            log.Columns.Add(new Column("السعر", row["ptr_value"]));
                            log.CreateLog("اسعار المناطق");

                            fill_Places_prices_listbox();
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


                if (Notify.validate("من فضلك اختر المنطقه ", Place_CB.SelectedIndex, this))
                {
                    return;
                }

                if (Notify.validate("من فضلك ادخل السعر", Price_TB.Text, this))
                {
                    return;
                }


                if (Add_Update())
                {
                    var log = new Log();
                    log.Columns.Add(new Column("المنطقه", Place_CB.Text));
                    log.Columns.Add(new Column("الوحدة", Unit_CB.Text));
                    log.Columns.Add(new Column("السعر", Price_TB.Text));
                    log.CreateLog("اسعار المناطق", Place_PriceId == null);
                    Place_PriceId = null;
                    fill_Places_prices_listbox();
                    Main_GD.RowDefinitions[3].Height = new GridLength(0);

                    // yesafar
                    Place_CB.SelectedIndex = -1;
                    Price_TB.Text = "";
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

                DB DataBase = new DB("person_transportation");

                DataBase.AddColumn("ptr_per_id", Per_Id);
                DataBase.AddColumn("ptr_pl_id", Place_CB.SelectedValue);
                DataBase.AddColumn("ptr_unit_id", Unit_CB.SelectedValue);
                DataBase.AddColumn("ptr_value", Price_TB.Text);

                if (Place_PriceId == null)
                {
                    if (DataBase.IsNotExist("ptr_id", "ptr_per_id", "ptr_pl_id", "ptr_unit_id"))
                    {
                        return Confirm.Check(DataBase.Insert());
                    }
                    else
                    {
                        Message.Show("لقد تم تسجيل هذه المنطقه من قبل", MessageBoxButton.OK, 5);
                        return false;
                    }


                }
                else
                {
                    DataBase.AddCondition("ptr_id", LB.SelectedValue);
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
                DB db2 = new DB("person_transportation");

                db2.SelectedColumns.Add("ptr_value");

                db2.AddCondition("ptr_pl_id", LB.SelectedValue);

                DataRow DR = db2.SelectRow();

                Price_TB.Text = DR["ptr_value"].ToString();

            }
            catch
            {

            }

        }
        private void Place_CB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (Place_CB.SelectedIndex == 0)
                {
                    Places p = new Places();
                    p.ShowDialog();
                    fill_Places_combobox();

                }
            }
            catch
            {

            }
        }

        private void Unit_CB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (Unit_CB.SelectedIndex == 0)
                {
                    Units p = new Units();
                    p.ShowDialog();
                    Units.Get_All_Units(Unit_CB, "إضافة ...");

                }
            }
            catch
            {

            }
        }

    }
}
