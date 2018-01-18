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
using System.Drawing;

namespace haies
{
    /// <summary>
    /// Interaction logic for Office_Account.xaml
    /// </summary>
    public partial class Office_Account : Page
    {
        int SelectedIndex = 0;
        public Office_Account()
        {
            InitializeComponent();
            Customer.Get_All_Customers(Customer_Search_CB, Customer_type.مصنع, "الكل");
            Drivers.Get_All_Drivers(Driver_Search_CB, false, "الكل");
            Cement.Get_All_Cement(Cement_Search_CB, "الكل");
            Cement_Search_CB.SelectedIndex = Customer_Search_CB.SelectedIndex = Driver_Search_CB.SelectedIndex = 0;
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
                            Fill_Receipt();
                            break;
                        case 1:
                            Fill_Payment();
                            break;
                        case 2:
                            Fill_Outcome();
                            break;
                        case 3:
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
                        Fill_Receipt();
                        break;
                    case 1:
                        Fill_Payment();
                        break;
                    case 2:
                        Fill_Outcome();
                        break;
                    case 3:
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
                if (Account_CB.SelectedIndex == 0)
                {

                    StringFormat sf = new StringFormat();
                    sf.Alignment = StringAlignment.Far;
                    sf.LineAlignment = StringAlignment.Center;

                    CPrinting.CPrinting print = new CPrinting.CPrinting();
                    print.PrintDocument.DefaultPageSettings.Landscape = true;
                    print.header.Add("بيان بالمبيعات عن الفترة من " + From_DTP.Value.Value.Date.ToString("yyyy/MM/dd") + " إلى " + To_DTP.Value.Value.Date.ToString("yyyy/MM/dd"));
                    App.Get_Printed_Table(print, Receipt_DG);
                    print.fonts[CPrinting.CPrinting.FontElement.ColumnHeader] = new Font("Arial", 14);
                    print.columnsWidth.Add("unit_name", 40);
                    print.columnsWidth.Add("rec_amount", 60);
                    print.columnsFonts.Add("rec_amount", new Font("tahoma", 9));
                    print.columnsDirection.Add("rec_amount", sf);                   
                    for (int i = 9; i < 15; i++)
                    {
                        print.columnsWidth.Add(print.printedDataTable[0].Columns[i].ColumnName, 65);
                        print.columnsFonts.Add(print.printedDataTable[0].Columns[i].ColumnName, new Font("tahoma", 9));
                        print.columnsDirection.Add(print.printedDataTable[0].Columns[i].ColumnName, sf);
                    }
                    print.columnsWidth.Add(print.printedDataTable[0].Columns[0].ColumnName, 80);
                    print.print();

                }
                else
                {
                    switch (Account_CB.SelectedIndex)
                    {
                        case 1:
                            DG = Customer_Payment_DG;
                            break;
                        case 2:
                            DG = Outcome_DG;
                            break;
                        case 3:
                            DG = Bank_DG;
                            break;
                    }
                    CPrinting.CPrinting print = new CPrinting.CPrinting();
                    print.header.Add("بيان ب" + Account_CB.Text + " عن الفترة من " + From_DTP.Value.Value.Date.ToString("yyyy/MM/dd") + " إلى " + To_DTP.Value.Value.Date.ToString("yyyy/MM/dd"));
                    App.Get_Printed_Table(print, DG);
                    print.fonts[CPrinting.CPrinting.FontElement.ColumnHeader] = new Font("Arial", 14);
                    print.print();
                }
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
                print.header.Add(string.Format(" كشف حساب مكتب الأسمنت عن الفترة من {0} إلى {1}",
                From_DTP.Value.Value.Date.ToString("yyyy/MM/dd"), To_DTP.Value.Value.Date.ToString("yyyy/MM/dd")));
                print.printedDataTable.Add(new DataTable());
                print.FooterTable.Add("الرصــــيد السابــــق :", Last_Bal_TK.Text);
                print.FooterTable.Add("إجمالــي المبيعــــات :", Total_Sales_TK.Text);
                print.FooterTable.Add("إجمالــي المدفوعــات :", Payments_TK.Text);
                print.FooterTable.Add("المبيعــــات الآجلـــة :", Futures_Sales_TK.Text);
                print.FooterTable.Add("المبيـعــات النقـديـــة :", cash_Sales_TK.Text);
                print.FooterTable.Add("إجمالي المصروفات :", Total_Outcome_TK.Text);
                print.FooterTable.Add("صافي الدخل :", Total_Income_TK.Text);
                print.FooterTable.Add("إجمالي إيداع البنــك :", Bank_TK.Text);
                print.FooterTable.Add("الرصيــــــــــــــــــد :", Balance_TK.Text);

                print.print();
            }
            catch
            {

            }
        }

        #region Receipts

        private void Fill_Receipt()
        {
            DB db = new DB();
            decimal[] Totals = new decimal[] { 0, 0, 0, 0, 0, 0 };
            try
            {
                db.AddCondition("trs_date", From_DTP.Value.Value.Date, false, ">=", "SD");
                db.AddCondition("trs_date", To_DTP.Value.Value.Date, false, "<=", "ED");
                db.AddCondition("rec_number", Receipt_Number_TB.Text, string.IsNullOrEmpty(Receipt_Number_TB.Text), " like ");
                db.AddCondition("trs_dri_id", Driver_Search_CB.SelectedValue, Driver_Search_CB.SelectedIndex < 1);
                db.AddCondition("trs_cust_id", Customer_Search_CB.SelectedValue, Customer_Search_CB.SelectedIndex < 1);
                db.AddCondition("rec_cem_id", Cement_Search_CB.SelectedValue, Cement_Search_CB.SelectedIndex < 1);

                DataTable dt = db.SelectTable(@"select rec_id,rec_number,trs_date,rec_amount,trs_paid,trs_is_editable,trs_discount,c.cem_name,unit_name,pl.pl_name,
                                                trs_id,trs_card_number,trs_payment_method,trs_load_type, p.per_name customer,p2.per_name driver,cs.car_number,                                                
                                                COALESCE(rec_sell_price,0)+COALESCE(trs_sell_price,0)-COALESCE(trs_discount/COALESCE(rec_amount,1),0) unit_price,
                                                Round(COALESCE((rec_sell_price*rec_amount),0)+(COALESCE(trs_sell_price,0)*COALESCE(rec_amount,1))- trs_discount,2) total_price,                                     
                                                Round(COALESCE((rec_sell_price*rec_amount),0)+(COALESCE(trs_sell_price,0)*COALESCE(rec_amount,1))- trs_discount - trs_paid,2) trs_rest from transportation t                                               
                                                join drivers d on d.dri_id=trs_dri_id
                                                join cars cs on trs_car_id=car_id 
                                                join persons p2 on p2.per_id=d.dri_per_id                                          
                                                left join customer cu on cust_id = trs_cust_id 
                                                left join persons p on p.per_id = cust_per_id                                            
                                                left join places pl on pl.pl_id=t.trs_pl_id 
                                                left join receipt r on t.trs_rec_id=r.rec_id     
                                                left join cement c on cem_id=rec_cem_id
                                                left join units on rec_unit_id = unit_id                                                                                        									                                            
                                                where trs_date>=@SD and trs_date<=@ED order by trs_date,rec_number;");

                dt.Columns.Add("cstl_value"); dt.Columns.Add("cstl_date"); dt.Columns.Add("rest");
                foreach (DataRow row in dt.Rows)
                {
                    if (row["rec_id"].ToString() != "")
                    {
                        var Row = Get_Paid(row["rec_id"]);
                        if (Row != null)
                        {
                            row["cstl_value"] = Row[0];
                            row["cstl_date"] = Row[1];
                            row["rest"] = decimal.Parse(row["trs_rest"].ToString()) - decimal.Parse(row["cstl_value"].ToString());
                            if (decimal.Parse(row["rest"].ToString()) == 0)
                            {
                                row["trs_payment_method"] = "خالص";
                            }
                        }
                    }
                    else
                    {
                        row["rest"] = row["trs_rest"];
                    }
                    Totals[0] += decimal.Parse(row["unit_price"].ToString());
                    Totals[1] += decimal.Parse(row["total_price"].ToString());
                    Totals[2] += decimal.Parse(row["trs_paid"].ToString());
                    Totals[3] += decimal.Parse(row["trs_rest"].ToString());
                    Totals[4] += decimal.Parse(row["trs_discount"].ToString());
                    Totals[5] += decimal.Parse(row["rest"].ToString());
                }
                dt.Rows.Add();
                dt.Rows[dt.Rows.Count - 1]["unit_price"] = Totals[0];
                dt.Rows[dt.Rows.Count - 1]["total_price"] = Totals[1];
                dt.Rows[dt.Rows.Count - 1]["trs_paid"] = Totals[2];
                dt.Rows[dt.Rows.Count - 1]["trs_rest"] = Totals[3];
                dt.Rows[dt.Rows.Count - 1]["trs_discount"] = Totals[4];
                dt.Rows[dt.Rows.Count - 1]["rest"] = Totals[5];
                Receipt_DG.ItemsSource = dt.DefaultView;
            }
            catch
            {

            }
        }

        private DataRow Get_Paid(object rec_id)
        {
            try
            {
                DB db = new DB();
                db.AddCondition("cstl_rec_id", rec_id);
                return db.SelectRow("select COALESCE(sum(cstl_value),0),date_format(max(cstl_date),'%Y/%m/%d') from receipt join client_loans on rec_id=cstl_rec_id");
            }
            catch
            {
                return null;
            }
        }

        private void Customer_Search_CB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (this.IsLoaded)
                    Fill_Receipt();
            }
            catch
            {

            }
        }

        private void Receipt_Number_TB_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (this.IsLoaded)
                    Fill_Receipt();
            }
            catch
            {

            }
        }

        private void Receipt_DG_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            try
            {
                DataRowView dr = Receipt_DG.SelectedItem as DataRowView;

                if (int.Parse(dr["trs_is_editable"].ToString()) == 1)
                {
                    DB d = new DB("Receipt");
                    d.AddCondition("rec_id", dr["rec_id"]);
                    d.AddColumn("rec_amount", ((TextBox)e.Column.GetCellContent(e.Row)).Text);
                    d.Update();
                }
                else
                {
                    Message.Show("عفوا لا يمكن التعديل", MessageBoxButton.OK);
                }
                Fill_Receipt();
            }
            catch
            {

            }
        }

        private void Cement_EP_Add(object sender, EventArgs e)
        {
            try
            {
                if (Message.Show("هل تريد الإلغاء", MessageBoxButton.YesNoCancel) == MessageBoxResult.Yes)
                {
                    DataRowView dr = Receipt_DG.SelectedItem as DataRowView;
                    if (dr["rec_id"].ToString() != "")
                    {
                        DB DataBase = new DB("receipt");
                        DataBase.AddColumn("rec_sell_price", 0);
                        DataBase.AddColumn("rec_buy_price", 0);
                        DataBase.AddColumn("rec_amount", 0);
                        DataBase.AddCondition("rec_id", dr["rec_id"]);
                        DataBase.Update();
                    }
                    DB DB = new DB("transportation");
                    DB.AddColumn("trs_sell_price", 0);
                    DB.AddColumn("trs_buy_price", 0);
                    DB.AddColumn("trs_paid", 0);
                    DB.AddColumn("trs_discount", 0);
                    DB.AddColumn("trs_dri_motive", 0);
                    DB.AddCondition("trs_id", dr["trs_id"]);
                    Confirm.Check(DB.Update());
                    Fill_Receipt();
                }
            }
            catch
            {

            }
        }

        private void EditPanel_Edit(object sender, EventArgs e)
        {
            try
            {

                if (Receipt_DG.SelectedIndex != -1)
                {
                    DataRowView dr = (DataRowView)Receipt_DG.SelectedItem;
                    if (int.Parse(dr["trs_is_editable"].ToString()) == 1)
                    {
                        Cement_Office c = Cement_Office.GetWindow(this) as Cement_Office;
                        if (dr["customer"].ToString() == "")
                        {
                            c.frame.Navigate(new Receipt(((DataRowView)Receipt_DG.SelectedItem)["rec_id"]));
                        }
                        else if (dr["rec_number"].ToString() == "")
                        {
                            c.frame.Navigate(new Add_Transportation(((DataRowView)Receipt_DG.SelectedItem)["trs_id"]));
                        }
                        else
                        {
                            c.frame.Navigate(new Transprtation(((DataRowView)Receipt_DG.SelectedItem)["trs_id"]));
                        }
                        Fill_Receipt();
                    }
                    else
                    {
                        Message.Show("عفوا لا يمكن التعديل", MessageBoxButton.OK);
                    }
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
                if (Receipt_DG.SelectedIndex != -1)
                {
                    DataRowView dr = (DataRowView)Receipt_DG.SelectedItem;
                    if (int.Parse(dr["trs_is_editable"].ToString()) == 1)
                    {
                        if (Message.Show("هل تريد الحذف", MessageBoxButton.YesNoCancel) == MessageBoxResult.Yes)
                        {
                            if (dr["rec_id"].ToString() != "")
                            {
                                DB d = new DB("receipt");
                                d.AddCondition("rec_id", dr["rec_id"]);
                                d.Delete();
                            }
                            else
                            {
                                DB d = new DB("transportation");
                                d.AddCondition("trs_id", dr["rec_id"]);
                                d.Delete();

                            }
                            var log = new Log();
                            log.Columns.Add(new Column("رقم الفسح", dr["rec_id"]));
                            log.CreateLog("الفسوحات");
                        }
                        Fill_Receipt();
                    }
                    else
                    {
                        Message.Show("عفوا لا يمكن الحذف", MessageBoxButton.OK);
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

                db.AddCondition("oto_date", From_DTP.Value.Value.Date, false, ">=", "SD");
                db.AddCondition("oto_date", To_DTP.Value.Value.Date, false, "<=", "ED");


                DataSet ds = db.SelectSet(@"select * from outcome_office  where oto_date>=@SD and oto_date<=@ED order by oto_date;                                            
                                            select COALESCE(sum(oto_value),0) from outcome_office where oto_date>=@SD and oto_date<=@ED;");
                ds.Tables[0].Rows.Add();
                ds.Tables[0].Rows[ds.Tables[0].Rows.Count - 1]["oto_value"] = ds.Tables[1].Rows[0][0];
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
                Outcome_Office s = new Outcome_Office();
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
                    Outcome_Office s = new Outcome_Office(((DataRowView)Outcome_DG.SelectedItem)["oto_id"]);
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

                        DB db = new DB("outcome_office");
                        db.AddCondition("oto_id", row["oto_id"]);
                        if (db.Delete())
                        {
                            var log = new Log();
                            log.Columns.Add(new Column("التاريـخ", row["oto_date"]));
                            log.Columns.Add(new Column("القيـمــة", row["oto_value"]));
                            log.Columns.Add(new Column("البيـــان", row["oto_description"]));
                            log.CreateLog("المصروفات");

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

        #region Customers_Payments
        private void Fill_Payment()
        {
            try
            {
                DB db = new DB("Client_loans");
                db.AddCondition("cstl_date", From_DTP.Value.Value.Date, false, ">=", "SD");
                db.AddCondition("cstl_date", To_DTP.Value.Value.Date, false, "<=", "ED");

                DataSet ds = db.SelectSet(@"select cl.*,p.per_name name,rec_number from Client_loans cl join drivers  on dri_id=cl.cstl_dri_id                                                    
                                                    join persons p on p.per_id=dri_per_id left join receipt on rec_id=cstl_rec_id where cstl_date>=@SD and cstl_date<=@ED order by cstl_date; 
                                                    select COALESCE(sum(cstl_value),0) from Client_loans join drivers on dri_id=cstl_dri_id 
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
                Client_loans s = new Client_loans();
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
                    DataRowView dr = Customer_Payment_DG.SelectedItem as DataRowView;
                    Client_loans s = new Client_loans(dr["cstl_id"]);
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
                        var row =((DataRowView)Customer_Payment_DG.SelectedItem);
                        DB d = new DB("Client_loans");
                        d.AddCondition("cstl_id", row["cstl_id"]);
                        if (d.Delete())
                        {
                            var log = new Log();
                            log.Columns.Add(new Column("السائق", row["name"]));
                            log.Columns.Add(new Column("التاريخ", row["cstl_value"]));
                            log.Columns.Add(new Column("الفسح", row["rec_number"]));
                            log.CreateLog("مدفوعات السائقين");
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
                db.AddCondition("bnko_date", From_DTP.Value.Value.Date, false, ">=", "SD");
                db.AddCondition("bnko_date", To_DTP.Value.Value.Date, false, "<=", "ED");

                DataSet ds = db.SelectSet(@"select * from bank_office where bnko_date>=@SD and bnko_date<=@ED order by bnko_date;
                                            select COALESCE(sum(bnko_value),0) from bank_office where bnko_date>=@SD and bnko_date<=@ED;");
                ds.Tables[0].Rows.Add();
                ds.Tables[0].Rows[ds.Tables[0].Rows.Count - 1]["bnko_value"] = ds.Tables[1].Rows[0][0];
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
                Bank_Office o = new Bank_Office();
                o.ShowDialog();
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
                    Bank_Office o = new Bank_Office(((DataRowView)Bank_DG.SelectedItem)["bnko_id"]);
                    o.ShowDialog();
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
                        var row =  ((DataRowView)Bank_DG.SelectedItem);
                        DB d = new DB("bank_office");
                        d.AddCondition("bnko_id",row["bnko_id"]);
                        if (d.Delete())
                        {
                            var log = new Log();
                            log.Columns.Add(new Column("التاريـخ", row["bnko_date"]));
                            log.Columns.Add(new Column("القيـمــة", row["bnko_value"]));
                            log.Columns.Add(new Column("البيـــان", row["bnko_description"]));
                            log.CreateLog("البنك - المكتب");
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
            decimal Total_Sales = 0, Last_Balance = 0, Total_Discount = 0, Cash_Sales = 0;

            try
            {

                DB db = new DB();
                db.AddCondition("sin_date", From_DTP.Value.Value.Date, false, ">=", "SD");
                db.AddCondition("sin_date", To_DTP.Value.Value.Date, false, "<=", "ED");
                DataSet ds = db.SelectSet(@"select sum(COALESCE((rec_sell_price*rec_amount),0)+(COALESCE(trs_sell_price,0)*COALESCE(rec_amount,1))) total_price,
                                            COALESCE(sum(trs_paid),0) ,COALESCE(sum(trs_discount),0)
                                            from transportation left join receipt on trs_rec_id=rec_id where trs_date>=@SD and trs_date<=@ED;                                            
                                            select COALESCE(sum(oto_value),0) from outcome_office where oto_date>=@SD and oto_date<=@ED;                                          
                                            select COALESCE(sum(bnko_value),0) from bank_office where bnko_date>=@SD and bnko_date<=@ED;
                                            select COALESCE(sum(cstl_value),0) from Client_loans join drivers on dri_id=cstl_dri_id 
                                             where cstl_date>=@SD and cstl_date<=@ED; ");

                Last_Balance = Get_Last_Balance();
                Total_Sales = decimal.Parse(ds.Tables[0].Rows[0][0].ToString());
                Cash_Sales = decimal.Parse(ds.Tables[0].Rows[0][1].ToString());
                Total_Discount = decimal.Parse(ds.Tables[0].Rows[0][2].ToString());
                Total_Sales -= Total_Discount;
                Last_Bal_TK.Text = Last_Balance.ToString("0.00");
                Total_Sales_TK.Text = Total_Sales.ToString("0.00");
                cash_Sales_TK.Text = Cash_Sales.ToString("0.00");
                //Total_Discount_TK.Text = Total_Discount.ToString("0.00");
                Futures_Sales_TK.Text = (Total_Sales - Cash_Sales).ToString("0.00");
                Total_Outcome_TK.Text = decimal.Parse(ds.Tables[1].Rows[0][0].ToString()).ToString("0.00");
                Bank_TK.Text = decimal.Parse(ds.Tables[2].Rows[0][0].ToString()).ToString("0.00");
                Payments_TK.Text = decimal.Parse(ds.Tables[3].Rows[0][0].ToString()).ToString("0.00");
                Total_Income_TK.Text = (Cash_Sales + decimal.Parse(ds.Tables[3].Rows[0][0].ToString())
                                                   - decimal.Parse(ds.Tables[1].Rows[0][0].ToString())).ToString("0.00");
                                                 
                Balance_TK.Text = (Cash_Sales + Last_Balance + decimal.Parse(ds.Tables[3].Rows[0][0].ToString())
                                                             - decimal.Parse(ds.Tables[1].Rows[0][0].ToString())
                                                             - decimal.Parse(ds.Tables[2].Rows[0][0].ToString())
                                                               ).ToString("0.00");
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
                decimal.TryParse(db.Select(@"select sum(trs_paid)  
                                         + (select COALESCE(sum(cstl_value),0) from Client_loans join drivers on dri_id=cstl_dri_id  where cstl_date<@Today)
                                         - (select COALESCE(sum(oto_value),0) from outcome_office where oto_date<@Today)                                                                                                                       
                                         - (select COALESCE(sum(bnko_value),0) from bank_office where bnko_date<@Today)                                          
                                            from transportation where trs_date<@Today;").ToString(), out Balance);
            }
            catch
            {

            }
            return Balance;
        }

        #endregion


    }
}
