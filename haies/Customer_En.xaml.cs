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
    /// Interaction logic for Product.xaml
    /// </summary>
    public partial class Customer_En : Page
    {
        Customer_type Customer_Type;
        public Customer_En(Customer_type customer_Type)
        {
            InitializeComponent();
            Customer_Type = customer_Type;
            if (Customer_Type == Customer_type.محطة)
            {
                Footer_GD.ColumnDefinitions[1].Width = new GridLength(0);
                Customers_Out_DG.Visibility = System.Windows.Visibility.Collapsed;
                Customer_Out_DG.Visibility = System.Windows.Visibility.Visible;
            }
            Payment_CB.ItemsSource = Enum.GetValues(typeof(Customer_Payment));
            Fill_Customers_LB();
        }

        private void Fill_Customers_LB()
        {

            DB db2 = new DB("person");

            // filter by name
            db2.AddCondition("cust_type", Customer_Type);

            // search by name
            db2.AddCondition("per_name", "%" + Name_Search_TB.Text + "%", false, " like ");

            // search by mobile
            db2.AddCondition("per_mobile", "%" + Mobile_Search_TB.Text + "%", false, " like ");


            db2.Fill(LB, "cust_id", "per_name", "select p.*,caro.* from persons p join customer caro on per_id=cust_per_id");


        }

        private decimal Get_Balance(DateTime DateTime)
        {
            decimal Balance = 0;
            try
            {

                DB db = new DB();
                db.AddCondition("cstl_date", DateTime.Date, false, "<", "Date");
                db.AddCondition("cstl_cust_id", LB.SelectedValue, false, "=", "cst_id");
                if (Customer_Type == Customer_type.مصنع)
                {
                    decimal.TryParse(db.Select(@"select COALESCE(sum(cstl_value),0) -(select COALESCE(sum((rec_sell_price*rec_amount)+(COALESCE(trs_sell_price,0)*COALESCE(rec_amount,1))
                                                    -trs_discount),0) from transportation left join receipt on rec_id= trs_rec_id where trs_date<@Date and trs_cust_id=@cst_id)
                                                     from customer_loans where cstl_date<@Date and cstl_cust_id=@cst_id; ").ToString(), out Balance);
                }
                else
                {
                    decimal.TryParse(db.Select(@"select COALESCE(sum(cstl_value),0) -(select COALESCE(sum(sin_cost),0) from station_income 
                                                 where sin_date<@Date and sin_cust_id=@cst_id)
                                                 from customer_loans where cstl_date<@Date and cstl_cust_id=@cst_id; ").ToString(), out Balance);

                }
            }
            catch
            {

            }
            return Balance * -1;
        }

        private void Get_Customers_Accounts()
        {

            DB db = new DB();
            DataSet ds = new DataSet();
            decimal[] Totals = new decimal[] { 0, 0, 0, 0, 0 };
            try
            {
                db.AddCondition("trs_date", From_DTP.Value.Value.Date, false, ">=", "SD");
                db.AddCondition("trs_date", To_DTP.Value.Value.Date, false, "<=", "ED");
                db.AddCondition("cust_id", LB.SelectedValue);
                if (Customer_Type == Customer_type.مصنع)
                {
                    ds = db.SelectSet(@"select rec_id,rec_number,trs_date,COALESCE(rec_amount,0),trs_paid,trs_discount,c.cem_name,unit_name,pl.pl_name,
                                                p.per_name customer,p2.per_name driver,cs.car_number, COALESCE(rec_sell_price,0)+COALESCE(trs_sell_price,0) unit_price,
                                                format(COALESCE(rec_sell_price*rec_amount,0)+(COALESCE(trs_sell_price,0)*COALESCE(rec_amount,1))-trs_discount,2) total_price,                                     
                                                format(COALESCE(rec_sell_price*rec_amount,0)+(COALESCE(trs_sell_price,0)*COALESCE(rec_amount,1))-trs_discount-trs_paid,2) trs_rest 
                                                from transportation t                                                                                            										      
                                                join drivers d on d.dri_id=trs_dri_id
                                                join cars cs on trs_car_id=car_id 
                                                join persons p2 on p2.per_id=d.dri_per_id                                         
                                                left join customer cu on cust_id = trs_cust_id 
                                                left join persons p on p.per_id = cust_per_id                                            
                                                left join places pl on pl.pl_id=t.trs_pl_id              
                                                left join  receipt r on t.trs_rec_id=r.rec_id  
                                                left join cement c on cem_id=rec_cem_id 
                                                left join units on rec_unit_id = unit_id                                
                                                where trs_date>=@SD and trs_date<=@ED and trs_cust_id=@cust_id ;
                                                select * from customer_loans where cstl_date>=@SD and cstl_date<=@ED and cstl_cust_id=@cust_id");
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        Totals[1] += decimal.Parse(row["total_price"].ToString());
                        Totals[2] += decimal.Parse(row["trs_paid"].ToString());
                    }
                }
                else
                {
                    ds = db.SelectSet(@"select s.*,gas_name from station_income s join gas on gas_id=sin_gas_id where sin_date>=@SD and sin_date<=@ED and sin_cust_id=@cust_id;
                                        select * from customer_loans where cstl_date>=@SD and cstl_date<=@ED and cstl_cust_id=@cust_id");
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        Totals[1] += decimal.Parse(row["sin_cost"].ToString());
                    }

                }
                foreach (DataRow row in ds.Tables[1].Rows)
                {
                    Totals[2] += decimal.Parse(row["cstl_value"].ToString());
                }
                if (Customer_Type == Customer_type.مصنع)
                {
                    Customers_Out_DG.ItemsSource = ds.Tables[0].DefaultView;
                }
                else
                {
                    Customer_Out_DG.ItemsSource = ds.Tables[0].DefaultView;
                }
                Customers_In_DG.ItemsSource = ds.Tables[1].DefaultView;
                Totals[0] = Get_Balance(From_DTP.Value.Value.Date);
                Totals[3] = Totals[1] - Totals[2];
                Totals[4] = Totals[3] + Totals[0];
                Balance_Before_TB.Text = Totals[0].ToString("0.00");
                Total_TB.Text = Totals[1].ToString("0.00");
                Paid_TB.Text = Totals[2].ToString("0.00");
                Rest_TB.Text = Totals[3].ToString("0.00");
                Balance_After_TB.Text = Totals[4].ToString("0.00");
            }
            catch
            {

            }
        }


        public static void Get_All_Customers(ComboBox CB, Customer_type cust_typ, string all = "")
        {

            try
            {
                DB db2 = new DB("persons");

                db2.AddCondition("cust_type", cust_typ);

                db2.Fill(CB, "cust_id", "per_name", "select p.*,c.* from persons p join customer c on c.cust_per_id=p.per_id", all);

            }

            catch
            {

            }
        }

        private bool Add_Update()
        {

            object person_id = null;

            try
            {

                DB DataBase = new DB("persons");

                DataBase.AddColumn("per_name", Name_TB.Text.Trim());
                DataBase.AddColumn("per_address", Address_TB.Text.Trim());
                DataBase.AddColumn("per_mobile", Mobile_TB.Text.Trim());

                if (LB.SelectedIndex == -1)
                {
                    if (DataBase.IsNotExist("per_id", "per_name", "per_mobile"))
                    {

                        if (DataBase.Insert())
                        {
                            person_id = DataBase.Last_Inserted;

                            if (Add_Update_Customer(person_id))
                            {
                                return true;
                            }

                            else
                            {
                                return false;
                            }
                        }
                        else
                        {
                            return false;
                        }

                    }
                    else
                    {
                        Message.Show("هذا الاسم مسجل من فضلك اختار اسم اخر ", MessageBoxButton.OK, 10);
                        return false;
                    }

                }

// hena ye3ny hwa mawgod ba3mel edit
                else
                {
                    DataBase.AddCondition("per_id", ((DataRowView)LB.SelectedItem)["per_id"]);
                    if (DataBase.Update())
                    {

                        if (Add_Update_Customer(((DataRowView)LB.SelectedItem)["per_id"]))
                        {
                            return true;
                        }

                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        private bool Add_Update_Customer(object person_idd)
        {

            try
            {
                DB DB = new DB("customer");
                DB.AddColumn("cust_per_id", person_idd);
                DB.AddColumn("cust_payment", Payment_CB.SelectedValue);
                DB.AddColumn("cust_type", Customer_Type);



                if (DB.IsNotExist("cust_id", "cust_per_id"))
                {
                    return Confirm.Check(DB.Insert());
                }
                else
                {
                    DB.AddCondition("cust_per_id", person_idd);
                    return Confirm.Check(DB.Update());
                }

            }
            catch
            {
                //MessageBox.Show("hena el error");
                return false;
            }
        }

        private void EditPanel_Add(object sender, EventArgs e)
        {
            try
            {
                App.Set_Style(Main_Grid, Operations.Add);
                Main_Grid.RowDefinitions[2].Height = new GridLength(35);
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
                if (LB.SelectedIndex != -1)
                {
                    App.Set_Style(Main_Grid, Operations.Edit);
                    Main_Grid.RowDefinitions[2].Height = new GridLength(35);
                    LB.IsEnabled = false;
                }
            }
            catch
            {

            }
        }

        private void EditPanel_Delete(object sender, EventArgs e)
        {
            try
            {
                if (LB.SelectedIndex != -1)
                {
                    if (Message.Show("هل تريد حذف هذا الشخص", MessageBoxButton.YesNoCancel, 5) == MessageBoxResult.Yes)
                    {
                        DB db = new DB("persons");
                        db.AddCondition("per_id", ((DataRowView)LB.SelectedItem)["per_id"]);
                        db.Delete();
                        Fill_Customers_LB();

                    }
                }
            }
            catch
            {

            }
        }

        private void Name_Search_TB_TextChanged(object sender, TextChangedEventArgs e)
        {
            Fill_Customers_LB();
        }

        private void Save_Panel_Save(object sender, EventArgs e)
        {
            try
            {
                if (Notify.validate("من فضلك ادخل الاسم", Name_TB.Text, Station.GetWindow(this)))
                {
                    return;
                }

                if (Notify.validate("من فضلك ادخل التعامل", Payment_CB.SelectedIndex, Station.GetWindow(this)))
                {
                    return;
                }

                App.Set_Style(Main_Grid, Operations.View);

                Main_Grid.RowDefinitions[2].Height = new GridLength(0);
                LB.IsEnabled = true;
                Add_Update();

                int i = LB.SelectedIndex;
                Fill_Customers_LB();
                LB.SelectedIndex = i;
            }
            catch
            {

            }
        }

        private void Save_Panel_Cancel(object sender, EventArgs e)
        {
            try
            {
                App.Set_Style(Main_Grid, Operations.View);
                Main_Grid.RowDefinitions[2].Height = new GridLength(0);
                LB.IsEnabled = true;
            }
            catch
            {

            }
        }

        private void From_DTP_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            try
            {
                if (this.IsLoaded)
                    Get_Customers_Accounts();
            }
            catch
            {

            }
        }

        private void EP_Edit(object sender, EventArgs e)
        {
            try
            {
                if ((LB.SelectedIndex != -1) && (Customers_In_DG.SelectedIndex != -1))
                {
                    Customer_Loans c = new Customer_Loans(LB.SelectedValue, ((DataRowView)Customers_In_DG.SelectedItem)["cstl_id"]);
                    c.ShowDialog();
                    Get_Customers_Accounts();
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
                if (Customers_In_DG.SelectedIndex != -1)
                {
                    if (Message.Show("هل تريد الحذف", MessageBoxButton.YesNoCancel, 10) == MessageBoxResult.Yes)
                    {
                        DB db = new DB("customer_loans");
                        db.AddCondition("cstl_id", ((DataRowView)Customers_In_DG.SelectedItem)["cstl_id"]);
                        db.Delete();
                        Get_Customers_Accounts();
                    }
                }
            }
            catch
            {

            }
        }

        private void EP_Add(object sender, EventArgs e)
        {
            try
            {
                if (LB.SelectedIndex != -1)
                {
                    Customer_Loans c = new Customer_Loans(LB.SelectedValue);
                    c.ShowDialog();
                    Get_Customers_Accounts();
                }
            }
            catch
            {

            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Cement_prices c = new Cement_prices(((DataRowView)LB.SelectedItem)["per_id"]);
                c.ShowDialog();
            }
            catch
            {

            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                if (LB.SelectedIndex != -1)
                {
                    Places_prices pp = new Places_prices(((DataRowView)LB.SelectedItem)["per_id"], "Customers");
                    pp.ShowDialog();
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
                Get_Customers_Accounts();
            }
            catch
            {

            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            try
            {
                CPrinting.CPrinting print = new CPrinting.CPrinting();
                print.header.Add(string.Format(" كشف حساب  {0} \r\n عن الفترة من {1} إلى {2}", ((DataRowView)LB.SelectedItem)["per_name"],
                    From_DTP.Value.Value.Date.ToString("yyyy/MM/dd"), To_DTP.Value.Value.Date.ToString("yyyy/MM/dd")));
                if (Customer_Type == Customer_type.مصنع)
                {
                    print.PrintDocument.DefaultPageSettings.Landscape = true;
                    App.Get_Printed_Table(print, Customers_Out_DG, Customers_In_DG, new string[] { "المصروفات", "الإيرادات" });
                    print.ColumnsLine.Add("trs_paid");
                }
                else
                {
                    App.Get_Printed_Table(print, Customer_Out_DG, Customers_In_DG, new string[] { "المصروفات", "الإيرادات" });
                    print.ColumnsLine.Add("sin_cost");
                }

                print.FooterTable.Add("المستحق سـابقاً : ", Balance_Before_TB.Text);
                print.FooterTable.Add("الإجمالــــــــــي : ", Total_TB.Text);
                print.FooterTable.Add("المــدفــــــــــوع : ", Paid_TB.Text);
                print.FooterTable.Add("البــــــــــــــاقـي : ", Rest_TB.Text);
                print.FooterTable.Add("المستحق حاليـاً : ", Balance_After_TB.Text);

                print.print();
            }
            catch
            {

            }
        }

    }
}
