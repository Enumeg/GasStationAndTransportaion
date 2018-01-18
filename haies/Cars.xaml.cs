using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using Source;
using System.Globalization;
using Microsoft.Windows.Controls;
namespace haies
{
    /// <summary>
    /// Interaction logic for Cars.xaml
    /// </summary>
    public partial class Cars : Page
    {
        public Cars()
        {
            InitializeComponent();
            Car_Owners.Get_All_Car_Owners(Owner_CB);
            Drivers.Get_All_Drivers(Driver_CB, true);

            Status_CB.ItemsSource = Enum.GetValues(typeof(Status));
            Status_CB.SelectedIndex = 1;

        }

        private void Fill_Cars_LB()
        {
            try
            {

                DB db = new DB();
                db.AddCondition("car_name", Name_Search_TB.Text, false, " like ");
                db.AddCondition("car_number", Num_Search_TB.Text, false, " like ");
                db.AddCondition("p1.per_name", Owner_Search_TB.Text, false, " like ", "pn1");
                db.AddCondition("p2.per_name", Driver_Search_TB.Text, false, " like ", "pn2");
                db.AddCondition("car_status", Status_CB.SelectedIndex);
                db.Fill(LB, "car_id", "car_name", @"select c.* from Cars c join drivers d on d.dri_id=c.car_dri_id join persons p1 on p1.per_id=d.dri_per_id and p1.per_name like @pn2
                                                                      join car_owner co on co.cor_id=c.car_cor_id join persons p2 on p2.per_id=co.cor_per_id and p2.per_name like @pn1");
            }
            catch
            {

            }
        }

        private void Get_Cars_Date()
        {
            try
            {
                DB db = new DB();
                db.AddCondition("ED", DateTime.Now.AddMonths(2));
                if (db.Select("select car_id from cars where car_application_date<=@ED or car_insurance_date<=@ED or car_card_date<=@ED or car_examination_date<=@ED") != null)
                {
                    if (Message.Show("يوجد بعض السيارات تخطت تاريخ إنتهاء أوراقها هل تريد الأطلاع على تقرير بها", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        Cars_Dates c = new Cars_Dates();
                        c.ShowDialog();
                    }
                }
            }
            catch
            {

            }
        }

        public static void Get_All_Cars(ComboBox CB, bool Is_Company_Car, string All = "")
        {
            try
            {

                DB db = new DB();

                db.AddCondition("car_cor_id", null, !Is_Company_Car, " is not ");
                db.AddCondition("car_status", Status.فعال);

                db.Fill(CB, "car_id", "car_number", @"select car_id, car_number from Cars order by car_number", All);
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
                db.AddCondition("cot_date", DateTime.Date, false, "<", "Date");
                db.AddCondition("cstl_cust_id", LB.SelectedValue, false, "=", "car_id");
                DB db2 = new DB("car_owner");
                db2.AddCondition("c.car_id", LB.SelectedValue);
                if (db2.Select("select co.cor_payment from car_owner co join cars c on c.car_cor_id=co.cor_id").ToString() == "0")
                {
                    decimal.TryParse(db.Select(@"select COALESCE(sum(cot_value),0)+(select COALESCE(sum(sin_cost),0) from station_income join customer on cust_id=sin_cust_id
                                                 join cars on cust_car_id=car_id where sin_date<@Date and car_id=@car_id)
                                                 +(select COALESCE(sum(trc_value),0) from transactions where trc_ref = 1 and trc_person=@car_id and trc_date<@Date) 
                                                 +( select COALESCE(sum(trs_dri_motive),0) from transportation left join receipt on rec_id=trs_rec_id 
                                                 join cars on car_id=trs_car_id where trs_date<@Date and trs_car_id=@car_id)
                                                 -(select sum(Round(COALESCE(rec_amount,1)*trs_buy_price,2)) 
                                                 from transportation left join receipt on trs_rec_id=rec_id                                                                                        
                                                 where trs_date<@Date and trs_car_id=@car_id)
                                                 from car_outcome where cot_date<@Date and cot_car_id=@car_id; ").ToString(), out Balance);
                }
                else
                {
                    decimal.TryParse(db.Select(@"select COALESCE(sum(cot_value),0)+(select COALESCE(sum(sin_cost),0) from station_income join customer on cust_id=sin_cust_id
                                                 join cars on cust_car_id=car_id where sin_date<@Date and car_id=@car_id)  
                                                +(select COALESCE(sum(trc_value),0) from transactions where trc_ref = 1 and trc_person=@car_id and trc_date<@Date) 
                                                -(select sum(Round(COALESCE(rec_amount,1)*trs_buy_price,2))
                                                 from transportation left join receipt on trs_rec_id=rec_id                                                                                        
                                                 where trs_date<@Date and trs_car_id=@car_id)                                           
                                                 from car_outcome where cot_date<@Date and cot_car_id=@car_id; ").ToString(), out Balance);
                }
            }
            catch
            {

            }
            return Balance * -1;
        }

        private void Get_Cars_Accounts()
        {
            try
            {
                decimal Balance = Get_Balance(From_DTP.Value.Value.Date);
                Get_Car_Income();
                Get_Cars_Outcome();
                Balance_Before_TB.Text = "الدخل سابقا : " + Balance.ToString("0.00");
                Balance_After_TB.Text = "الدخل حاليا : " + (Balance + decimal.Parse(Total_In_TB.Text.Split(':')[1]) - decimal.Parse(Total_Out_TB.Text.Split(':')[1])).ToString();
            }
            catch
            {

            }
        }

        private void Get_Car_Income()
        {
            decimal Total = 0;
            try
            {
                DB db = new DB();
                db.AddCondition("trs_car_id", LB.SelectedValue, false);
                db.AddCondition("trs_date", From_DTP.Value.Value.Date, false, ">=", "SD");
                db.AddCondition("trs_date", To_DTP.Value.Value.Date, false, "<=", "ED");
                DataTable dt = db.SelectTable(@"select rec_number,trs_date,cem_name,Round(COALESCE(rec_amount,1)*trs_buy_price,2)  trs_buy_price,pl_name from Transportation 
                                                left join receipt on trs_rec_id=rec_id 
                                                left join cement c on cem_id=rec_cem_id 
                                                join places on pl_id=trs_pl_id order by trs_date,rec_number");

                foreach (DataRow row in dt.Rows)
                {
                    Total += decimal.Parse(row["trs_buy_price"].ToString());
                }
                Total_In_TB.Text = "إجمالي الإيرادات : " + Total.ToString("0.00");

                Car_In_DG.ItemsSource = dt.DefaultView;
            }
            catch
            {

            }

        }

        private void Get_Cars_Outcome()
        {
            try
            {
                decimal Total = 0;
                Total_Out_TB.Text = "الإجمالي : " + Total.ToString("0.00");

                Car_Out_DG.ItemsSource = null;
                DB db = new DB();
                db.AddCondition("cot_car_id", LB.SelectedValue, false, "=", "car_id");
                db.AddCondition("cstl_date", From_DTP.Value.Value.Date, false, ">=", "SD");
                db.AddCondition("cstl_date", To_DTP.Value.Value.Date, false, "<=", "ED");
                DataSet ds = db.SelectSet(@"select * from car_outcome where cot_date>=@SD and cot_date<=@ED and cot_car_id=@car_id union all
                                            select trc_id,trc_person,trc_date,trc_value,trc_description from transactions where trc_ref = 1 and trc_person=@car_id and 
                                            trc_date>=@SD and trc_date<=@ED order by cot_date;                                            
                                            select sum(sin_amount),COALESCE(sum(sin_cost),0) from station_income join customer on cust_id=sin_cust_id 
                                            join cars on cust_car_id=car_id where sin_date>=@SD and sin_date<=@ED and car_id=@car_id ;
                                            select COALESCE(sum(trs_dri_motive),0) from transportation left join receipt on rec_id=trs_rec_id 
                                            join cars on car_id=trs_car_id where trs_date>=@SD and trs_date<=@ED and trs_car_id=@car_id");
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    Total += decimal.Parse(row["cot_value"].ToString());
                }
                if (decimal.Parse(ds.Tables[1].Rows[0][1].ToString()) != 0)
                {
                    DataRow dr = ds.Tables[0].NewRow();
                    dr["cot_value"] = ds.Tables[1].Rows[0][1];
                    dr["cot_type"] = string.Format("( {0} {1} )", ds.Tables[1].Rows[0][0].ToString(), " لتر ديزل");
                    Total += decimal.Parse(ds.Tables[1].Rows[0][1].ToString());
                    ds.Tables[0].Rows.InsertAt(dr, 0);
                }
                DB db2 = new DB("car_owner");
                db2.AddCondition("c.car_id", LB.SelectedValue);
                if (db2.Select("select co.cor_payment from car_owner co join cars c on c.car_cor_id=co.cor_id").ToString() == "0")
                {
                    if (decimal.Parse(ds.Tables[2].Rows[0][0].ToString()) != 0)
                    {
                        DataRow dr = ds.Tables[0].NewRow();
                        dr["cot_value"] = ds.Tables[2].Rows[0][0];
                        dr["cot_type"] = "حافز السائق";
                        Total += decimal.Parse(ds.Tables[2].Rows[0][0].ToString());
                        ds.Tables[0].Rows.Add(dr);
                    }
                }
                Total_Out_TB.Text = "إجمالي المصروفات : " + Total.ToString("0.00");

                Car_Out_DG.ItemsSource = ds.Tables[0].DefaultView;
            }
            catch
            {

            }
        }

        private bool Add_Update_Cars()
        {
            try
            {
                DB db = new DB("cars");
                db.AddColumn("car_number", Num_TB.Text.Trim());
                db.AddColumn("car_name", Name_TB.Text.Trim());
                db.AddColumn("car_cor_id", Owner_CB.SelectedValue);
                db.AddColumn("car_dri_id", Driver_CB.SelectedValue);
                db.AddColumn("car_application_date", Application_DTP.Value.Value.Date);
                db.AddColumn("car_insurance_date", Insurance_DTP.Value.Value.Date);
                db.AddColumn("car_card_date", Card_DTP.Value.Value.Date);
                db.AddColumn("car_examination_date", Examination_DTP.Value.Value.Date);
                if (LB.SelectedIndex == -1)
                {
                    if (db.IsNotExist("car_id", "car_number"))
                    {
                        return Confirm.Check(db.Insert());
                    }
                    else
                    {
                        Message.Show("لقد تم تسجيل سيارة بنفس الرقم من قبل", MessageBoxButton.OK, 5);
                        return false;
                    }
                }
                else
                {
                    db.AddCondition("car_id", LB.SelectedValue);
                    return Confirm.Check(db.Update());
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
                //Application_DTP.CultureInfo.DateTimeFormat.Calendar = new HijriCalendar();
                App.Set_Style(Main_Grid, Operations.Add);
                LB.IsEnabled = false;
                LB.SelectedIndex = -1;
                Main_Grid.RowDefinitions[4].Height = new GridLength(35);
            }
            catch
            {

            }
        }

        private void EditPanel_Edit(object sender, EventArgs e)
        {
            try
            {
                //Application_DTP.CultureInfo.DateTimeFormat.Calendar = new HijriCalendar();
                Main_Grid.DataContext = null;
                Main_Grid.DataContext = LB.SelectedItem;
                if (LB.SelectedIndex != -1)
                {
                    App.Set_Style(Main_Grid, Operations.Edit);
                    LB.IsEnabled = false;
                    Main_Grid.RowDefinitions[4].Height = new GridLength(35);
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
                    if (Message.Show("هل تريد حذف هذه السيارة", MessageBoxButton.YesNoCancel, 5) == MessageBoxResult.Yes)
                    {
                        DB db = new DB("Cars");
                        db.AddCondition("car_id", LB.SelectedValue);
                        if (db.Delete())
                        {
                            var log = new Log();
                            log.Columns.Add(new Column("الرقم", Num_TB.Text));
                            log.CreateLog("السيارات");
                        }
                        Fill_Cars_LB();
                    }
                }
            }
            catch
            {

            }
        }

        private void SavePanel_Save(object sender, EventArgs e)
        {
            try
            {

                if (Notify.validate("من فضلك ادخل الاسم", Name_TB.Text, MainWindow.GetWindow(this)))
                {
                    return;
                }

                if (Notify.validate("من فضلك ادخل الرقم", Num_TB.Text, MainWindow.GetWindow(this)))
                {
                    return;
                }

                if (Notify.validate("من فضلك ادخل المالك", Owner_CB.SelectedIndex, MainWindow.GetWindow(this)))
                {
                    return;
                }

                if (Notify.validate("من فضلك ادخل السائق", Driver_CB.SelectedIndex, MainWindow.GetWindow(this)))
                {
                    return;
                }

                if (Add_Update_Cars())
                {
                    var log = new Log();
                    log.Columns.Add(new Column("الرقم", Num_TB.Text));
                    log.CreateLog("السيارات", LB.SelectedIndex == -1);
                    App.Set_Style(Main_Grid, Operations.View);
                    LB.IsEnabled = true;
                    Main_Grid.RowDefinitions[4].Height = new GridLength(0);
                    Fill_Cars_LB();
                    //Application_DTP.CultureInfo.DateTimeFormat.Calendar = new GregorianCalendar();
                }
            }
            catch
            {

            }
        }

        private void SavePanel_Cancel(object sender, EventArgs e)
        {
            try
            {
                App.Set_Style(Main_Grid, Operations.View);
                LB.IsEnabled = true;
                Main_Grid.RowDefinitions[4].Height = new GridLength(0);
                Fill_Cars_LB();
                //Application_DTP.CultureInfo.DateTimeFormat.Calendar = new GregorianCalendar();
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

                    Car_Outcome c = new Car_Outcome((DataRowView)LB.SelectedItem);
                    c.ShowDialog();
                    Get_Cars_Accounts();
                }
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

                    Car_Outcome c = new Car_Outcome((DataRowView)LB.SelectedItem, ((DataRowView)Car_Out_DG.SelectedItem)["cot_id"]);
                    c.ShowDialog();
                    Get_Cars_Accounts();
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
                if (Car_Out_DG.SelectedIndex != -1)
                {
                    if (Message.Show("هل تريد الحذف", MessageBoxButton.YesNoCancel, 10) == MessageBoxResult.Yes)
                    {
                        DB db = new DB("Car_Outcome");
                        db.AddCondition("cot_id", ((DataRowView)Car_Out_DG.SelectedItem)["cot_id"]);
                        if (db.Delete())
                        {
                            var log = new Log();
                            log.Columns.Add(new Column("التاريـخ", ((DataRowView)Car_Out_DG.SelectedItem)[1]));
                            log.Columns.Add(new Column("القيـمــة", ((DataRowView)Car_Out_DG.SelectedItem)[2]));
                            log.Columns.Add(new Column("النوع", ((DataRowView)Car_Out_DG.SelectedItem)[3]));
                            log.Columns.Add(new Column("السيارة", Num_TB.Text));
                            log.CreateLog("مصروفات السيارات");
                        }
                        Get_Cars_Accounts();
                    }
                }
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
                    Get_Cars_Accounts();
            }
            catch
            {

            }
        }

        private void Name_Search_TB_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                Fill_Cars_LB();
            }
            catch
            {

            }
        }

        private void LB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                Get_Cars_Accounts();
                Main_Grid.DataContext = LB.SelectedItem;
            }
            catch
            {

            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CPrinting.CPrinting print = new CPrinting.CPrinting();
                print.header.Add(string.Format(" كشف حساب السيارة رقم {0} \r\n عن الفترة من {1} إلى {2}", ((DataRowView)LB.SelectedItem)["car_number"],
                    From_DTP.Value.Value.Date.ToString("yyyy/MM/dd"), To_DTP.Value.Value.Date.ToString("yyyy/MM/dd")));
                App.Get_Printed_Table(print, Car_In_DG, Car_Out_DG, new string[] { "الإيرادات", "المصروفات" });
                print.ColumnsLine.Add("trs_buy_price");
                print.FooterTable.Add("الدخل ســــابقـــــــاً : ", Balance_Before_TB.Text.Split(':')[1]);
                print.FooterTable.Add("إجمالـــــي الإيرادات : ", Total_In_TB.Text.Split(':')[1]);
                print.FooterTable.Add("إجمالي المصروفات : ", Total_Out_TB.Text.Split(':')[1]);
                print.FooterTable.Add("الدخل حـــــاليــــــاً : ", Balance_After_TB.Text.Split(':')[1]);
                print.print();
            }
            catch
            {

            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!App.Cars_IsLoaded)
                {
                    Get_Cars_Date();
                    App.Cars_IsLoaded = true;
                }
            }
            catch
            {

            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                Salary s = new Salary(LB.SelectedValue);
                s.ShowDialog();
            }
            catch
            {

            }
        }

        private void Archive_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ChangeStatus_BTN.Content = Status_CB.SelectedIndex == 1 ? "أرشيف" : "تفعيل";
            ChangeStatus_BTN.Tag = Status_CB.SelectedIndex == 0 ? "/haies;component/Images/Activate.ico" : "/haies;component/Images/Archive.ico";

            Fill_Cars_LB();
        }
        private void ChangeStatus_BTN_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var status = Status_CB.SelectedIndex == 0 ? 1 : 0;
                DB db = new DB("cars");
                db.AddColumn("car_status", status);
                db.AddCondition("car_id", LB.SelectedValue);
                Confirm.Check(db.Update());
                Fill_Cars_LB();
            }
            catch
            {


            }
        }


    }
}
