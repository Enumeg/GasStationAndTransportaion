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
using System.IO;

namespace haies
{
    /// <summary>
    /// Interaction logic for newUser.xaml
    /// </summary>
    public partial class Receipt : Page
    {

        object Receipt_Id;

        bool Is_Editable = false;

        public Receipt(object id = null)
        {
            InitializeComponent();
            Receipt_Id = id;
          
            Cement.Get_All_Cement(Cement_CB);
            Units.Get_All_Units(Unit_CB);
            Drivers.Get_All_Drivers(Driver_CB, false);
            Cars.Get_All_Cars(Car_CB, false);
            Suppliers.Get_All_Suppliers(Supplier_CB);
            Get_Receipt_Number();
            Get_Receipt();
            Is_Editable = true;
            try
            {
                if (App.User.GroupId == "1")
                {
                    Receipt_Number_TK.Style = FindResource("Search_TextBox") as Style;
                }
            }
            catch
            {
            }
        }

        private void Get_Receipt()
        {
            try
            {
                DB db2 = new DB();

                db2.AddCondition("rec_id", Receipt_Id);

                DataRowView DR = db2.SelectRowView("select r.*,t.* from receipt r join transportation t on rec_id=trs_rec_id");
                Main_GD.DataContext = DR;
                Receipt_Number_TK.Text = DR["rec_number"].ToString();
                Date_TB.Value = DateTime.Parse(DR["trs_date"].ToString());
                Driver_CB.SelectedValue = DR["trs_dri_id"];
                Car_CB.SelectedValue = DR["trs_car_id"];
                Cement_CB.SelectedValue = DR["rec_cem_id"];
                Unit_CB.SelectedValue = DR["rec_unit_id"];
                Supplier_CB.SelectedValue = DR["rec_sup_id"];

                Amount_TB.Text = DR["rec_amount"].ToString();
                Unit_Price_TB.Text = DR["rec_sell_price"].ToString();
                Total_Paice_TB.Text = (decimal.Parse(DR["rec_amount"].ToString()) * decimal.Parse(DR["rec_sell_price"].ToString())).ToString();
                Paid_TB.Text = DR["trs_Paid"].ToString();
                Discount_TB.Text = DR["trs_discount"].ToString();
                Get_Driver_Data();
                
                Rest_TB.Text = (decimal.Parse(Total_Paice_TB.Text) - decimal.Parse(Discount_TB.Text) - decimal.Parse(Paid_TB.Text)).ToString("0.00");

            }
            catch
            {


            }
        }

        private void Get_Driver_Data()
        {
            try
            {
                DB db = new DB();
                db.AddCondition("car_dri_id", Driver_CB.SelectedValue);
                Car_CB.SelectedValue = db.Select("select car_id from cars");
                db.Conditions[0].Name = "dri_id";
                DataRow dr = db.SelectRow(@"select p1.per_mobile dri_mobile ,p2.per_name,p2.per_mobile,p2.per_address from drivers d
                                            join persons p1 on p1.per_id= d.dri_per_id 
                                            left join persons p2 on p2.per_id= d.spr_per_id ");
                Driver_Mobile_TK.Text = dr["dri_mobile"].ToString();
                Sponsar_Mobile_tk.Text = dr["per_mobile"].ToString();
                Sponsar_Name.Text = dr["per_name"].ToString();
                Driver_Address_TK.Text = dr["per_address"].ToString();
            }
            catch
            {

            }
        }

        private void Get_Receipt_Number()
        {
            try
            {
                DB db = new DB("receipt");
                db.AddCondition("rec_sup_id", Supplier_CB.SelectedValue);
                Object num = db.Select("select max(rec_number) from receipt");

                if (num.ToString() == "")
                {
                    Receipt_Number_TK.Text = Properties.Settings.Default.Fash_number;
                }
                else
                {
                    Receipt_Number_TK.Text = (int.Parse(num.ToString()) + 1).ToString();
                }
            }
            catch
            {

            }
        }

        private void Get_Receipt_Prices()
        {
            decimal[] Cement_price = new decimal[2];


            try
            {
                Cement_price = Get_Cement_price();
                Unit_Price_TB.Text = (Cement_price[0]).ToString("0.000000");
                Total_Paice_TB.Text = ((Cement_price[0]) * decimal.Parse(Amount_TB.Text)).ToString("0.00");                
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

                DataRow dr = db2.SelectRow(@"select ctp_sell_price,ctp_buy_price from cement_price where ctp_date=(select max(ctp_date) from cement_price where ctp_date<=@ctp_date and ctp_sup_id=@ctp_sup_id
                                             and ctp_unit_id=@ctp_unit_id and ctp_cem_id=@ctp_cem_id) and ctp_unit_id=@ctp_unit_id and ctp_cem_id=@ctp_cem_id and ctp_sup_id=@ctp_sup_id");

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
                    if (!string.IsNullOrWhiteSpace(Receipt_Number_TK.Text))
                    {
                        DataBase.AddColumn("rec_number", Receipt_Number_TK.Text);
                    }
                    else
                    {
                        DataBase.AddColumn("rec_number", null);
                    }
                    if (DataBase.IsNotExist("rec_id", "rec_number", "rec_Sup_id"))
                    {

                        if (DataBase.Insert())
                        {
                            Receipt_Id = DataBase.Last_Inserted;
                            if (Add_Update_transportation(Receipt_Id))
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
                        Message.Show("لقد تم تسجيل هذا المستند من قبل", MessageBoxButton.OK, 5);
                        return false;
                    }


                }
                else
                {
                    DataBase.AddCondition("rec_id", Receipt_Id);
                    if (DataBase.Update())
                    {
                        return Confirm.Check(Add_Update_transportation(Receipt_Id));
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

        private bool Add_Update_transportation(object rec_id)
        {

            try
            {
                DB DB = new DB("transportation");
                DB.AddColumn("trs_rec_id", rec_id);
                DB.AddColumn("trs_date", Date_TB.Value.Value.Date);
                DB.AddColumn("trs_dri_id", Driver_CB.SelectedValue);
                DB.AddColumn("trs_car_id", Car_CB.SelectedValue);
                DB.AddColumn("trs_paid", Paid_TB.Text.Trim());
                DB.AddColumn("trs_discount", Discount_TB.Text.Trim());
                DB.AddColumn("trs_card_number", Card_Number_TB.Text.Trim());
                DB.AddColumn("trs_payment_method", Payment_Method_TB.Text.Trim());
                DB.AddColumn("trs_load_type", "أسمنت");
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


                if (Notify.validate("من فضلك ادخل التاريخ", Date_TB.Text, Cement_Office.GetWindow(this)))
                {
                    return;
                }

                if (Notify.validate("من فضلك اختر نوع الاسمنت", Cement_CB.SelectedIndex, Cement_Office.GetWindow(this)))
                {
                    return;
                }
                if (Notify.validate("من فضلك اختر الوحده", Unit_CB.SelectedIndex, Cement_Office.GetWindow(this)))
                {
                    return;
                }

                if (Notify.validate("من فضلك ادخل الكميه", Amount_TB.Text, Cement_Office.GetWindow(this)))
                {
                    return;
                }
                if (Notify.validate("من فضلك اختر السائق", Driver_CB.SelectedIndex, Cement_Office.GetWindow(this)))
                {
                    return;
                }

                if (Notify.validate("من فضلك اختر السياره", Car_CB.SelectedIndex, Cement_Office.GetWindow(this)))
                {
                    return;
                }




                if (Add_Update())
                {
                    var log = new Log();
                    log.Columns.Add(new Column("رقم الفسح", Receipt_Number_TK.Text));
                    log.CreateLog("الفسوحات", Receipt_Id == null);

                    Is_Editable = false;
                    this.Receipt_Id = null;
                    App.Clear_Form(Main_GD);
                    Get_Receipt_Number();
                    Is_Editable = true;
                }


            }
            catch
            {
                return;
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                Driver_Search d = new Driver_Search();
                d.ShowDialog();
                Driver_CB.SelectedValue = d.Driver_Id;
                Get_Driver_Data();
            }
            catch
            {

            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            try
            {
                Add_Driver d = new Add_Driver();
                d.ShowDialog();
                Drivers.Get_All_Drivers(Driver_CB, false);
                Cars.Get_All_Cars(Car_CB, false);
            }
            catch
            {

            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            try
            {
                object dri_id = Driver_CB.SelectedValue;
                Add_Driver d = new Add_Driver(Driver_CB.SelectedValue);
                d.ShowDialog();
                Drivers.Get_All_Drivers(Driver_CB, false);
                Cars.Get_All_Cars(Car_CB, false);
                Driver_CB.SelectedValue = dri_id;
                Get_Driver_Data();
            }
            catch
            {

            }
        }

        private void Cement_CB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (this.Is_Editable)
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
                if (this.Is_Editable)
                    Get_Receipt_Prices();
            }
            catch
            {

            }
        }

        private void Driver_CB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (this.IsLoaded)
                {
                    Get_Driver_Data();
                }

            }
            catch
            {

            }
        }

        private void Paid_TB_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                Rest_TB.Text = (decimal.Parse(Total_Paice_TB.Text) - decimal.Parse(Discount_TB.Text) - decimal.Parse(Paid_TB.Text)).ToString("0.00");
            }
            catch
            {

            }
        }

        private void Supplier_CB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Get_Receipt_Number();
            if (this.Is_Editable)
                Get_Receipt_Prices();
        }


    }
}
