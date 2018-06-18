using System;
using System.Windows;
using Source;

namespace haies
{
    /// <summary>
    /// Interaction logic for GasPrice.xaml
    /// </summary>
    public partial class Gas_Price
    {
        private object _gspId;
        public Gas_Price(object gspid = null)
        {
            InitializeComponent();
            Gas.Get_All_Gas(Gas_CB);
            Gas_CB.SelectedIndex = 0;
            _gspId = gspid;
            if (gspid != null)
                Get_Gas_Price();
        }
        private void Get_Gas_Price()
        {
            try
            {
                var d = new DB();
                d.AddCondition("gsp_id", _gspId);
                var DR = d.SelectRow("select * from gas_price");
                Gas_Date.Value = DateTime.Parse(DR["gsp_date"].ToString());
                Gas_CB.SelectedValue = DR["gsp_gas_id"];
                Sell_Price_TB.Text = DR["gsp_sellCost"].ToString();
                Sell_Tax_TB.Text = DR["gsp_sell_Tax"].ToString();
                Buy_Price_TB.Text = DR["gsp_buyCost"].ToString();
                Buy_Tax_TB.Text = DR["gsp_buy_tax"].ToString();
            }
            catch (Exception ex)
            {

            }
        }
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Notify.validate("من فضلك ادخل التاريخ", Gas_Date.Text, this))
                {
                    return;
                }
                if (Notify.validate("من فضلك اختر المحروق", Gas_CB.SelectedIndex, this))
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
                if (Notify.validate("من فضلك ادخل ضريبة البيع", Sell_Tax_TB.Text, this))
                {
                    return;
                }
                if (Notify.validate("من فضلك ادخل ضريبة الشراء", Buy_Tax_TB.Text, this))
                {
                    return;
                }
                if (Add_Update())
                {
                    var log = new Log();
                    log.Columns.Add(new Column("التاريـخ", Gas_Date.Value.Value.Date.ToShortDateString()));
                    log.Columns.Add(new Column("النوع", Gas_CB.Text));
                    log.Columns.Add(new Column("سعر البيع", Sell_Price_TB.Text));
                    log.Columns.Add(new Column("سعر الشراء", Buy_Price_TB.Text));
                    log.Columns.Add(new Column("ضريبة البيع", Sell_Tax_TB.Text));
                    log.Columns.Add(new Column("ضريبة الشراء", Buy_Tax_TB.Text));
                    log.CreateLog("أسعار المحروقات", _gspId == null);
                    if (New.IsChecked != null && (bool)New.IsChecked)
                    {
                        App.Clear_Form(this);
                    }
                    else
                    {
                        Close();
                    }
                }
            }
            catch (Exception ex)
            {
                return;
            }
        }
        public bool Add_Update()
        {
            try
            {

                var dataBase = new DB("Gas_price");

                dataBase.AddColumn("gsp_date", Gas_Date.Value.Value.Date);
                dataBase.AddColumn("gsp_gas_id", Gas_CB.SelectedValue);
                dataBase.AddColumn("gsp_sellCost", Sell_Price_TB.Text.Trim());
                dataBase.AddColumn("gsp_buyCost", Buy_Price_TB.Text.Trim());
                dataBase.AddColumn("gsp_sell_tax", Sell_Tax_TB.Text.Trim());
                dataBase.AddColumn("gsp_buy_tax", Buy_Tax_TB.Text.Trim());
                if (_gspId == null)
                {
                    if (dataBase.IsNotExist("gsp_id", "gsp_date", "gsp_gas_id"))
                    {
                        return Confirm.Check(dataBase.Insert());
                    }
                    Message.Show("لقد تم تسجيل هذا السعر من قبل", MessageBoxButton.OK, 5);
                    return false;
                }
                dataBase.AddCondition("gsp_id", _gspId);
                return Confirm.Check(dataBase.Update());
            }
            catch
            {
                //MessageBox.Show("kiki_method");
                return false;
            }
        }

    }
}
