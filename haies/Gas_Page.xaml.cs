using System;
using System.Windows;
using System.Windows.Controls;
using Source;
using System.Data;
using System.Collections.Generic;
using System.Drawing;

namespace haies
{
    /// <summary>
    /// Interaction logic for Cement.xaml
    /// </summary>
    public partial class Gas_Page : Page
    {
        object GasId = null;

        public Gas_Page()
        {
            InitializeComponent();
            fill_gas_listbox();

            Get_Cement_Prices();
        }
       
        private void fill_gas_listbox()
        {

            DB db2 = new DB("gas");

            // search by name
            db2.AddCondition("gas_name", "%" + Gas_TB.Text + "%", false, " like ");

            db2.Fill(LB, "gas_id", "gas_name", "select * from gas");


        }

        private void Get_Cement_Prices()
        {
            try
            {
                DB db2 = new DB("cement");
                db2.AddCondition("ctp_sup_id", null, false, " is ");
                DataTable DT = db2.SelectTable("select cp.*,cem_name from cement_price cp join cement c on cem_id=ctp_cem_id");
                DT.Columns.Add("unit");
                foreach (DataRow Row in DT.Rows)
                {
                    Row["unit"] = Row["ctp_unit"].ToString() == "0" ? "طن" : "كيس";
                }
                Gas_DG.ItemsSource = DT.DefaultView;
            }
            catch
            {

            }
        }

        private void EP_Add(object sender, EventArgs e)
        {
            try
            {
                GasId = null;
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
                    GasId = LB.SelectedValue;
                    Gas_TB.Text = ((DataRowView)LB.SelectedItem)[1].ToString();
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
                    if (Message.Show("هل تريد حذف هذا المحروق", MessageBoxButton.YesNoCancel, 10) == MessageBoxResult.Yes)
                    {
                        GasId = LB.SelectedValue;
                        DB db = new DB("gas");
                        db.AddCondition("gas_id", GasId);
                        if (db.Delete())
                        {
                            var log = new Log();
                            log.Columns.Add(new Column("الإسم", Gas_TB.Text));
                            log.CreateLog("المحروقات");
                            fill_gas_listbox();

                        }
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

                DB DataBase = new DB("gas");

                DataBase.AddColumn("gas_name", Gas_TB.Text);

                if (this.GasId == null)
                {
                    if (DataBase.IsNotExist("gas_id", "gas_name"))
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
                    DataBase.AddCondition("gas_id", this.GasId);
                    return Confirm.Check(DataBase.Update());
                }
            }
            catch
            {
                //MessageBox.Show("kiki_method");
                return false;
            }
        }

        private void From_DTP_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            try
            {
                if (this.IsLoaded)
                {
                    Get_Gas_Balance();
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
                Get_Gas_Balance();

            }
            catch
            {

            }

        }

        private void add_update_outcome_Save(object sender, EventArgs e)
        {
            try
            {

                if (Notify.validate("من فضلك ادخل اسم المحروق", Gas_TB.Text, Station.GetWindow(this)))
                {
                    return;
                }



                if (Add_Update())
                {
                    var log = new Log();
                    log.Columns.Add(new Column("الإسم", Gas_TB.Text));
                    log.CreateLog("المحروقات", this.GasId == null);
                    GasId = null;
                    fill_gas_listbox();
                    Main_GD.RowDefinitions[1].Height = new GridLength(0);

                    // yesafar
                    Gas_TB.Text = "";
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

        private void Gas_TB_TextChanged(object sender, TextChangedEventArgs e)
        {
            fill_gas_listbox();
        }

        private void Get_Gas_Balance()
        {
            DateTime Date = new DateTime();
            decimal Current_Balance =0;
            object purchases, sales;
            DataTable dt = new DataTable();
            Dictionary<DateTime, decimal> dic_pur = new Dictionary<DateTime, decimal>();
            Dictionary<DateTime, decimal> dic_lose = new Dictionary<DateTime, decimal>();
            dt.Columns.Add("date", typeof(DateTime));
            dt.Columns.Add("raseed", typeof(decimal));
            dt.Columns.Add("purchases", typeof(decimal));
            dt.Columns.Add("sales", typeof(decimal));
            dt.Columns.Add("loose", typeof(decimal));
            dt.Columns.Add("raseed_now", typeof(decimal));

            try
            {                                                                            
                DB sell = new DB("pump_sales");               
                sell.AddCondition("pms_date", From_DTP.Value.Value.Date, false, "<", "SD");
                sell.AddCondition("gas_id", LB.SelectedValue);
                decimal.TryParse(sell.Select(@"select COALESCE(sum(pur_amount),0) -
                                      ((select COALESCE(sum(ps.pms_amount),0) from pump_sales ps join pumps p on ps.pms_pum_id=p.pum_id
                                       join gas g on g.gas_id=p.pum_gas_id where pms_date<@SD and pum_gas_id=@gas_id)+
                                      (select COALESCE(sum(gls_amount),0) from gas_lose where gls_date<@SD and gls_gas_id=@gas_id))
                                       from purchases where pur_date<@SD and pur_gas_id=@gas_id;").ToString(),out Current_Balance);
               
                DB pur = new DB();
                pur.AddCondition("pur_date", From_DTP.Value.Value.Date, false, ">=", "SD");
                pur.AddCondition("pur_date", To_DTP.Value.Value.Date, false, "<=", "ED");
                pur.AddCondition("gas_id", LB.SelectedValue);
                DataSet ds1 = pur.SelectSet(@"select pur_date,sum(pur_amount) pur_amount from purchases where pur_date>=@SD and pur_date<=@ED and pur_gas_id=@gas_id group by pur_date;
                                              select gls_date,sum(gls_amount) gls_amount from gas_lose where gls_date>=@SD and gls_date<=@ED and gls_gas_id=@gas_id group by gls_date;
                                              select ps.pms_date, sum(ps.pms_amount) gas_amount from pump_sales ps join pumps p on ps.pms_pum_id=p.pum_id
                                              join gas g on g.gas_id=p.pum_gas_id where pms_date>=@SD and pms_date<=@ED and pum_gas_id=@gas_id group by pms_date;");

                foreach (DataRow row in ds1.Tables[0].Rows)
                {
                    dic_pur.Add(DateTime.Parse(row["pur_date"].ToString()), decimal.Parse(row["pur_amount"].ToString()));
                }
                foreach (DataRow row2 in ds1.Tables[1].Rows)
                {
                    dic_lose.Add(DateTime.Parse(row2["gls_date"].ToString()), decimal.Parse(row2["gls_amount"].ToString()));
                }


                foreach (DataRow r in ds1.Tables[2].Rows)
                {
                    Date = DateTime.Parse(r["pms_date"].ToString());
                    purchases = !dic_pur.ContainsKey(Date) ? 0 : dic_pur[Date];
                    sales = !dic_lose.ContainsKey(Date) ? 0 : dic_lose[Date];

                    dt.Rows.Add();

                    dt.Rows[dt.Rows.Count - 1]["date"] = Date;
                    dt.Rows[dt.Rows.Count - 1]["raseed"] = Current_Balance;
                    dt.Rows[dt.Rows.Count - 1]["purchases"] = purchases;
                    dt.Rows[dt.Rows.Count - 1]["sales"] = r["gas_amount"];
                    dt.Rows[dt.Rows.Count - 1]["loose"] = sales;


                    Current_Balance += decimal.Parse(purchases.ToString()) - decimal.Parse(sales.ToString()) - decimal.Parse(r["gas_amount"].ToString());

                    dt.Rows[dt.Rows.Count - 1]["raseed_now"] = Current_Balance;


                }
                Gas_DG.ItemsSource = dt.DefaultView;
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
                print.header.Add("بيان بأرصدة " + ((DataRowView)LB.SelectedItem)["gas_name"] + " عن الفترة من " + From_DTP.Value.Value.Date.ToString("yyyy/MM/dd") + " إلى " + To_DTP.Value.Value.Date.ToString("yyyy/MM/dd"));
                App.Get_Printed_Table(print, Gas_DG);
                print.fonts[CPrinting.CPrinting.FontElement.ColumnHeader] = new Font("Arial", 14);
                print.print();
            }
            catch
            {

            }
        }

        private void Gas_DG_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                Gas_Lose g = new Gas_Lose();
                g.ShowDialog();
            }
            catch
            {

            }
        }

    }
}
