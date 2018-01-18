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
    public partial class Car_Owners : Page
    {
        public Car_Owners()
        {
            InitializeComponent();

            Deal_CB.ItemsSource = Enum.GetValues(typeof(Deal_Types));
            Maintenance_CB.ItemsSource = Enum.GetValues(typeof(Maintenance_Types));

            Status_CB.ItemsSource = Enum.GetValues(typeof(Status));
            Status_CB.SelectedIndex = 1;

        }

        private void Fill_Car_Owners_LB()
        {
            try
            {
                DB db2 = new DB("person");

                // search by name
                db2.AddCondition("per_name", "%" + Name_Search_TB.Text + "%", false, " like ");

                // search by mobile
                db2.AddCondition("per_mobile", "%" + Mobile_Search_TB.Text + "%", false, " like ");

                // search by Car Number
                //db2.AddCondition("car_number", "%" + Car_Search_TB.Text + "%", false, " like ");
                db2.AddCondition("per_status", Status_CB.SelectedIndex);


                db2.Fill(LB, "cor_id", "per_name", "select p.*,caro.* from persons p join car_owner caro on per_id=cor_per_id ");
            }
            catch
            {

            }
        }

        public static void Get_All_Car_Owners(ComboBox CB, string All = "")
        {

            try
            {
                DB db2 = new DB("person");
                db2.AddCondition("per_status", Status.فعال);
                db2.Fill(CB, "cor_id", "per_name", "select p.*,caro.* from persons p join car_owner caro on per_id=cor_per_id ", All);
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
                db.AddCondition("cstl_cust_id", LB.SelectedValue, false, "=", "cor_id");
                db.AddCondition("per_id", ((DataRowView)LB.SelectedItem)["per_id"]);
                if (Deal_CB.SelectedIndex == 0)
                {
                    decimal.TryParse(db.Select(@"select COALESCE(sum(cot_value),0)+(select COALESCE(sum(adv_value),0) from advance join persons on per_id=adv_per_id
                                                join car_owner on cor_per_id=per_id where adv_date<@Date and cor_id=@cor_id)
                                                +(select COALESCE(sum(trc_value),0) from transactions where trc_ref = 0 
                                                and trc_direction = 1 and trc_person=@per_id and trc_date <@Date)
                                                +(select COALESCE(sum(trc_value),0) from transactions
                                                join cars on car_id=trc_person
                                                join car_owner on cor_id=car_cor_id 
                                                where trc_ref = 1 and trc_date<@Date and cor_id=@cor_id) 
                                                +(select COALESCE(sum(trs_dri_motive),0) from transportation left join receipt on rec_id=trs_rec_id 
                                                join cars on car_id=trs_car_id
                                                join car_owner on cor_id=car_cor_id 
                                                where trs_date<@Date and cor_id=@cor_id)-
                                                (select COALESCE(sum(sin_cost),0) from station_income join customer on cust_id=sin_cust_id
                                                join cars on cust_car_id=car_id
                                                join car_owner on cor_id=car_cor_id
                                                where sin_date<@Date and cor_id=@cor_id) 
                                                -(select sum(Round(COALESCE(rec_amount,1)*trs_buy_price,2))
                                                from transportation left join receipt on trs_rec_id=rec_id                                           
                                                join cars on car_id=trs_car_id
                                                join car_owner on cor_id=car_cor_id
                                                where trs_date<@Date and cor_id=@cor_id)
                                                from car_outcome join cars on car_id=cot_car_id join car_owner on cor_id=car_cor_id 
                                                where cot_date<@Date and cor_id=@cor_id; ").ToString(), out Balance);
                }
                else
                {
                    decimal.TryParse(db.Select(@"select COALESCE(sum(cot_value),0)+(select COALESCE(sum(adv_value),0) from advance join persons on per_id=adv_per_id
                                                join car_owner on cor_per_id=per_id where adv_date<@Date and cor_id=@cor_id)
                                                +(select COALESCE(sum(trc_value),0) from transactions where trc_ref = 0 
                                                and trc_direction = 1 and trc_person=@per_id and trc_date <@Date)
                                                +(select COALESCE(sum(trc_value),0) from transactions
                                                join cars on car_id=trc_person
                                                join car_owner on cor_id=car_cor_id 
                                                where trc_ref = 1 and trc_date<@Date and cor_id=@cor_id) 
                                                -(select COALESCE(sum(sin_cost),0) from station_income join customer on cust_id=sin_cust_id
                                                join cars on cust_car_id=car_id
                                                join car_owner on cor_id=car_cor_id
                                                where sin_date<@Date and cor_id=@cor_id) 
                                                -(select sum(Round(COALESCE(rec_amount,1)*trs_buy_price,2))
                                                from transportation left join receipt on trs_rec_id=rec_id                                           
                                                join cars on car_id=trs_car_id
                                                join car_owner on cor_id=car_cor_id
                                                where trs_date<@Date and cor_id=@cor_id)
                                                from car_outcome join cars on car_id=cot_car_id join car_owner on cor_id=car_cor_id 
                                                where cot_date<@Date and cor_id=@cor_id; ").ToString(), out Balance);
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
                db.AddCondition("cor_id", LB.SelectedValue, false);
                db.AddCondition("trs_date", From_DTP.Value.Value.Date, false, ">=", "SD");
                db.AddCondition("trs_date", To_DTP.Value.Value.Date, false, "<=", "ED");
                DataTable dt = db.SelectTable(@"select rec_number,trs_date,Round(COALESCE(rec_amount,1)*trs_buy_price,2)  trs_buy_price,cem_name,  pl_name,car_number car  from transportation 
                                                left join receipt on trs_rec_id=rec_id 
                                                left join cement c on cem_id=rec_cem_id 
                                                join places on pl_id=trs_pl_id
                                                join cars on trs_car_id=car_id
                                                join car_owner on cor_id=car_cor_id
                                                order by trs_date,rec_number");

                foreach (DataRow row in dt.Rows)
                {
                    Total += decimal.Parse(row["trs_buy_price"].ToString());
                }
                Total_In_TB.Text = "إجمالـــــي الإيرادات : " + Total.ToString("0.00");
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
                Car_Out_DG.ItemsSource = null;
                decimal Total = 0;
                Total_Out_TB.Text = "إجمالي المصروفات : " + Total.ToString("0.00");

                DB db = new DB();
                db.AddCondition("cor_id", LB.SelectedValue, false);
                db.AddCondition("cstl_date", From_DTP.Value.Value.Date, false, ">=", "SD");
                db.AddCondition("cstl_date", To_DTP.Value.Value.Date, false, "<=", "ED");
                db.AddCondition("per_id", ((DataRowView)LB.SelectedItem)["per_id"]);
                DataSet ds = db.SelectSet(@"select cot.*,car_number from car_outcome cot join cars on car_id=cot_car_id join car_owner on cor_id=car_cor_id 
                                            where cot_date>=@SD and cot_date<=@ED and cor_id=@cor_id  group by car_id,cot_type union all
                                            select trc_id,trc_person,trc_date,trc_value,trc_description,trc_ref from transactions where trc_ref = 0 and trc_direction = 1 and
                                            trc_person=@per_id and trc_date>=@SD and trc_date<=@ED union all
                                            select trc_id,trc_person,trc_date,trc_value,trc_description,car_number from transactions 
                                            join cars on car_id=trc_person
                                            join car_owner on cor_id=car_cor_id 
                                            where trc_ref = 1 and trc_date>=@SD and trc_date<=@ED and cor_id=@cor_id order by cot_date;                                            
                                            select COALESCE(sum(sin_amount),0),COALESCE(sum(sin_cost),0),car_number from station_income 
                                            join customer on cust_id=sin_cust_id                                            
                                            join cars on cust_car_id=car_id
                                            join car_owner on cor_id=car_cor_id
                                            where sin_date>=@SD and sin_date<=@ED and cor_id=@cor_id group by car_id;
                                            select * from advance join persons on per_id=adv_per_id join car_owner on cor_per_id=per_id
                                            where adv_date>=@SD and adv_date<=@ED and cor_id=@cor_id order by adv_date;
                                            select COALESCE(sum(trs_dri_motive),0),car_number from transportation left join receipt on rec_id=trs_rec_id 
                                            join cars on car_id=trs_car_id
                                            join car_owner on cor_id=car_cor_id 
                                            where trs_date>=@SD and trs_date<=@ED and cor_id=@cor_id group by car_id;");

                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    Total += decimal.Parse(row["cot_value"].ToString());
                }
               
                foreach (DataRow row in ds.Tables[1].Rows)
                {
                    DataRow dr = ds.Tables[0].NewRow();
                    dr["cot_value"] = row[1];
                    dr["cot_type"] = string.Format("( {0} {1} )", row[0].ToString(), " لتر ديزل");
                    dr["car_number"] = row[2];
                    Total += decimal.Parse(row[1].ToString());
                    ds.Tables[0].Rows.InsertAt(dr, 0);
                }
                if (Deal_CB.SelectedIndex == 0)
                {
                    foreach (DataRow row in ds.Tables[3].Rows)
                    {
                        DataRow dr = ds.Tables[0].NewRow();
                        dr["cot_value"] = row[0];
                        dr["cot_type"] = "حافز السائق";
                        dr["car_number"] = row[1];
                        Total += decimal.Parse(row[0].ToString());
                        ds.Tables[0].Rows.Add(dr);
                    }
                }
                foreach (DataRow row in ds.Tables[2].Rows)
                {
                    ds.Tables[0].Rows.Add(row["adv_id"], null, row["adv_date"], row["adv_value"], row["adv_description"]);
                    Total += decimal.Parse(row["adv_value"].ToString());
                }
                Total_Out_TB.Text = "إجمالي المصروفات : " + Total.ToString("0.00");
                Car_Out_DG.ItemsSource = ds.Tables[0].DefaultView;
            }
            catch
            {

            }
            finally
            {

            }
        }

        private bool Add_Update_Person()
        {
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
                            if (Add_Update_Car_Owner(DataBase.Last_Inserted))
                            {
                                return true;
                            }

                            else
                            {
                                DB d = new DB("persons");
                                d.AddCondition("per_id", DataBase.Last_Inserted);
                                d.Delete();
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
                        Message.Show("هذا الاسم مسجل من فضلك اختار اسم اخر ", MessageBoxButton.OK, 5);
                        return true;
                    }



                }

// hena ye3ny hwa mawgod ba3mel edit
                else
                {
                    DataBase.AddCondition("per_id", ((DataRowView)LB.SelectedItem)["per_id"]);
                    if (DataBase.Update())
                    {
                        return (Add_Update_Car_Owner(((DataRowView)LB.SelectedItem)["per_id"]));
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch
            {
                //MessageBox.Show("kiki_method");
                return false;
            }
        }

        private bool Add_Update_Car_Owner(object person_idd)
        {

            try
            {
                DB DB = new DB("car_owner");

                DB.AddColumn("cor_per_id", person_idd);
                DB.AddColumn("cor_payment", Deal_CB.SelectedValue);
                DB.AddColumn("cor_maintenance", Maintenance_CB.SelectedValue);

                if (DB.IsNotExist("cor_id", "cor_per_id"))
                {
                    return Confirm.Check(DB.Insert());
                }
                else
                {
                    // 3alshan hwa mawgod 2abl keda

                    DB.AddCondition("cor_per_id", person_idd);
                    return Confirm.Check(DB.Update());

                }

            }
            catch
            {
                //MessageBox.Show("hena el error");
                return false;
            }
        }

        private void Name_Search_TB_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                Fill_Car_Owners_LB();
            }
            catch
            {

            }
        }

       
        
        private void EditPanel_Add(object sender, EventArgs e)
        {
            try
            {
                App.Set_Style(Main_Grid, Operations.Add);
                Main_Grid.RowDefinitions[3].Height = new GridLength(35);
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
                    Main_Grid.RowDefinitions[3].Height = new GridLength(35);
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
                        if (db.Delete())
                        {
                            var log = new Log();
                            log.Columns.Add(new Column("الإسم", Name_TB.Text));
                            log.CreateLog("ملاك السيارات");
                        }                           

                        Fill_Car_Owners_LB();

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
                if (Notify.validate("من فضلك اختر التعامل", Deal_CB.SelectedIndex,MainWindow.GetWindow(this) ))
                {
                    return;
                }
                if (Notify.validate("من فضلك اختر الصيانه", Maintenance_CB.SelectedIndex, MainWindow.GetWindow(this)))
                {
                    return;
                }


                if (Add_Update_Person())
                {
                    var log = new Log();                   
                    log.Columns.Add(new Column("الإسم", Name_TB.Text));
                    log.CreateLog("ملاك السيارات", LB.SelectedIndex == -1);

                    App.Set_Style(Main_Grid, Operations.View);

                    Main_Grid.RowDefinitions[3].Height = new GridLength(0);

                    LB.IsEnabled = true;

                    Fill_Car_Owners_LB();
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
                Main_Grid.RowDefinitions[3].Height = new GridLength(0);
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
                    Get_Cars_Accounts();
            }
            catch
            {

            }
        }

        // Places
        private void Button_Click(object sender, RoutedEventArgs e)
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

        
        // Advance
        private void EP_Edit(object sender, EventArgs e)
        {
            try
            {
                if (LB.SelectedIndex != -1)
                {
                    Advance c = new Advance(((DataRowView)LB.SelectedItem), ((DataRowView)Car_Out_DG.SelectedItem)[0]);
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
                        DB db = new DB("advance");
                        db.AddCondition("adv_id", ((DataRowView)Car_Out_DG.SelectedItem)[0]);
                        if (db.Delete())
                        {
                            var log = new Log();
                            log.Columns.Add(new Column("التاريـخ", ((DataRowView)Car_Out_DG.SelectedItem)[1]));
                            log.Columns.Add(new Column("القيـمــة", ((DataRowView)Car_Out_DG.SelectedItem)[2]));
                            log.Columns.Add(new Column("البيـــان", ((DataRowView)Car_Out_DG.SelectedItem)[3]));
                            log.Columns.Add(new Column("إسم المالك", Name_TB.Text));
                            log.CreateLog("دفعات ملاك السيارات");
                        }
                        Get_Cars_Accounts();
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
                    Advance c = new Advance(((DataRowView)LB.SelectedItem));
                    c.ShowDialog();
                    Get_Cars_Accounts();
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
                Get_Cars_Accounts();
            }
            catch
            {

            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                CPrinting.CPrinting print = new CPrinting.CPrinting();
                print.header.Add(string.Format(" كشف حساب {0} \r\n عن الفترة من {1} إلى {2}", ((DataRowView)LB.SelectedItem)["per_name"],
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

        private void Archive_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ChangeStatus_BTN.Content = Status_CB.SelectedIndex == 1 ? "أرشيف" : "تفعيل";
            ChangeStatus_BTN.Tag = Status_CB.SelectedIndex == 0 ? "/haies;component/Images/Activate.ico" : "/haies;component/Images/Archive.ico";

            Fill_Car_Owners_LB();
        }
        private void ChangeStatus_BTN_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var status = Status_CB.SelectedIndex == 0 ? 1 : 0;
                DB db = new DB("persons");
                db.AddColumn("per_status", status);
                db.AddCondition("per_id", ((DataRowView)LB.SelectedItem)["per_id"]);
                Confirm.Check(db.Update());
                Fill_Car_Owners_LB();
            }
            catch
            {


            }
        }

    }
}
