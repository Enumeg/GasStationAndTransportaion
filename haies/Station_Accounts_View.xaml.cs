using System;
using System.Data;
using System.Windows.Controls;
using Source;
using System.Windows;
using System.Drawing;

namespace haies
{
    /// <summary>
    /// Interaction logic for Station_Accounts_View.xaml
    /// </summary>
    public partial class Station_Accounts_View : Page
    {
        DataTable Pumps = new DataTable();
        DataTable Gas_Price = new DataTable();
        int SelectedIndex = 0;
        public Station_Accounts_View()
        {
            InitializeComponent();
            Pumps.Columns.Add("pmr_id"); Pumps.Columns.Add("pmr_date", typeof(DateTime)); Pumps.Columns.Add("gas_name"); Pumps.Columns.Add("pum_number");
            Pumps.Columns.Add("pmr_today", typeof(decimal)); Pumps.Columns.Add("pmr_yesterday", typeof(decimal));
            Pumps.Columns.Add("pmr_amount", typeof(decimal)); Pumps.Columns.Add("pmr_value", typeof(decimal));
            Fill_Pumps();
            Get_Balance();
        }
        private void From_DTP_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            try
            {
                if (this.IsLoaded)
                {
                    Get_Balance();
                    switch (Account_CB.SelectedIndex)
                    {
                        case 0:
                            Fill_Pumps();
                            break;
                        case 1:
                            Fill_Sales();
                            break;
                        case 2:
                            Fill_Purchases();
                            break;
                        case 3:
                            Fill_Outcome();
                            break;
                        case 4:
                            Fill_Payment();
                            break;
                        case 5:
                            Fill_Bank();
                            break;
                    }





                }
            }
            catch
            {

            }
        }
        private void Account_CB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                View_GD.ColumnDefinitions[Account_CB.SelectedIndex].Width = new GridLength(1, GridUnitType.Star);
                View_GD.ColumnDefinitions[SelectedIndex].Width = new GridLength(0);
                SelectedIndex = Account_CB.SelectedIndex;
                switch (Account_CB.SelectedIndex)
                {
                    case 0:
                        Fill_Pumps();
                        break;
                    case 1:
                        Fill_Sales();
                        break;
                    case 2:
                        Fill_Purchases();
                        break;
                    case 3:
                        Fill_Outcome();
                        break;
                    case 4:
                        Fill_Payment();
                        break;
                    case 5:
                        Fill_Bank();
                        break;
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
                DataGrid DG = new DataGrid();
                switch (Account_CB.SelectedIndex)
                {
                    case 0:
                        DG = Pumps_DG;
                        break;
                    case 1:
                        DG = Sales_DG;
                        break;
                    case 2:
                        DG = Purchases_DG;
                        break;
                    case 3:
                        DG = Outcome_DG;
                        break;
                    case 4:
                        DG = Customer_Payment_DG;
                        break;
                    case 5:
                        DG = Bank_DG;
                        break;
                }
                CPrinting.CPrinting print = new CPrinting.CPrinting();
                print.header.Add("بيان ب" + Account_CB.Text + " عن الفترة من " + From_DTP.Value.Value.Date.ToString("yyyy/MM/dd") + " إلى " + To_DTP.Value.Value.Date.ToString("yyyy/MM/dd"));
                App.Get_Printed_Table(print, DG);
                print.fonts[CPrinting.CPrinting.FontElement.ColumnHeader] = new Font("Arial", 14);
                print.print();
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
                print.header.Add(string.Format(" كشف حساب المحطة  عن الفترة من {0} إلى {1}",
                From_DTP.Value.Value.Date.ToString("yyyy/MM/dd"), To_DTP.Value.Value.Date.ToString("yyyy/MM/dd")));
                print.printedDataTable.Add(new DataTable());
                print.FooterTable.Add("الرصــــيد السابــــق :", Last_Bal_TK.Text);
                print.FooterTable.Add("إجمالــي المبيعــــات :", Total_Sales_TK.Text);
                print.FooterTable.Add("المبيعــــات الآجلـــة :", Futures_Sales_TK.Text);
                print.FooterTable.Add("المبيـعــات النقـديـــة :", cash_Sales_TK.Text);
                print.FooterTable.Add("إجمالي المدفوعــات :", Payments_TK.Text);
                print.FooterTable.Add("إجمالي المصروفات :", Total_Outcome_TK.Text);
                print.FooterTable.Add("صافي دخل المحطة :", Total_Income_TK.Text);
                print.FooterTable.Add("إجمالي إيداع البنــك :", Bank_TK.Text);
                print.FooterTable.Add("الرصيــــــــــــــــــد :", Balance_TK.Text);
                print.FooterTable.Add("إجمالي المشتريـــات :", Total_Purchases_TK.Text);

                print.print();
            }
            catch
            {

            }
        }

        #region Pumps
        private decimal Get_Pump_Value(object today_Read, object yesterday_Read)
        {
            decimal value = 0, Today_Read = 0, Yesterday_Read = 0;

            try
            {
                Today_Read = decimal.Parse(today_Read.ToString());
                Yesterday_Read = decimal.Parse(yesterday_Read.ToString());
                value = Today_Read >= Yesterday_Read ? (Today_Read - Yesterday_Read) :
                    (Properties.Settings.Default.Pump_Max - Yesterday_Read + Today_Read);
            }
            catch
            {

            }
            return value;
        }
        private void Fill_Pumps()
        {
            Pumps.Rows.Clear();
            DateTime date = From_DTP.Value.Value.Date;
            decimal value = 0, total_value = 0;
            try
            {
                DB db1 = new DB();
                Gas_Price = db1.SelectTable("select gsp_sellCost from gas_price join gas on gsp_gas_id=gas_id join pumps on gas_id=pum_gas_id group by pum_id order by pum_id");
                DB db = new DB();
                DataSet ds = new DataSet();
                db.AddCondition("pmr_date", From_DTP.Value.Value.Date, false, "=", "Today");
                db.AddCondition("pmr_date", From_DTP.Value.Value.Date.AddDays(-1), false, "=", "Yrday");
                while (date <= To_DTP.Value.Value.Date)
                {
                    db.Conditions[0].Value = date;
                    db.Conditions[1].Value = date.AddDays(-1);

                    ds = db.SelectSet(@"select pmr_id,pmr_date,gas_name,pum_number,pmr_value from pump_read join pumps on pum_id=pmr_pum_id join gas on gas_id=pum_gas_id 
                                        where pmr_date=@Today order by pmr_pum_id; 
                                        select COALESCE(pmr_value,0) pmr_value from pump_read where pmr_date=@Yrday order by pmr_pum_id; ");
                    if (ds.Tables[1].Rows.Count > 0)
                    {
                        for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                        {
                            value = Get_Pump_Value(ds.Tables[0].Rows[i]["pmr_value"], ds.Tables[1].Rows[i]["pmr_value"]);
                            Pumps.Rows.Add(ds.Tables[0].Rows[i]["pmr_id"], ds.Tables[0].Rows[i]["pmr_date"], ds.Tables[0].Rows[i]["gas_name"], ds.Tables[0].Rows[i]["pum_number"],
                               ds.Tables[0].Rows[i]["pmr_value"], ds.Tables[1].Rows[i]["pmr_value"], value, Math.Round(value * decimal.Parse(Gas_Price.Rows[i][0].ToString()), 2));
                            total_value += value * decimal.Parse(Gas_Price.Rows[i][0].ToString());
                        }
                    }
                    else
                    {
                        for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                        {
                            value = Get_Pump_Value(ds.Tables[0].Rows[i]["pmr_value"], 0);
                            Pumps.Rows.Add(ds.Tables[0].Rows[i]["pmr_id"], ds.Tables[0].Rows[i]["pmr_date"], ds.Tables[0].Rows[i]["gas_name"], ds.Tables[0].Rows[i]["pum_number"],
                               ds.Tables[0].Rows[i]["pmr_value"], 0, value, Math.Round(value * decimal.Parse(Gas_Price.Rows[i][0].ToString()), 2));
                            total_value += value * decimal.Parse(Gas_Price.Rows[i][0].ToString());
                        }
                    }
                    date = date.AddDays(1);
                }
                Pumps.Rows.Add();
                Pumps.Rows[Pumps.Rows.Count - 1]["pmr_value"] = Math.Round(total_value, 2);
                Pumps_DG.ItemsSource = Pumps.DefaultView;
            }
            catch
            {

            }
        }

        private void Pumps_EP_Add(object sender, EventArgs e)
        {
            try
            {
                Pump_read o = new Pump_read();
                Station s = new Station(o, Operations.Edit);
                s.ShowDialog();
                Fill_Pumps();
            }
            catch
            {

            }
        }
        private void Pumps_EP_Edit(object sender, EventArgs e)
        {
            try
            {
                if (Pumps_DG.SelectedIndex != -1)
                {
                    Station s = new Station(new Pump_read(((DataRowView)Pumps_DG.SelectedItem)["pmr_id"]), Operations.Edit);
                    s.ShowDialog();
                    Fill_Pumps();
                }
            }
            catch
            {

            }
        }
        private void Pumps_EP_Delete(object sender, EventArgs e)
        {
            try
            {
                if (Pumps_DG.SelectedIndex != -1)
                {

                    if (Message.Show("هل تريد الحذف", System.Windows.MessageBoxButton.YesNoCancel, 5) == System.Windows.MessageBoxResult.Yes)
                    {
                        var row = ((DataRowView)Pumps_DG.SelectedItem);
                        DB d1 = new DB("Pump_sales");
                        d1.AddCondition("pms_pmr_id", row["pmr_id"]);
                        DB d = new DB("Pump_read");
                        d.AddCondition("pmr_id", row["pmr_id"]);
                        if (d1.Execute_Queries(d1, d))
                        {
                            var log = new Log();
                            log.Columns.Add(new Column("التاريخ", row["pmr_date"]));
                            log.Columns.Add(new Column("المحروق", row["gas_name"]));
                            log.Columns.Add(new Column("العداد", row["pum_number"]));
                            log.Columns.Add(new Column("القراءه", row["pmr_yesterday"]));
                            log.CreateLog("قراءه طلمبات");
                            Fill_Pumps();
                        }
                    }
                }
            }
            catch
            {

            }
        }

        #endregion

        #region Sales
        private void Fill_Sales()
        {
            DB db = new DB();
            try
            {
                db.AddCondition("sin_date", From_DTP.Value.Value.Date, false, ">=", "SD");
                db.AddCondition("sin_date", To_DTP.Value.Value.Date, false, "<=", "ED");

                DataSet ds = db.SelectSet(@"select si.*,g.gas_name,p.per_name custo from station_income si join gas g on gas_id=sin_gas_id                                            
                                            join customer cu on cust_id=sin_cust_id 
                                            join persons p on per_id = cust_per_id
                                            where sin_date>=@SD and sin_date<=@ED order by sin_date;
                                            select COALESCE(sum(sin_cost),0) from station_income where sin_date>=@SD and sin_date<=@ED;");
                ds.Tables[0].Rows.Add();
                ds.Tables[0].Rows[ds.Tables[0].Rows.Count - 1]["sin_cost"] = ds.Tables[1].Rows[0][0];
                Sales_DG.ItemsSource = ds.Tables[0].DefaultView;
            }
            catch
            {

            }
        }
        private void Sales_EP_Add(object sender, EventArgs e)
        {
            try
            {
                Station_Income o = new Station_Income();
                Station s = new Station(o, Operations.Edit);
                s.ShowDialog();
                Fill_Sales();
            }
            catch
            {

            }
        }
        private void Sales_EP_Edit(object sender, EventArgs e)
        {
            try
            {
                if (Sales_DG.SelectedIndex != -1)
                {

                    Station_Income o = new Station_Income(((DataRowView)Sales_DG.SelectedItem)["sin_id"]);
                    Station s = new Station(o, Operations.Edit);
                    s.ShowDialog();
                    Fill_Sales();
                }
            }
            catch
            {

            }
        }
        private void Sales_EP_Delete(object sender, EventArgs e)
        {
            try
            {
                if (Sales_DG.SelectedIndex != -1)
                {
                    if (Message.Show("هل تريد الحذف", System.Windows.MessageBoxButton.YesNoCancel, 5) == System.Windows.MessageBoxResult.Yes)
                    {
                        var row = ((DataRowView)Sales_DG.SelectedItem);
                        DB d = new DB("station_income");
                        d.AddCondition("sin_id", row["sin_id"]);
                        if (d.Delete())
                        {
                            var log = new Log();
                            log.Columns.Add(new Column("التاريخ", row["sin_date"]));
                            log.Columns.Add(new Column("المحروق", row["gas_name"]));
                            log.Columns.Add(new Column("العــمـيــل", row["custo"]));
                            log.Columns.Add(new Column("الكمـيــــه", row["sin_amount"]));
                            log.CreateLog("إيرادات المحطة");

                            Fill_Sales();
                        }
                    }
                }
            }
            catch
            {

            }
        }

        #endregion

        #region Outcome
        private void Fill_Outcome()
        {
            DB db = new DB();
            try
            {

                db.AddCondition("trs_date", From_DTP.Value.Value.Date, false, ">=", "SD");
                db.AddCondition("trs_date", To_DTP.Value.Value.Date, false, "<=", "ED");


                DataSet ds = db.SelectSet(@"select s.* from station_outcome s where sout_date>=@SD and sout_date<=@ED order by sout_date;
                                            select COALESCE(sum(sout_value),0) from station_outcome where sout_date>=@SD and sout_date<=@ED;");
                ds.Tables[0].Rows.Add();
                ds.Tables[0].Rows[ds.Tables[0].Rows.Count - 1]["sout_value"] = ds.Tables[1].Rows[0][0];
                Outcome_DG.ItemsSource = ds.Tables[0].DefaultView;
            }
            catch
            {

            }
        }
        private void Outcome_EP_Add(object sender, EventArgs e)
        {
            try
            {
                Station_Outcome o = new Station_Outcome();
                Station s = new Station(o, Operations.Edit);
                s.ShowDialog();
                Fill_Outcome();
            }
            catch
            {

            }
        }
        private void Outcome_EP_Edit(object sender, EventArgs e)
        {
            try
            {
                if (Outcome_DG.SelectedIndex != -1)
                {
                    Station_Outcome o = new Station_Outcome(((DataRowView)Outcome_DG.SelectedItem)["sout_id"]);
                    Station s = new Station(o, Operations.Edit);
                    s.ShowDialog();
                    Fill_Outcome();
                }
            }
            catch
            {

            }
        }
        private void Outcome_EP_Delete(object sender, EventArgs e)
        {
            try
            {
                if (Outcome_DG.SelectedIndex != -1)
                {
                    if (Message.Show("هل تريد الحذف", System.Windows.MessageBoxButton.YesNoCancel, 5) == System.Windows.MessageBoxResult.Yes)
                    {
                        var row = ((DataRowView)Outcome_DG.SelectedItem);
                        DB d = new DB("station_outcome");
                        d.AddCondition("sout_id", row["sout_id"]);
                        if (d.Delete())
                        {
                            var log = new Log();
                            log.Columns.Add(new Column("التاريخ", row["sout_date"]));
                            log.Columns.Add(new Column("النوع", row["sout_type"]));
                            log.Columns.Add(new Column("الوصف", row["sout_description"]));
                            log.Columns.Add(new Column("القيمة", row["sout_value"]));
                            log.CreateLog("مصروفات المحطة");

                            Fill_Outcome();
                        }
                    }
                }
            }
            catch
            {

            }
        }

        #endregion

        #region Purchases
        private void Fill_Purchases()
        {
            DB db = new DB();
            try
            {
                db.AddCondition("pur_date", From_DTP.Value.Value.Date, false, ">=", "SD");
                db.AddCondition("pur_date", To_DTP.Value.Value.Date, false, "<=", "ED");

                DataSet ds = db.SelectSet(@"select pur.*,g.gas_name from purchases pur join gas g on pur_gas_id=gas_id                
                                            where pur_date>=@SD and pur_date<=@ED order by pur_date;
                                            select COALESCE(sum(pur_totalCost),0) from purchases where pur_date>=@SD and pur_date<=@ED;");
                ds.Tables[0].Rows.Add();
                ds.Tables[0].Rows[ds.Tables[0].Rows.Count - 1]["pur_totalCost"] = ds.Tables[1].Rows[0][0];
                Purchases_DG.ItemsSource = ds.Tables[0].DefaultView;
            }
            catch
            {

            }
        }
        private void Purchases_EP_Add(object sender, EventArgs e)
        {
            try
            {
                Station_Purchases o = new Station_Purchases();
                Station s = new Station(o, Operations.Edit);
                s.ShowDialog();
                Fill_Purchases();
            }
            catch
            {

            }
        }
        private void Purchases_EP_Edit(object sender, EventArgs e)
        {
            try
            {
                if (Purchases_DG.SelectedIndex != -1)
                {
                    Station_Purchases o = new Station_Purchases(((DataRowView)Purchases_DG.SelectedItem)["pur_id"]);
                    Station s = new Station(o, Operations.Edit);
                    s.ShowDialog();
                    Fill_Purchases();
                }
            }
            catch
            {

            }
        }
        private void Purchases_EP_Delete(object sender, EventArgs e)
        {
            try
            {
                if (Purchases_DG.SelectedIndex != -1)
                {
                    if (Message.Show("هل تريد الحذف", System.Windows.MessageBoxButton.YesNoCancel, 5) == System.Windows.MessageBoxResult.Yes)
                    {
                        var row = ((DataRowView)Purchases_DG.SelectedItem);
                        DB d = new DB("purchases");
                        d.AddCondition("pur_id", row["pur_id"]);
                        if (d.Delete())
                        {
                            var log = new Log();
                            log.Columns.Add(new Column("التاريخ", row["pur_date"]));
                            log.Columns.Add(new Column("المحروق", row["gas_name"]));
                            log.Columns.Add(new Column("الكمية", row["pur_amount"]));
                            log.CreateLog("مشتريات المحطة");
                            Fill_Purchases();

                        }
                    }
                }
            }
            catch
            {

            }
        }

        #endregion

        #region Customers_Payments
        private void Fill_Payment()
        {
            try
            {
                DB db = new DB("customer_loans");
                db.AddCondition("cstl_date", From_DTP.Value.Value.Date, false, ">=", "SD");
                db.AddCondition("cstl_date", To_DTP.Value.Value.Date, false, "<=", "ED");

                DataSet ds = db.SelectSet(@"select cl.*,p.per_name name from customer_loans cl join customer c on c.cust_id=cl.cstl_cust_id and cust_type=1                                                   
                                                    join persons p on p.per_id=c.cust_per_id where cstl_date>=@SD and cstl_date<=@ED order by cstl_date; 
                                                    select COALESCE(sum(cstl_value),0) from customer_loans join customer on cust_id=cstl_cust_id and cust_type=1
                                                    where cstl_date>=@SD and cstl_date<=@ED ;");


                ds.Tables[0].Rows.Add();
                ds.Tables[0].Rows[ds.Tables[0].Rows.Count - 1]["cstl_value"] = ds.Tables[1].Rows[0][0];
                Customer_Payment_DG.ItemsSource = ds.Tables[0].DefaultView;

            }
            catch
            {

            }
        }

        private void Receipt_EP_Add(object sender, EventArgs e)
        {
            try
            {
                Customer_Pay o = new Customer_Pay();
                Station s = new Station(o, Operations.Edit);
                s.ShowDialog();
                Fill_Payment();
            }
            catch
            {

            }
        }
        private void Receipt_EP_Edit(object sender, EventArgs e)
        {
            try
            {
                if (Customer_Payment_DG.SelectedIndex != -1)
                {
                    Customer_Pay o = new Customer_Pay(((DataRowView)Customer_Payment_DG.SelectedItem)["cstl_id"]);
                    Station s = new Station(o, Operations.Edit);
                    s.ShowDialog();
                    Fill_Payment();
                }
            }
            catch
            {

            }
        }
        private void Receipt_EP_Delete(object sender, EventArgs e)
        {
            try
            {
                if (Customer_Payment_DG.SelectedIndex != -1)
                {
                    if (Message.Show("هل تريد الحذف", System.Windows.MessageBoxButton.YesNoCancel, 5) == System.Windows.MessageBoxResult.Yes)
                    {
                        var row = ((DataRowView)Customer_Payment_DG.SelectedItem);
                        DB d = new DB("customer_loans");
                        d.AddCondition("cstl_id", row["cstl_id"]);
                        if (d.Delete())
                        {
                            var log = new Log();
                            log.Columns.Add(new Column("التاريخ", row["cstl_date"]));
                            log.Columns.Add(new Column("القيمه", row["cstl_value"]));
                            log.Columns.Add(new Column("العــمـيــل", row["name"]));
                            log.Columns.Add(new Column("طريقه الدفع", row["cstl_description"]));
                            log.CreateLog("مدفوعات العملاء");

                            Fill_Payment();
                        }

                    }
                }
            }
            catch
            {

            }
        }

        #endregion

        #region Bank
        private void Fill_Bank()
        {
            DB db = new DB();
            try
            {
                db.AddCondition("bnk_date", From_DTP.Value.Value.Date, false, ">=", "SD");
                db.AddCondition("bnk_date", To_DTP.Value.Value.Date, false, "<=", "ED");

                DataSet ds = db.SelectSet(@"select * from bank where bnk_date>=@SD and bnk_date<=@ED order by bnk_date;
                                            select COALESCE(sum(bnk_value),0) from bank where bnk_date>=@SD and bnk_date<=@ED;");
                ds.Tables[0].Rows.Add();
                ds.Tables[0].Rows[ds.Tables[0].Rows.Count - 1]["bnk_value"] = ds.Tables[1].Rows[0][0];
                Bank_DG.ItemsSource = ds.Tables[0].DefaultView;
            }
            catch
            {

            }
        }
        private void Bank_EP_Add(object sender, EventArgs e)
        {
            try
            {
                Bank o = new Bank();
                Station s = new Station(o, Operations.Edit);
                s.ShowDialog();
                Fill_Bank();
            }
            catch
            {

            }
        }
        private void Bank_EP_Edit(object sender, EventArgs e)
        {
            try
            {
                if (Bank_DG.SelectedIndex != -1)
                {
                    Bank o = new Bank(((DataRowView)Bank_DG.SelectedItem)["bnk_id"]);
                    Station s = new Station(o, Operations.Edit);
                    s.ShowDialog();
                    Fill_Bank();
                }
            }
            catch
            {

            }
        }
        private void Bank_EP_Delete(object sender, EventArgs e)
        {
            try
            {
                if (Bank_DG.SelectedIndex != -1)
                {
                    if (Message.Show("هل تريد الحذف", System.Windows.MessageBoxButton.YesNoCancel, 5) == System.Windows.MessageBoxResult.Yes)
                    {
                        var row = ((DataRowView)Bank_DG.SelectedItem);
                        DB d = new DB("Bank");
                        d.AddCondition("bnk_id", row["bnk_id"]);
                        if (d.Delete())
                        {
                            var log = new Log();
                            log.Columns.Add(new Column("التاريـخ", row["bnk_date"]));
                            log.Columns.Add(new Column("القيـمــة", row["bnk_value"]));
                            log.Columns.Add(new Column("البيـــان", row["bnk_description"]));
                            log.CreateLog("البنك - المحطة");
                            Fill_Bank();
                        }

                    }
                }
            }
            catch
            {

            }
        }

        #endregion

        #region Balance
        private void Get_Balance()
        {
            decimal Total_Sales = 0, Last_Balance = 0;

            try
            {

                DB db = new DB();
                db.AddCondition("sin_date", From_DTP.Value.Value.Date, false, ">=", "SD");
                db.AddCondition("sin_date", To_DTP.Value.Value.Date, false, "<=", "ED");
                DataSet ds = db.SelectSet(@"select COALESCE(sum(sin_cost),0) from station_income where sin_date>=@SD and sin_date<=@ED ;
                                            select COALESCE(sum(sout_value),0) from station_outcome where sout_date>=@SD and sout_date<=@ED;
                                            select COALESCE(sum(pur_totalCost),0) from purchases where pur_date>=@SD and pur_date<=@ED;
                                            select COALESCE(sum(bnk_value),0) from bank where bnk_date>=@SD and bnk_date<=@ED;
                                            select COALESCE(sum(cstl_value),0) from customer_loans join customer on cust_id=cstl_cust_id 
                                            and cust_type=1 where cstl_date>=@SD and cstl_date<=@ED;
                                            select COALESCE(sum(pms_amount*gsp_sellCost),0) from pump_sales ps join pumps p on ps.pms_pum_id=p.pum_id
                                            join gas g on g.gas_id=p.pum_gas_id join gas_price on gsp_gas_id = gas_id where pms_date>=@SD and pms_date<=@ED;");

                Last_Balance = Get_Last_Balance();
                Total_Sales = decimal.Parse(ds.Tables[5].Rows[0][0].ToString());
                Last_Bal_TK.Text = Last_Balance.ToString("0.00");
                Total_Sales_TK.Text = Total_Sales.ToString("0.00");
                Futures_Sales_TK.Text = decimal.Parse(ds.Tables[0].Rows[0][0].ToString()).ToString("0.00");
                cash_Sales_TK.Text = (Total_Sales - decimal.Parse(ds.Tables[0].Rows[0][0].ToString())).ToString("0.00");
                Total_Outcome_TK.Text = decimal.Parse(ds.Tables[1].Rows[0][0].ToString()).ToString("0.00");
                Total_Purchases_TK.Text = decimal.Parse(ds.Tables[2].Rows[0][0].ToString()).ToString("0.00");
                Bank_TK.Text = decimal.Parse(ds.Tables[3].Rows[0][0].ToString()).ToString("0.00");
                Payments_TK.Text = decimal.Parse(ds.Tables[4].Rows[0][0].ToString()).ToString("0.00");
                Total_Income_TK.Text = (Total_Sales - decimal.Parse(ds.Tables[0].Rows[0][0].ToString())
                                                     - decimal.Parse(ds.Tables[1].Rows[0][0].ToString())
                                                     + decimal.Parse(ds.Tables[4].Rows[0][0].ToString())).ToString("0.00");
                Balance_TK.Text = (Total_Sales + Last_Balance - decimal.Parse(ds.Tables[0].Rows[0][0].ToString())
                                                               - decimal.Parse(ds.Tables[1].Rows[0][0].ToString())
                                                               - decimal.Parse(ds.Tables[3].Rows[0][0].ToString())
                                                               + decimal.Parse(ds.Tables[4].Rows[0][0].ToString())).ToString("0.00");
            }
            catch
            {

            }
        }
        private decimal Get_Last_Balance()
        {
            decimal Balance = 0;
            try
            {
                DB db = new DB();
                db.AddCondition("pmr_date", From_DTP.Value.Value.Date, false, "<", "Today");
                decimal.TryParse(db.Select(@"select COALESCE(sum(pms_amount*gsp_sellCost),0) 
                                         + (select COALESCE(sum(cstl_value),0) from customer_loans join customer on cust_id=cstl_cust_id and cust_type=1 where cstl_date<@Today)
                                         -((select COALESCE(sum(sin_cost),0) from station_income where sin_date<@Today)                                            
                                         + (select COALESCE(sum(sout_value),0) from station_outcome where sout_date<@Today)                                            
                                         + (select COALESCE(sum(bnk_value),0) from bank where bnk_date<@Today))                                           
                                         from pump_sales ps join pumps p on ps.pms_pum_id=p.pum_id
                                         join gas g on g.gas_id=p.pum_gas_id join gas_price on gsp_gas_id = gas_id where pms_date<@Today;").ToString(), out Balance);
            }
            catch
            {

            }
            return Balance;
        }

        #endregion


    }
}
