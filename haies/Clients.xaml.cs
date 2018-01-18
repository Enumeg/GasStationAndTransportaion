using System;
using System.Windows;
using System.Windows.Controls;
using Source;
using System.Data;

namespace haies
{
    /// <summary>
    /// Interaction logic for Clients.xaml
    /// </summary>
    public partial class Clients : Page
    {
        public Clients()
        {
            InitializeComponent();
            Fill_Clients_LB();
        }

        private void Fill_Clients_LB()
        {

            try
            {
                DB db2 = new DB("collector");


                // search by name
                db2.AddCondition("p1.per_name", "%" + Name_Search_TB.Text + "%", Name_Search_TB.Text == "", " like ");

                // search by mobile
                db2.AddCondition("p1.per_mobile", "%" + Phone_Search_TB.Text + "%", Phone_Search_TB.Text == "", " like ");


                db2.Fill(LB, "ID", "NAME", @"select d.dri_id ID,p1.per_id ID2_per,p1.per_name NAME, p1.per_mobile MOBO,p2.per_id IDO_per,p2.per_name NAMO,
                                             p1.per_address ADDO,p2.per_mobile MOBOO,c.car_number CARO 
                                             from drivers d join persons p1 on p1.per_id= d.dri_per_id 
                                             left join persons p2 on p2.per_id= d.spr_per_id 
                                             left join cars c on car_dri_id = dri_id where (p2.per_id<>1 OR p2.per_id is null) group by p1.per_id order by p1.per_name");
            }
            catch
            {

            }

        }

        private decimal Get_Balance(DateTime DateTime)
        {
            decimal Balance = 0;
            try
            {

                DB db = new DB();
                db.AddCondition("cstl_date", DateTime.Date, false, "<", "Date");
                db.AddCondition("cstl_cust_id", LB.SelectedValue, false, "=", "cst_id");

                decimal.TryParse(db.Select(@"select COALESCE(sum(cstl_value),0) -(select COALESCE(sum((rec_sell_price*rec_amount)+(COALESCE(trs_sell_price,0)*COALESCE(rec_amount,1))
                                                    -trs_discount),0)- COALESCE(sum(trs_paid),0) from transportation left join receipt on rec_id= trs_rec_id where trs_date<@Date and trs_dri_id=@cst_id)
                                                     from client_loans where cstl_date<@Date and cstl_dri_id=@cst_id; ").ToString(), out Balance);
            }
            catch
            {

            }
            return Balance * -1;
        }

        private void Get_Client_Accounts()
        {

            DB db = new DB();
            DataSet ds = new DataSet();
            decimal[] Totals = new decimal[] { 0, 0, 0, 0, 0 };
            try
            {
                db.AddCondition("trs_date", From_DTP.Value.Value.Date, false, ">=", "SD");
                db.AddCondition("trs_date", To_DTP.Value.Value.Date, false, "<=", "ED");
                db.AddCondition("dri_id", LB.SelectedValue);

                ds = db.SelectSet(@"select rec_id,rec_number,trs_date,COALESCE(rec_amount,0) rec_amount,trs_paid,trs_discount,c.cem_name,unit_name,pl.pl_name,
                                                cs.car_number, trs_payment_method,trs_card_number,
                                                Round(COALESCE(rec_sell_price,0)+COALESCE(trs_sell_price,0)-(trs_discount/COALESCE(rec_amount,1)),2) unit_price,
                                                Round(COALESCE(rec_sell_price*rec_amount,0)+(COALESCE(trs_sell_price,0)*COALESCE(rec_amount,1))-trs_discount,2) total_price,                                     
                                                Round(COALESCE(rec_sell_price*rec_amount,0)+(COALESCE(trs_sell_price,0)*COALESCE(rec_amount,1))-trs_paid,2) trs_rest 
                                                from transportation t                                                                                            										                                                      
                                                join cars cs on trs_car_id=car_id                                                                                                                                                                
                                                left join places pl on pl.pl_id=t.trs_pl_id              
                                                left join  receipt r on t.trs_rec_id=r.rec_id  
                                                left join cement c on cem_id=rec_cem_id 
                                                left join units on rec_unit_id = unit_id                                
                                                where trs_date>=@SD and trs_date<=@ED and trs_dri_id=@dri_id order by trs_date,rec_number;
                                                select * from client_loans where cstl_date>=@SD and cstl_date<=@ED and cstl_dri_id=@dri_id order by cstl_date");
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    Totals[1] += decimal.Parse(row["total_price"].ToString());
                    if (decimal.Parse(row["trs_paid"].ToString()) > 0)
                    {
                        ds.Tables[1].Rows.Add(null, null, row["trs_date"], row["trs_paid"], row["trs_payment_method"]);
                    }
                }

                foreach (DataRow row in ds.Tables[1].Rows)
                {
                    Totals[2] += decimal.Parse(row["cstl_value"].ToString());
                }

                Customers_Out_DG.ItemsSource = ds.Tables[0].DefaultView;
                ds.Tables[1].DefaultView.Sort = "cstl_date";
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

        private bool Add_Update()
        {
            object Sponser_ID = null, Driver_ID = null;
            try
            {
                if (LB.SelectedIndex == -1)
                {
                    Driver_ID = Add_Update_Person(Driver_Name_TB.Text, Driver_Address_TB.Text.Trim(), Driver_Mobile_TB.Text);
                    Sponser_ID = Add_Update_Person(Sponser_Name_TB.Text, "", Sponser_Mobile_TB.Text);

                    if (Driver_ID != null && Sponser_ID != null)
                    {
                        if (Add_Update_Driver(Sponser_ID, Driver_ID))
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
                    return (Add_Update_Person(Driver_Name_TB.Text, Driver_Address_TB.Text.Trim(), Driver_Mobile_TB.Text, Driver_Name_TB.Tag) != null &&
                     Add_Update_Person(Sponser_Name_TB.Text, "", Sponser_Mobile_TB.Text, Sponser_Name_TB.Tag) != null
                     && Add_Update_Cars(LB.SelectedValue));
                }
            }
            catch
            {
                return false;
            }
        }

        private bool Add_Update_Driver(object sponser_Id, object driver_Id)
        {
            try
            {
                DB DB = new DB("drivers");
                DB.AddColumn("dri_per_id", driver_Id);
                DB.AddColumn("spr_per_id", sponser_Id);
                DB.AddColumn("dri_salary", 0);
                if (DB.Insert())
                {
                    return Add_Update_Cars(DB.Last_Inserted);
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                //MessageBox.Show("hena el error");
                return false;
            }
        }

        private object Add_Update_Person(string Name, string Address, string Mobile, object per_id = null)
        {
            object Per_Id = null;
            try
            {
                DB DataBase = new DB("persons");
                DataBase.AddColumn("per_name", Name.Trim());
                DataBase.AddColumn("per_address", Address.Trim());
                DataBase.AddColumn("per_mobile", Mobile.Trim());
                if (per_id == null)
                {
                    if (DataBase.IsNotExist("per_id", "per_name"))
                    {
                        if (DataBase.Insert())
                        {
                            Per_Id = DataBase.Last_Inserted;
                        }
                    }
                    else
                    {
                        DB d = new DB();
                        d.AddCondition("per_name", Name.Trim());
                        Per_Id = d.Select("select per_id from persons ");

                        return true;
                    }
                }
                else
                {
                    DataBase.AddCondition("per_id", per_id);
                    if (DataBase.Update())
                    {
                        Per_Id = per_id;
                    }
                }

            }
            catch
            {

            }
            return Per_Id;
        }

        private bool Add_Update_Cars(object driver_Id)
        {
            try
            {
                if (Car_Number.Text != "")
                {
                    DB db = new DB("cars");
                    db.AddColumn("car_number", Car_Number.Text.Trim());
                    db.AddColumn("car_name", Car_Number.Text.Trim());
                    db.AddColumn("car_dri_id", driver_Id);
                    if (db.IsNotExist("car_id", "car_number"))
                    {
                        return (db.Insert());
                    }
                    else
                    {
                        db.AddCondition("car_number", Car_Number.Text.Trim());
                        return db.Update();
                    }
                }
                else
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        private void EditPanel_Add(object sender, EventArgs e)
        {
            try
            {
                //App.Set_Style(Main_Grid, Operations.Add);
                //Save_Panel.Visibility = System.Windows.Visibility.Visible;
                //LB.IsEnabled = false;
                //LB.SelectedIndex = -1;
                Add_Driver d = new Add_Driver();
                d.ShowDialog();
                Fill_Clients_LB();
            }
            catch
            {

            }
        }

        private void EditPanel_Edit(object sender, EventArgs e)
        {
            try
            {
                //if (LB.SelectedIndex != -1)
                //{
                //    App.Set_Style(Main_Grid, Operations.Edit);
                //    Save_Panel.Visibility = System.Windows.Visibility.Visible;
                //    LB.IsEnabled = false;
                //}

                Add_Driver d = new Add_Driver(LB.SelectedValue);
                d.ShowDialog();
                int i = LB.SelectedIndex;
                Fill_Clients_LB();
                LB.SelectedIndex = i;
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
                    if (Message.Show("هل تريد حذف هذا السائق", MessageBoxButton.YesNoCancel, 5) == MessageBoxResult.Yes)
                    {
                        DB db1 = new DB("drivers");
                        DB db2 = new DB("persons");
                        DB db3 = new DB("persons");
                        DB db4 = new DB("cars");

                        db4.AddCondition("car_dri_id", LB.SelectedValue);
                        db1.AddCondition("dri_id", LB.SelectedValue);

                        db2.AddCondition("per_id", ((DataRowView)LB.SelectedItem)["ID2_per"]);
                        db3.AddCondition("per_id", ((DataRowView)LB.SelectedItem)["IDO_per"]);


                        if (db4.Delete() && db2.Delete() && db3.Delete() && db1.Delete())
                        {
                            var log = new Log();
                            log.Columns.Add(new Column("الإسم", Driver_Name_TB.Text));
                            log.CreateLog("عملاء السيارات");

                            Fill_Clients_LB();
                        }

                        else
                        {

                        }

                    }
                }
            }
            catch
            {

            }
        }

        private void Name_Search_TB_TextChanged(object sender, TextChangedEventArgs e)
        {
            Fill_Clients_LB();
        }

        private void Save_Panel_Save(object sender, EventArgs e)
        {
            try
            {
                if (Notify.validate("من فضلك ادخل اسم السائق", Driver_Name_TB.Text, MainWindow.GetWindow(this)))
                {
                    return;
                }




                App.Set_Style(Main_Grid, Operations.View);

                Save_Panel.Visibility = System.Windows.Visibility.Collapsed;

                LB.IsEnabled = true;
                if (Add_Update())
                {
                    var log = new Log();
                    log.Columns.Add(new Column("الإسم", Driver_Name_TB.Text));
                    log.CreateLog("عملاء السيارات", LB.SelectedIndex == -1);

                    int i = LB.SelectedIndex;
                    Fill_Clients_LB();
                    LB.SelectedIndex = i;
                }
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
                Save_Panel.Visibility = System.Windows.Visibility.Collapsed;
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
                Get_Client_Accounts();
            }
            catch
            {

            }
        }

        private void LB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                Get_Client_Accounts();
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
                print.header.Add(string.Format(" كشف حساب  {0} \r\n عن الفترة من {1} إلى {2}", ((DataRowView)LB.SelectedItem)["NAME"],
                    From_DTP.Value.Value.Date.ToString("yyyy/MM/dd"), To_DTP.Value.Value.Date.ToString("yyyy/MM/dd")));

                print.PrintDocument.DefaultPageSettings.Landscape = true;
                App.Get_Printed_Table(print, Customers_Out_DG, Customers_In_DG, new string[] { "عدد الردود", "المدفوعات" });
                print.ColumnsLine.Add("trs_paid");


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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Future_Sales f = new Future_Sales(Future.Clients, Customer_type.مصنع);
                f.ShowDialog();
            }
            catch
            {

            }
        }


    }
}
