using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using Source;

namespace haies
{
    /// <summary>
    /// Interaction logic for places.xaml
    /// </summary>
    public partial class Places : Window
    {

        object PlaceId = null;

        public Places()
        {
            InitializeComponent();
            fill_places_listbox();
        }
        private void fill_places_listbox()
        {

            DB db2 = new DB("places");

            // search by name
            db2.AddCondition("pl_name", "%" + Place_TB.Text + "%", false, " like ");

            db2.Fill(LB, "pl_id", "pl_name", "select * from places");


        }
        public static void Get_All_Places(ComboBox CB, object Customer = null, string All = "")
        {

            try
            {
                DB db2 = new DB("person_transportation");

                db2.AddCondition("ptr_per_id", Customer, Customer == null);

                db2.Fill(CB, "ptr_pl_id", "pl_name", "select ptr.*,p.* from person_transportation ptr join places p on ptr_pl_id=pl_id", All);

            }

            catch
            {

            }
        }
        private void EP_Add(object sender, EventArgs e)
        {
            try
            {
                PlaceId = null;
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

                    PlaceId = LB.SelectedValue;
                    Place_TB.Text = ((DataRowView)LB.SelectedItem)[1].ToString();
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
                    if (Message.Show("هل تريد حذف هذه المنطقه", MessageBoxButton.YesNoCancel, 10) == MessageBoxResult.Yes)
                    {
                        PlaceId = LB.SelectedValue;
                        DB db = new DB("places");
                        db.AddCondition("pl_id", PlaceId);
                        if (db.Delete())
                        {
                            var log = new Log();
                            log.Columns.Add(new Column("الإسم", Place_TB.Text));
                            log.CreateLog("المناطق");
                            fill_places_listbox();
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


                if (Notify.validate("من فضلك ادخل المنطقه", Place_TB.Text, this))
                {
                    return;
                }


                if (Add_Update())
                {
                    var log = new Log();
                    log.Columns.Add(new Column("الإسم", Place_TB.Text));
                    log.CreateLog("المناطق", PlaceId==null);
                    PlaceId = null;
                    fill_places_listbox();
                    Main_GD.RowDefinitions[1].Height = new GridLength(0);

                    // yesafar
                    Place_TB.Text = "";
                }
            }
            catch
            {
                return;
            }
        }
        private bool Add_Update()
        {
            try
            {

                DB DataBase = new DB("places");

                DataBase.AddColumn("pl_name", Place_TB.Text);

                if (this.PlaceId == null)
                {
                    if (DataBase.IsNotExist("pl_id", "pl_name"))
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
                    DataBase.AddCondition("pl_id", this.PlaceId);
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
                fill_places_listbox();
            }
            catch
            {

            }
        }
    }
}
