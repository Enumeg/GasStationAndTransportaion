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
using System.Data;

namespace haies
{
    /// <summary>
    /// Interaction logic for Add_Transportation.xaml
    /// </summary>
    public partial class Add_Transportation : Page
    {

        object Trs_Id, Receipt_Id;

        public Add_Transportation(object id = null)
        {
            InitializeComponent();
            Trs_Id = id;
            Customer.Get_All_Customers(Customer_CB, Customer_type.مصنع);
            Cement.Get_All_Cement(Cement_CB);
            Units.Get_All_Units(Unit_CB);
            Drivers.Get_All_Drivers(Driver_CB, true);
            Cars.Get_All_Cars(Car_CB, true);
            Suppliers.Get_All_Suppliers(Supplier_CB);
            Get_Transportation();

        }

        private void Get_Transportation()
        {
            try
            {
                DB db2 = new DB();

                db2.AddCondition("trs_id", Trs_Id);

                DataRowView DR = db2.SelectRowView("select r.*,t.* from transportation t left join receipt r on rec_id=trs_rec_id");
                Main_GD.DataContext = DR;
                Receipt_Id = DR["rec_id"];
                Date_TB.Value = DateTime.Parse(DR["trs_date"].ToString());
                Driver_CB.SelectedValue = DR["trs_dri_id"];
                Car_CB.SelectedValue = DR["trs_car_id"];
                Cement_CB.SelectedValue = DR["rec_cem_id"];
                Unit_CB.SelectedValue = DR["rec_unit_id"];
                Customer_CB.SelectedValue = DR["trs_cust_id"];
                Supplier_CB.SelectedValue = DR["rec_sup_id"];
                Fill_Places_CB();
                Place_CB.SelectedValue = DR["trs_pl_id"];
                Amount_TB.Text = DR["rec_amount"].ToString();
                Unit_Price_TB.Text = (decimal.Parse(DR["trs_sell_price"].ToString()) + decimal.Parse(DR["rec_sell_price"].ToString())).ToString();
                Total_Paice_TB.Text = (decimal.Parse(DR["rec_amount"].ToString()) * decimal.Parse(Unit_Price_TB.Text)).ToString();
                Paid_TB.Text = DR["trs_Paid"].ToString();
                Discount_TB.Text = DR["trs_discount"].ToString();
                Bill_Num_TB.Text = DR["trs_card_number"].ToString();
                Rest_TB.Text = (decimal.Parse(Total_Paice_TB.Text) - decimal.Parse(Discount_TB.Text) - decimal.Parse(Paid_TB.Text)).ToString("0.00");
                Supplier_CB.SelectedValue = DR["rec_sup_id"];

            }
            catch
            {


            }
        }

        private decimal Get_Person_Transportations()
        {
            decimal value = 0;
            try
            {
                DB db2 = new DB();
                db2.AddCondition("ptr_per_id", ((DataRowView)Customer_CB.SelectedItem)["per_id"]);
                db2.AddCondition("ptr_pl_id", Place_CB.SelectedValue);
                db2.AddCondition("ptr_unit_id", Unit_CB.SelectedValue);
                decimal.TryParse(db2.Select(@"select COALESCE(ptr_value,0) from person_transportation ").ToString(), out value);

            }
            catch
            {

            }
            return value;
        }

        private decimal Get_Driver_Motive()
        {
            decimal value = 0;
            try
            {
                DB db2 = new DB();

                db2.AddCondition("ptr_per_id", null, false, " is ");
                db2.AddCondition("ptr_pl_id", Place_CB.SelectedValue);

                decimal.TryParse(db2.Select(@"select COALESCE(ptr_value,0) from person_transportation ").ToString(), out value);

            }
            catch
            {

            }
            return value;
        }

        private decimal Get_CarOwner_Cost()
        {
            decimal value = 0;
            try
            {
                DB db2 = new DB();

                db2.AddCondition("car_id", Car_CB.SelectedValue);
                db2.AddCondition("ptr_unit_id", Unit_CB.SelectedValue);
                db2.AddCondition("ptr_pl_id", Place_CB.SelectedValue);

                decimal.TryParse(db2.Select(@"select COALESCE(ptr_value,0) from person_transportation join persons on per_id=ptr_per_id
                                             join car_owner on cor_per_id=per_id  join cars on car_cor_id=cor_id 
                                             ").ToString(), out value);

            }
            catch
            {

            }
            return value;
        }

        private void Fill_Places_CB()
        {

            try
            {
                DB db2 = new DB("person_transportation");

                db2.AddCondition("ptr_per_id", ((DataRowView)Customer_CB.SelectedItem)[0], Customer_CB.SelectedIndex == -1);

                db2.Fill(Place_CB, "pl_id", "pl_name", "select distinct(pl_id),pl_name from person_transportation ptr join places p on ptr_pl_id=pl_id");

            }

            catch
            {

            }
        }

        private void Get_Rest()
        {
            try
            {
                Rest_TB.Text = (decimal.Parse(Total_Paice_TB.Text) - decimal.Parse(Discount_TB.Text) - decimal.Parse(Paid_TB.Text)).ToString("0.00");
            }
            catch
            {

            }
        }

        private void Get_Receipt_Prices()
        {
            decimal[] Cement_price = new decimal[2];
            decimal trs_price = 0;

            try
            {
                Cement_price = Get_Cement_price();
                trs_price = Get_Person_Transportations();
                trs_price = trs_price == 0 ? 0 : trs_price + (decimal.Parse(Discount_TB.Text) / decimal.Parse(Amount_TB.Text)) - Cement_price[2];
                Unit_Price_TB.Text = (Cement_price[0] + trs_price - (decimal.Parse(Discount_TB.Text) / decimal.Parse(Amount_TB.Text))).ToString("0.000000");
                Total_Paice_TB.Text = (decimal.Parse(Unit_Price_TB.Text) * decimal.Parse(Amount_TB.Text)).ToString("0.00");
            }
            catch
            {

            }
        }

        private decimal[] Get_Cement_price()
        {
            decimal[] values = new decimal[] { 0, 0, 0 };
            try
            {
                DB db2 = new DB("cement_price");
                db2.AddCondition("ctp_sup_id", Supplier_CB.SelectedValue);
                db2.AddCondition("ctp_unit_id", Unit_CB.SelectedValue);
                db2.AddCondition("ctp_cem_id", Cement_CB.SelectedValue);
                db2.AddCondition("ctp_date", Date_TB.Value.Value.Date);
                DataRow dr = db2.SelectRow(@"select ctp_sell_price,ctp_buy_price from cement_price where ctp_date=(select max(ctp_date) from cement_price where ctp_date<=@ctp_date
                                             and ctp_unit_id=@ctp_unit_id and ctp_cem_id=@ctp_cem_id and ctp_sup_id=@ctp_sup_id) and ctp_unit_id=@ctp_unit_id and ctp_cem_id=@ctp_cem_id and ctp_sup_id=@ctp_sup_id");
                decimal.TryParse(dr["ctp_sell_price"].ToString(), out values[0]);
                decimal.TryParse(dr["ctp_buy_price"].ToString(), out values[1]);
                values[2] = values[0] - values[1];
            }
            catch
            {


            }
            return values;
        }

        private bool Add_Update()
        {
            decimal[] Cement_price = new decimal[2];
            try
            {
                DB DataBase = new DB("receipt");
                Cement_price = Get_Cement_price();
                DataBase.AddColumn("rec_Sup_id", Supplier_CB.SelectedValue);
                DataBase.AddColumn("rec_cem_id", Cement_CB.SelectedValue);
                DataBase.AddColumn("rec_unit_id", Unit_CB.SelectedValue);
                DataBase.AddColumn("rec_sell_price", Cement_price[0]);
                DataBase.AddColumn("rec_buy_price", Cement_price[1]);
                DataBase.AddColumn("rec_amount", Amount_TB.Text.Trim());
                if (Receipt_Id == null)
                {
                    if (DataBase.Insert())
                    {
                        Receipt_Id = DataBase.Last_Inserted;
                        if (Add_Update_Transportation(Receipt_Id))
                        {
                            return Confirm.Check(true);
                        }
                        else
                        {
                            DB DB = new DB("receipt");
                            DB.AddCondition("rec_id", Receipt_Id);
                            DB.Delete();
                            return Confirm.Check(false);
                        }
                    }
                    else
                    {
                        return Confirm.Check(false);
                    }
                }
                else
                {
                    DataBase.AddCondition("rec_id", Receipt_Id);
                    if (DataBase.Update())
                    {
                        return Confirm.Check(Add_Update_Transportation(Receipt_Id));
                    }
                    else
                    {
                        return Confirm.Check(false);
                    }
                }
            }
            catch
            {
                //MessageBox.Show("kiki_method");
                return false;
            }
        }

        private bool Add_Update_Transportation(object rec_id)
        {

            try
            {
                DB DB = new DB("transportation");
                DB.AddColumn("trs_rec_id", rec_id);
                DB.AddColumn("trs_date", Date_TB.Value.Value.Date);
                DB.AddColumn("trs_dri_id", Driver_CB.SelectedValue);
                DB.AddColumn("trs_car_id", Car_CB.SelectedValue);
                DB.AddColumn("trs_cust_id", Customer_CB.SelectedValue);
                DB.AddColumn("trs_pl_id", Place_CB.SelectedValue);
                DB.AddColumn("trs_sell_price", Get_Person_Transportations() + (decimal.Parse(Discount_TB.Text) / decimal.Parse(Amount_TB.Text)) - Get_Cement_price()[2]);
                DB.AddColumn("trs_buy_price", Get_CarOwner_Cost());
                DB.AddColumn("trs_paid", Paid_TB.Text.Trim());
                DB.AddColumn("trs_discount", Discount_TB.Text.Trim());
                DB.AddColumn("trs_dri_motive", Get_Driver_Motive());
                DB.AddColumn("trs_payment_method", Payment_Method_TB.Text.Trim());
                DB.AddColumn("trs_card_number", Bill_Num_TB.Text.Trim());
                if (DB.IsNotExist("trs_id", "trs_rec_id"))
                {
                    return (DB.Insert());
                }
                else
                {
                    // 3alshan hwa mawgod 2abl keda
                    DB.AddCondition("trs_rec_id", rec_id);
                    return (DB.Update());
                }

            }
            catch
            {
                //MessageBox.Show("hena el error");
                return (false);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                if (Notify.validate("من فضلك اختر السائق", Driver_CB.SelectedIndex, Cement_Office.GetWindow(this)))
                {
                    return;
                }

                if (Notify.validate("من فضلك اختر السياره", Car_CB.SelectedIndex, Cement_Office.GetWindow(this)))
                {
                    return;
                }



                if (Notify.validate("من فضلك اختر العميل", Customer_CB.SelectedIndex, Cement_Office.GetWindow(this)))
                {
                    return;
                }


                if (Notify.validate("من فضلك اختر الاتجاه", Place_CB.SelectedIndex, Cement_Office.GetWindow(this)))
                {
                    return;
                }

                if (Add_Update())
                {
                    var log = new Log();
                    log.Columns.Add(new Column("رقــــم الفاتورة", Receipt_Id.ToString()));
                    log.CreateLog("النقليات", Receipt_Id == null);

                    App.Clear_Form(Main_GD);
                    Discount_TB.Text = Paid_TB.Text = "0";
                    Receipt_Id = Trs_Id = null;
                }


            }
            catch
            {
                return;
            }
        }

        private void Discount_TB_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                Get_Rest();
            }
            catch
            {

            }
        }

        private void Customer_CB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                Fill_Places_CB();
                Get_Rest();
            }
            catch
            {

            }
        }

        private void Place_CB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                Get_Receipt_Prices();
                Get_Rest();
            }
            catch
            {

            }
        }

        private void Driver_CB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                DB db = new DB();
                db.AddCondition("car_dri_id", Driver_CB.SelectedValue);
                Car_CB.SelectedValue = db.Select("select car_id from cars");
            }
            catch
            {

            }
        }

        private void Cement_CB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (this.IsLoaded)
                    Get_Receipt_Prices();
            }
            catch
            {

            }
        }

        private void Amount_TB_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (this.IsLoaded)
                    Get_Receipt_Prices();
            }
            catch
            {

            }
        }

    }
}
