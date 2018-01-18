using System;
using System.Windows;
using System.Windows.Controls;
using Source;
using System.Data;
using System.Drawing;

namespace haies
{
    /// <summary>
    /// Interaction logic for Factory_Page.xaml
    /// </summary>
    public partial class Factory_Page : Page
    {
        DataTable factory;

        public Factory_Page()
        {
            InitializeComponent();
            Suppliers.Get_All_Suppliers(Suppliers_CB);
            Suppliers_CB.SelectedIndex = 0;
            Get_Factory();
            Get_Least();
            factory = new DataTable();
            factory.Columns.Add("Date", typeof(DateTime)); factory.Columns.Add("M_Number"); factory.Columns.Add("M_Type");
            factory.Columns.Add("M_Debtor", typeof(decimal)); factory.Columns.Add("M_Creditor", typeof(decimal)); factory.Columns.Add("C_Number");
            factory.Columns.Add("cement"); factory.Columns.Add("amount"); factory.Columns.Add("unit"); factory.Columns.Add("account");
            factory.Columns.Add("C_Creditor", typeof(decimal)); factory.Columns.Add("C_Debtor", typeof(decimal));
            factory.Columns[0].Caption = "تاريخ الحركة";
            factory.Columns[1].Caption = "رقم الحركة";
            factory.Columns[2].Caption = "فئة الحركة";
            factory.Columns[3].Caption = "مدين";
            factory.Columns[4].Caption = "دائن";
            factory.Columns[5].Caption = "أمر تحميل العميل";
            factory.Columns[6].Caption = "النوع";
            factory.Columns[7].Caption = "الكمية";
            factory.Columns[8].Caption = "الوحدة";
            factory.Columns[9].Caption = "حساب البنك";
            factory.Columns[10].Caption = "دائن";
            factory.Columns[11].Caption = "مدين";
        }

        public decimal Get_Balance(DateTime DateTime)
        {
            decimal Balance = 0;
            try
            {
                var fac = Suppliers_CB.SelectedValue.ToString() == "1" ? " and rec_cem_id < 5" : "";
                DB db = new DB();
                db.AddCondition("fac_date", DateTime.Date, false, "<", "Date");
                db.AddCondition("sup_id", Suppliers_CB.SelectedValue);
                decimal.TryParse(db.Select(@"select COALESCE(sum(fac_value),0) -(select COALESCE(sum(rec_buy_price*rec_amount),0)
                                             from receipt join transportation on trs_rec_id=rec_id where trs_date<@Date and rec_sup_id=@sup_id" + fac + @" ) 
                                             from factory_Installment where fac_date<@Date and fac_sup_id=@sup_id").ToString(), out Balance);


            }
            catch
            {

            }
            return Balance;
        }

        private void Get_Factory()
        {
            try
            {
                var fac = Suppliers_CB.SelectedValue.ToString() == "1" ? " and rec_cem_id < 5" : "";
                decimal[] totals = new decimal[] { 0, 0 };
                decimal Balance = Get_Balance(From_DTP.Value.Value.Date);
                DataTable dt = new DataTable();
                dt.Columns.Add("fac_in"); dt.Columns.Add("fac_out"); dt.Columns.Add("fac_Balance");
                dt.Columns.Add("fac_description"); dt.Columns.Add("Rec_number"); dt.Columns.Add("fac_date", typeof(DateTime)); dt.Columns.Add("fac_id"); dt.Columns.Add("Min");
                DB db = new DB();
                db.AddCondition("fac_date", From_DTP.Value.Value.Date, false, ">=", "SD");
                db.AddCondition("fac_date", To_DTP.Value.Value.Date, false, "<=", "ED");
                db.AddCondition("sup_id", Suppliers_CB.SelectedValue);
                DataSet ds = db.SelectSet(@"select * from factory_Installment where fac_date>=@SD and fac_date<=@ED  and fac_sup_id=@sup_id order by fac_date;
                                            select Round(rec_buy_price*rec_amount ,2) rec_value,rec_number,cem_name,rec_amount,rec_unit_id,trs_date,unit_name from receipt                                             
                                            join cement on rec_cem_id=cem_id 
                                            join transportation on trs_rec_id=rec_id 
                                            join units on rec_unit_id = unit_id
                                            where trs_date>=@SD and trs_date<=@ED  and rec_sup_id=@sup_id " + fac + " order by rec_number");
                dt.Rows.Add(0, 0, Balance, null, null, From_DTP.Value.Value.Date);

                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    dt.Rows.Add(row["fac_value"], null, 0, row["fac_description"], null, row["fac_date"], row["fac_id"]);
                    totals[0] += decimal.Parse(row["fac_value"].ToString());
                }
                foreach (DataRow row in ds.Tables[1].Rows)
                {
                    dt.Rows.Add(null, row["rec_value"], 0, string.Format("( {0} {1} {2} )", row["rec_amount"].ToString(),
                        row["unit_name"].ToString(), row["cem_name"])
                       , row["rec_number"], row["trs_date"]);
                    totals[1] += decimal.Parse(row["rec_value"].ToString());
                }
                dt.DefaultView.Sort = "fac_date";
                foreach (DataRowView row in dt.DefaultView)
                {
                    Balance = row["fac_in"].ToString() == "" ? Balance - decimal.Parse(row["fac_out"].ToString()) : Balance + decimal.Parse(row["fac_in"].ToString());
                    if (Balance < 0)
                    {
                        row["Min"] = true;
                    }
                    row["fac_Balance"] = Math.Round(Balance, 2);
                }
                dt.Rows.Add(totals[0], totals[1], null, "الإجمالي", null, To_DTP.Value.Value.Date.AddDays(1));
                Factory_DG.ItemsSource = dt.DefaultView;

            }
            catch
            {

            }
        }

        public decimal Get_Least()
        {
            decimal least = 0;
            try
            {
                DB db = new DB("limits");
                db.SelectedColumns.Add("lim_value");
                db.AddCondition("lim_cust_id", Suppliers_CB.SelectedValue);
                Least_TB.Text = db.Select().ToString();
                decimal.TryParse(Least_TB.Text, out least);
            }
            catch
            {

            }
            return least;
        }

        private void EP_Add(object sender, EventArgs e)
        {
            try
            {
                Factory_Installments F = new Factory_Installments(Suppliers_CB.SelectedValue);
                F.ShowDialog();
                Get_Factory();
            }
            catch
            {

            }
        }

        private void EP_Edit(object sender, EventArgs e)
        {
            try
            {
                Factory_Installments F = new Factory_Installments(Suppliers_CB.SelectedValue, ((DataRowView)Factory_DG.SelectedItem)["fac_id"]);
                F.ShowDialog();
                Get_Factory();
            }
            catch
            {

            }
        }

        private void EP_Delete(object sender, EventArgs e)
        {
            try
            {
                if (Message.Show("هل تريد حذف هذا الرصيد", MessageBoxButton.YesNoCancel, 5) == MessageBoxResult.Yes)
                {
                    var row = ((DataRowView)Factory_DG.SelectedItem);
                    DB db = new DB("factory_Installment");
                    db.AddCondition("fac_id", row["fac_id"]);
                    if (db.Delete())
                    {
                        var log = new Log();
                        log.Columns.Add(new Column("التاريـخ", row["fac_date"]));
                        log.Columns.Add(new Column("القيـمــة", row["fac_in"]));
                        log.Columns.Add(new Column("البيـــان", row["fac_description"]));
                        log.CreateLog("أقساط المصنع");
                    
                    Get_Factory();
                }}
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
                    Get_Factory();
            }
            catch
            {

            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Save_Panel.Visibility = System.Windows.Visibility.Visible;
                Edit_BTN.Visibility = System.Windows.Visibility.Collapsed;
                Least_TB.Style = FindResource("Edit_TextBox") as Style;
                Get_Least();
            }
            catch
            {

            }
        }

        private void SavePanel_Save(object sender, EventArgs e)
        {
            try
            {
                if (Update_Least())
                {
                    var log = new Log();
                    log.Columns.Add(new Column("الرصيد", Least_TB.Text));
                    log.CreateLog("الحد الأدنى لرصيد المصنع", false);

                    Save_Panel.Visibility = System.Windows.Visibility.Collapsed;
                    Edit_BTN.Visibility = System.Windows.Visibility.Visible;
                    Least_TB.Style = FindResource("View_TextBox") as Style;
                    Get_Least();
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
                Save_Panel.Visibility = System.Windows.Visibility.Collapsed;
                Edit_BTN.Visibility = System.Windows.Visibility.Visible;
                Least_TB.Style = FindResource("View_TextBox") as Style;
                Get_Least();
            }
            catch
            {

            }
        }

        private bool Update_Least()
        {
            try
            {
                DB db = new DB("limits");
                db.AddColumn("lim_value", Least_TB.Text.Trim());
                db.AddCondition("lim_cust_id", Suppliers_CB.SelectedValue);
                return Confirm.Check(db.Update());
            }
            catch
            {
                return false;
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!App.Cement_IsLoaded)
                {
                    if (Message.Show("هل تريد إضافة رصيد", MessageBoxButton.YesNo, 6) == MessageBoxResult.Yes)
                    {
                        Factory_Installments F = new Factory_Installments(Suppliers_CB.SelectedValue);
                        F.ShowDialog();
                        Get_Factory();

                    }
                    App.Cement_IsLoaded = true;
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
                factory.Rows.Clear();
                foreach (DataRowView row in Factory_DG.ItemsSource)
                {
                    if (((DataView)Factory_DG.ItemsSource).Table.Rows.IndexOf(row.Row) == 0) { continue; }
                    if (row["fac_Balance"].ToString() != "")
                    {
                        if (row["fac_id"].ToString() != "")
                        {
                            factory.Rows.Add(row["fac_date"], null, "RECEIPT", 0, row["fac_in"], null, null, null, null, row["fac_description"]);
                        }
                        else
                        {
                            string[] vales = row["fac_description"].ToString().Split(' ');
                            factory.Rows.Add(row["fac_date"], null, "INVOICE", row["fac_out"], 0, row["Rec_number"], vales[3] + " " + vales[4], vales[1], vales[2]);
                        }
                        if (decimal.Parse(row["fac_Balance"].ToString()) < 0)
                        {
                            factory.Rows[factory.Rows.Count - 1]["C_Creditor"] = row["fac_Balance"].ToString().Replace("-", "");
                        }
                        else
                        {
                            factory.Rows[factory.Rows.Count - 1]["C_Debtor"] = row["fac_Balance"];
                        }
                    }
                    else
                    {
                        factory.Rows.Add(row["fac_date"], null, "Total", row["fac_out"], 0, row["Rec_number"]);
                    }
                }
                CPrinting.CPrinting print = new CPrinting.CPrinting();
                print.printedDataTable.Add(factory);
                print.columnFormat.Add("Date", "{0:yyyy/MM/dd}");
                print.Groups.Add("C_Debtor", "الرصيد");
                print.Groups.Add("C_Creditor", "الرصيد");
                print.PrintDocument.DefaultPageSettings.Landscape = true;
                print.PrintDocument.DefaultPageSettings.Margins = new System.Drawing.Printing.Margins(40, 40, 100, 100);
                print.print();
            }
            catch
            {

            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.IsLoaded)
                Get_Factory();
        }
    }
}
