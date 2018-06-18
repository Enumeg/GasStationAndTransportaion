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
        private StationAccounts _stationAccounts;
        int SelectedIndex = 0;
        public Station_Accounts_View()
        {
            InitializeComponent();
            _stationAccounts = new StationAccounts();

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
                print.FooterTable.Add("ضريبة المبيعــــات :", Sales_Tax_TK.Text);
                print.FooterTable.Add("صافي المبيعــــات :", Net_Sales_TK.Text);
                print.FooterTable.Add("المبيعــــات الآجلـــة :", Futures_Sales_TK.Text);
                print.FooterTable.Add("المبيـعــات النقـديـــة :", cash_Sales_TK.Text);
                print.FooterTable.Add("إجمالي المدفوعــات :", Payments_TK.Text);
                print.FooterTable.Add("إجمالي المصروفات :", Total_Outcome_TK.Text);
                print.FooterTable.Add("صافي دخل المحطة :", Total_Income_TK.Text);
                print.FooterTable.Add("إجمالي إيداع البنــك :", Bank_TK.Text);
                print.FooterTable.Add("الرصيــــــــــــــــــد :", Balance_TK.Text);
                print.FooterTable.Add("إجمالي المشتريـــات :", Total_Purchases_TK.Text);
                print.FooterTable.Add("ضريبة المشتريـــات :", Purchases_Tax_TK.Text);
                print.FooterTable.Add("صافي المشتريـــات :", Net_Purchases_TK.Text);

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
            Pumps_DG.ItemsSource = _stationAccounts.ListPumps(From_DTP.Value.Value.Date, To_DTP.Value.Value.Date);
        }

        private void Pumps_EP_Add(object sender, EventArgs e)
        {
            try
            {
                PumpRead o = new PumpRead();
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
                    Station s = new Station(new PumpRead(((DataRowView)Pumps_DG.SelectedItem)["pmr_id"]), Operations.Edit);
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
            Sales_DG.ItemsSource = _stationAccounts.ListSales(From_DTP.Value.Value.Date, To_DTP.Value.Value.Date);
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
            Outcome_DG.ItemsSource = _stationAccounts.ListOutcome(From_DTP.Value.Value.Date, To_DTP.Value.Value.Date);
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
            Purchases_DG.ItemsSource = _stationAccounts.ListPurchases(From_DTP.Value.Value.Date, To_DTP.Value.Value.Date);
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
            Customer_Payment_DG.ItemsSource = _stationAccounts.ListPayments(From_DTP.Value.Value.Date, To_DTP.Value.Value.Date);          
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
            Bank_DG.ItemsSource = _stationAccounts.ListBank(From_DTP.Value.Value.Date, To_DTP.Value.Value.Date);
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

                var model = _stationAccounts.GetAccounts(From_DTP.Value.Value.Date, To_DTP.Value.Value.Date);
                Last_Bal_TK.Text = model.LastBalance.ToString("0.00");
                Total_Sales_TK.Text = model.Sales.ToString("0.00");
                Sales_Tax_TK.Text = model.SalesTax.ToString("0.00");
                Net_Sales_TK.Text = model.NetSales.ToString("0.00");
                Futures_Sales_TK.Text = model.FutureSales.ToString("0.00");
                cash_Sales_TK.Text = model.CashSales.ToString("0.00");
                Total_Outcome_TK.Text = model.Outcome.ToString("0.00");
                Total_Purchases_TK.Text = model.Purchases.ToString("0.00");
                Purchases_Tax_TK.Text = model.PurchasesTax.ToString("0.00");
                Net_Purchases_TK.Text = model.NetPurchases.ToString("0.00");
                Bank_TK.Text = model.Bank.ToString("0.00");
                Payments_TK.Text = model.Payment.ToString("0.00");
                Total_Income_TK.Text = model.Income.ToString("0.00");
                Balance_TK.Text = model.Balance.ToString("0.00");
                TotalSales_TK.Text = model.Sales.ToString("0.00");
                SalesTax_TK.Text = model.SalesTax.ToString("0.00");
                NetSales_TK.Text = model.NetSales.ToString("0.00");
                TotalPurchases_TK.Text = model.Purchases.ToString("0.00");
                PurchasesTax_TK.Text = model.PurchasesTax.ToString("0.00");
                NetPurchases_TK.Text = model.NetPurchases.ToString("0.00");

            }
            catch
            {

            }
        }

        #endregion


    }
}
