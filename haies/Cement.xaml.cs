using System;
using System.Windows;
using System.Windows.Controls;
using Source;
using System.Data;

namespace haies
{
    /// <summary>
    /// Interaction logic for Cement.xaml
    /// </summary>
    public partial class Cement : Page
    {
        object CementId = null;

        public Cement()
        {
            InitializeComponent();
            Fill_Cement_LB();
            Get_Cement_Prices();
            Suppliers.Get_All_Suppliers(Suppliers_CB, "الكل");
        }
        private void Fill_Cement_LB()
        {

            try
            {
                DB db2 = new DB("cement");

                // search by name
                db2.AddCondition("cem_name", "%" + Cement_TB.Text + "%", false, " like ");

                db2.Fill(LB, "cem_id", "cem_name", "select * from cement");


            }
            catch
            {

            }
        }
        public static void Get_All_Cement(ComboBox CB, string All = "")
        {
            try
            {
                DB db2 = new DB("cement");

                db2.Fill(CB, "cem_id", "cem_name", "select * from cement", All);
            }

            catch
            {

            }
        }
        private void Get_Cement_Prices()
        {
            try
            {
                DB db = new DB();
                db.AddCondition("ctp_sup_id", Suppliers_CB.SelectedValue, Suppliers_CB.SelectedIndex < 1);
                Cement_Prices_DG.ItemsSource = db.SelectTableView("select cp.*,cem_name,unit_name from cement_price cp join cement c on cem_id=ctp_cem_id join units on ctp_unit_id=unit_id");
            }
            catch
            {

            }
        }

        private void EP_Add(object sender, EventArgs e)
        {
            try
            {

                CementId = null;
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
                    CementId = LB.SelectedValue;
                    Cement_TB.Text = ((DataRowView)LB.SelectedItem)[1].ToString();
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
                        CementId = LB.SelectedValue;
                        DB db = new DB("cement");
                        db.AddCondition("cem_id", CementId);
                        if (db.Delete())
                        {
                            var log = new Log();
                            log.Columns.Add(new Column("الإسم", Cement_TB.Text));
                            log.CreateLog("الأسمنت");
                        }
                        Fill_Cement_LB();
                    }
                }
            }
            catch
            {

            }
        }

        private bool Add_Update()
        {
            try
            {

                DB DataBase = new DB("cement");

                DataBase.AddColumn("cem_name", Cement_TB.Text);

                if (this.CementId == null)
                {
                    if (DataBase.IsNotExist("cem_id", "cem_name"))
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
                    DataBase.AddCondition("cem_id", this.CementId);
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
                Fill_Cement_LB();
            }
            catch
            {

            }
        }
        private void add_update_outcome_Save(object sender, EventArgs e)
        {
            try
            {
                if (Notify.validate("من فضلك ادخل الاسم", Cement_TB.Text, MainWindow.GetWindow(this)))
                {
                    return;
                }

                if (Add_Update())
                {
                    var log = new Log();
                    log.Columns.Add(new Column("الإسم", Cement_TB.Text));
                    log.CreateLog("الأسمنت", CementId == null);
                    CementId = null;
                    Fill_Cement_LB();
                    Main_GD.RowDefinitions[1].Height = new GridLength(0);

                    // yesafar
                    Cement_TB.Text = "";
                    Get_Cement_Prices();
                }
            }
            catch
            {
                return;
            }
        }
        private void add_update_outcome_Cancel(object sender, EventArgs e)
        {
            try
            {
                Main_GD.RowDefinitions[1].Height = new GridLength(0);
            }
            catch
            {

            }
        }
        private void Cement_Prices_DG_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            try
            {
                DataRowView drv = Cement_Prices_DG.SelectedItem as DataRowView;
                DB d = new DB("cement_price");
                d.AddCondition("ctp_id", drv["ctp_id"]);
                d.AddColumn("ctp_price", ((TextBox)e.Column.GetCellContent(e.Row)).Text);
                d.Update();
                Get_Cement_Prices();
            }
            catch
            {

            }
        }

        private void Cem_EP_Add(object sender, EventArgs e)
        {
            try
            {

                Cement_prices cp = new Cement_prices();
                cp.ShowDialog();
                Get_Cement_Prices();

            }
            catch
            {

            }
        }

        private void Cem_EP_Edit(object sender, EventArgs e)
        {
            try
            {
                if (Cement_Prices_DG.SelectedIndex != -1)
                {
                    Cement_prices cp = new Cement_prices(((DataRowView)Cement_Prices_DG.SelectedItem)[0]);
                    cp.ShowDialog();
                    Get_Cement_Prices();
                }
            }
            catch
            {

            }
        }

        private void Cem_EP_Delete(object sender, EventArgs e)
        {
            try
            {
                if (Cement_Prices_DG.SelectedIndex != -1)
                {
                    if (Message.Show("هل تريد حذف هذا السعر", MessageBoxButton.YesNoCancel, 5) == MessageBoxResult.Yes)
                    {
                        DB DataBase = new DB("cement_price");
                        DataBase.AddCondition("ctp_id", ((DataRowView)Cement_Prices_DG.SelectedItem)[0]);
                        if (DataBase.Delete())
                        {
                            var log = new Log();
                            log.Columns.Add(new Column("التاريـخ", ((DataRowView)Cement_Prices_DG.SelectedItem)[1]));
                            log.Columns.Add(new Column("النوع", ((DataRowView)Cement_Prices_DG.SelectedItem)[2]));
                            log.Columns.Add(new Column("الوحدة", ((DataRowView)Cement_Prices_DG.SelectedItem)[3]));
                            log.CreateLog("أسعار الأسمنت");
                        }
                        Get_Cement_Prices();
                    }
                }
            }
            catch
            {

            }
        }

        private void Suppliers_CB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                Get_Cement_Prices();
            }
            catch
            {

            }
        }
    }
}
