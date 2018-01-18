using System;
using System.Data;
using System.Windows;
using Source;

namespace haies
{
    /// <summary>
    /// Interaction logic for places.xaml
    /// </summary>
    public partial class Cement_prices : Window
    {

        object CTP_ID = null;

        public Cement_prices(object ctp_ID = null)
        {
            InitializeComponent();
            Cement.Get_All_Cement(Cement_CB);
            Units.Get_All_Units(Unit_CB);
            Suppliers.Get_All_Suppliers(Supplier_CB);
            CTP_ID = ctp_ID;
            Get_Cement_Price();
        }

        private void Get_Cement_Price()
        {
            try
            {
                DB d = new DB();
                d.AddCondition("ctp_id", CTP_ID);
                DataRow DR = d.SelectRow("select * from cement_price");
                CTP_Date.Value = DateTime.Parse(DR["ctp_date"].ToString());
                Cement_CB.SelectedValue = DR["ctp_cem_id"];
                Unit_CB.SelectedValue = DR["ctp_unit_id"];
                Sell_Price_TB.Text = DR["ctp_sell_price"].ToString();
                Buy_Price_TB.Text = DR["ctp_buy_price"].ToString();
                Supplier_CB.SelectedValue = DR["ctp_sup_id"];
            }
            catch
            {

            }
        }
        private void add_update_outcome_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Notify.validate("من فضلك ادخل التاريخ", CTP_Date.Text, this))
                {
                    return;
                }
                if (Notify.validate("من فضلك اختر الاسمنت", Cement_CB.SelectedIndex, this))
                {
                    return;
                }
                if (Notify.validate("من فضلك اختر الوحده", Unit_CB.SelectedIndex, this))
                {
                    return;
                }
                if (Notify.validate("من فضلك ادخل سعر البيع", Sell_Price_TB.Text, this))
                {
                    return;
                }
                if (Notify.validate("من فضلك ادخل سعر الشراء", Buy_Price_TB.Text, this))
                {
                    return;
                }
                if (Notify.validate("من فضلك ادخل المورد ", Buy_Price_TB.Text, this))
                {
                    return;
                }
                if (Add_Update())
                {
                    var log = new Log();
                    log.Columns.Add(new Column("التاريـخ", CTP_Date.Value.Value.Date.ToShortDateString()));
                    log.Columns.Add(new Column("النوع", Cement_CB.Text));
                    log.Columns.Add(new Column("الوحدة", Unit_CB.Text));
                    log.CreateLog("أسعار الأسمنت", CTP_ID == null);
                    if ((bool)New.IsChecked)
                    {
                        App.Clear_Form(this);
                    }
                    else
                    {
                        this.Close();
                    }
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

                DB DataBase = new DB("cement_price");

                DataBase.AddColumn("ctp_date", CTP_Date.Value.Value.Date);
                DataBase.AddColumn("ctp_cem_id", Cement_CB.SelectedValue);
                DataBase.AddColumn("ctp_unit_id", Unit_CB.SelectedValue);
                DataBase.AddColumn("ctp_sell_price", Sell_Price_TB.Text.Trim());
                DataBase.AddColumn("ctp_buy_price", Buy_Price_TB.Text.Trim());
                DataBase.AddColumn("ctp_sup_id", Supplier_CB.SelectedValue);

                if (CTP_ID == null)
                {
                    if (DataBase.IsNotExist("ctp_id", "ctp_date", "ctp_cem_id", "ctp_unit_id", "ctp_sup_id"))
                    {
                        return Confirm.Check(DataBase.Insert());
                    }
                    else
                    {
                        Message.Show("لقد تم تسجيل هذا السعر من قبل", MessageBoxButton.OK, 5);
                        return false;
                    }


                }
                else
                {
                    DataBase.AddCondition("ctp_id", CTP_ID);
                    return Confirm.Check(DataBase.Update());
                }
            }
            catch
            {
                //MessageBox.Show("kiki_method");
                return false;
            }
        }

    }
}
