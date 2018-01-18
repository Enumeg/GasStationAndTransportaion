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
    /// Interaction logic for accounting.xaml
    /// </summary>
    public partial class Receipts : Page
    {
        public Receipts()
        {
            InitializeComponent();
            Drivers.Get_All_Drivers(Driver_CB, false, "الكل");
            Cement.Get_All_Cement(Cement_CB, "الكل");
            Cement_CB.SelectedIndex = Driver_CB.SelectedIndex = 0;
            Fill_Receipt();
        }

        private void Fill_Receipt()
        {
            DB db = new DB();
            string s = "";
            decimal[] Totals = new decimal[] { 0, 0, 0, 0, 0, 0, 0, 0 };
            try
            {
                db.AddCondition("trs_date", From_DTP.Value.Value.Date, false, ">=", "SD");
                db.AddCondition("trs_date", To_DTP.Value.Value.Date, false, "<=", "ED");
                db.AddCondition("rec_number", Receipt_Number_TB.Text, false, " like ");
                db.AddCondition("trs_dri_id", Driver_CB.SelectedValue, Driver_CB.SelectedIndex < 1);
                db.AddCondition("rec_cem_id", Cement_CB.SelectedValue, Cement_CB.SelectedIndex < 1);
                if (State_CB.SelectedIndex == 1)
                {
                    s = "and trs_paid + trs_discount = (rec_sell_price*rec_amount) ";
                }
                else if (State_CB.SelectedIndex == 2)
                {
                    s = "and trs_paid + trs_discount <> (rec_sell_price*rec_amount) ";
                }
                DataTable dt = db.SelectTable(@"select rec_id,rec_number,trs_date,rec_amount,trs_paid,trs_discount,trs_is_editable,c.cem_name,unit_name,per_name,car_number,trs_id,
                                                trs_payment_method,
                                                format((rec_sell_price*rec_amount),2) rec_sell_price,
                                                format((rec_buy_price*rec_amount),2) rec_buy_price,
                                                format(((rec_sell_price-rec_buy_price)*rec_amount)- trs_discount,2) rec_net,                                              
                                                format((rec_sell_price*rec_amount)- trs_discount - trs_paid ,2)trs_rest from receipt r                                                                                                                                                                                  
                                                join cement c on cem_id=rec_cem_id 
                                                join units on rec_unit_id = unit_id
                                                join transportation t on t.trs_rec_id=r.rec_id   
                                                join drivers d on d.dri_id=trs_dri_id
                                                join cars cs on trs_car_id=car_id 
                                                join persons p2 on p2.per_id=d.dri_per_id          	                                                							                                          
                                                where trs_date>=@SD and trs_date<=@ED " + s + " order by trs_date,rec_number;");
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
                    Totals[0] += decimal.Parse(row["rec_sell_price"].ToString());
                    Totals[1] += decimal.Parse(row["rec_buy_price"].ToString());
                    Totals[2] += decimal.Parse(row["rec_net"].ToString());
                    Totals[4] += decimal.Parse(row["trs_paid"].ToString());
                    Totals[5] += decimal.Parse(row["trs_rest"].ToString());
                    Totals[6] += decimal.Parse(row["trs_discount"].ToString());
                    Totals[7] += decimal.Parse(row["rest"].ToString());
                }
                dt.Rows.Add();
                dt.Rows[dt.Rows.Count - 1]["rec_sell_price"] = Totals[0];
                dt.Rows[dt.Rows.Count - 1]["rec_buy_price"] = Totals[1];
                dt.Rows[dt.Rows.Count - 1]["rec_net"] = Totals[2];
                dt.Rows[dt.Rows.Count - 1]["trs_paid"] = Totals[4];
                dt.Rows[dt.Rows.Count - 1]["trs_rest"] = Totals[5];
                dt.Rows[dt.Rows.Count - 1]["trs_discount"] = Totals[6];
                dt.Rows[dt.Rows.Count - 1]["rest"] = Totals[7];
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

        private void Receipt_EP_Add(object sender, EventArgs e)
        {
            try
            {
                Cement_Office c = new Cement_Office(new Receipt());
                c.ShowDialog();
                Fill_Receipt();
            }
            catch
            {

            }
        }

        private void Receipt_EP_Edit(object sender, EventArgs e)
        {
            try
            {
                if (Receipt_DG.SelectedIndex != -1)
                {
                    DataRowView dr = (DataRowView)Receipt_DG.SelectedItem;
                    if (int.Parse(dr["trs_is_editable"].ToString()) == 1)
                    {
                        Cement_Office c = new Cement_Office(new Receipt(dr["rec_id"]));
                        c.ShowDialog();
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

        private void Receipt_EP_Delete(object sender, EventArgs e)
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
                            DB d = new DB("receipt");
                            d.AddCondition("rec_id", ((DataRowView)Receipt_DG.SelectedItem)["rec_id"]);
                            if (d.Delete())
                            {
                                var log = new Log();
                                log.Columns.Add(new Column("رقم الفسح", ((DataRowView)Receipt_DG.SelectedItem)["rec_number"]));
                                log.CreateLog("الفسوحات");
                                Fill_Receipt();
                            }
                        }


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

        private void From_DTP_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            try
            {
                if (this.IsLoaded)
                {
                    Fill_Receipt();
                }
            }
            catch
            {

            }
        }

        private void Receipt_Number_TB_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                Fill_Receipt();
            }
            catch
            {

            }
        }

        private void Customer_CB_SelectionChanged(object sender, SelectionChangedEventArgs e)
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StringFormat sf = new StringFormat();
                sf.Alignment = StringAlignment.Far;
                sf.LineAlignment = StringAlignment.Center;

                CPrinting.CPrinting print = new CPrinting.CPrinting();
                print.PrintDocument.DefaultPageSettings.Landscape = true;
                print.header.Add("بيان بالفسوحات عن الفترة من " + From_DTP.Value.Value.Date.ToString("yyyy/MM/dd") + " إلى " + To_DTP.Value.Value.Date.ToString("yyyy/MM/dd"));
                App.Get_Printed_Table(print, Receipt_DG);
                print.fonts[CPrinting.CPrinting.FontElement.ColumnHeader] = new Font("Arial", 14);
                print.columnsWidth.Add("unit_name", 50);
                print.columnsWidth.Add("rec_amount", 55);
                print.columnsFonts.Add("rec_amount", new Font("tahoma", 10));
                print.columnsDirection.Add("rec_amount", sf);
                for (int i = 7; i < 14; i++)
                {
                    print.columnsWidth.Add(print.printedDataTable[0].Columns[i].ColumnName, 70);
                    print.columnsFonts.Add(print.printedDataTable[0].Columns[i].ColumnName, new Font("tahoma", 10));
                    print.columnsDirection.Add(print.printedDataTable[0].Columns[i].ColumnName, sf);
                }
                print.print();
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
                    if (d.Update())
                    {
                        var log = new Log();
                        log.Columns.Add(new Column("رقم الفسح", ((DataRowView)Receipt_DG.SelectedItem)["rec_number"]));
                        log.CreateLog("الفسوحات", false);
                    }

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

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                try
                {
                    CheckBox cb = sender as CheckBox;
                    DB d = new DB("transportation");
                    d.AddCondition("trs_id", cb.Tag);
                    d.AddColumn("trs_is_editable", cb.IsChecked);
                    d.Update();
                    Fill_Receipt();
                }
                catch
                {

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
                MainWindow M = (MainWindow)haies.MainWindow.GetWindow(this);
                M.frame.Navigate(new Clients());
            }
            catch
            {

            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            try
            {
                Totals t = new Totals();
                t.ShowDialog();
            }
            catch
            {

            }
        }

    }
}
