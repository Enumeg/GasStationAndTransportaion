using System.Data;
using System.Windows;
using System.Windows.Controls;
using Source;
using System.Windows.Data;
using System;
using System.Drawing;

namespace haies
{
    /// <summary>
    /// Interaction logic for Receipts_Office.xaml
    /// </summary>

    public partial class Receipts_Office : Page
    {
        public Receipts_Office()
        {
            InitializeComponent();
            Customer.Get_All_Customers(Customer_Search_CB, Customer_type.مصنع, "الكل");
            Drivers.Get_All_Drivers(Driver_Search_CB, false, "الكل");
            Cement.Get_All_Cement(Cement_Search_CB, "الكل");
            Cement_Search_CB.SelectedIndex = Customer_Search_CB.SelectedIndex = Driver_Search_CB.SelectedIndex = 0;
            Fill_Receipt();
        }

        private void Fill_Receipt()
        {
            DB db = new DB();
            decimal[] Totals = new decimal[] { 0, 0, 0, 0, 0 };
            try
            {
                db.AddCondition("trs_date", From_DTP.Value.Value.Date, false, ">=", "SD");
                db.AddCondition("trs_date", To_DTP.Value.Value.Date, false, "<=", "ED");
                db.AddCondition("rec_number", Receipt_Number_TB.Text, string.IsNullOrEmpty(Receipt_Number_TB.Text), " like ");
                db.AddCondition("trs_dri_id", Driver_Search_CB.SelectedValue, Driver_Search_CB.SelectedIndex < 1);
                db.AddCondition("trs_cust_id", Customer_Search_CB.SelectedValue, Customer_Search_CB.SelectedIndex < 1);
                db.AddCondition("rec_cem_id", Cement_Search_CB.SelectedValue, Cement_Search_CB.SelectedIndex < 1);

                DataTable dt = db.SelectTable(@"select rec_id,rec_number,trs_date,rec_amount,trs_paid,trs_is_editable,trs_discount,c.cem_name,unit_name,pl.pl_name,
                                                trs_id,trs_card_number,trs_payment_method,trs_load_type,
                                                p.per_name customer,p2.per_name driver,cs.car_number, COALESCE(rec_sell_price,0)+COALESCE(trs_sell_price,0) unit_price,
                                                format(COALESCE((rec_sell_price*rec_amount),0)+(COALESCE(trs_sell_price,0)*COALESCE(rec_amount,1)),2) total_price,                                     
                                                format(COALESCE((rec_sell_price*rec_amount),0)+(COALESCE(trs_sell_price,0)*COALESCE(rec_amount,1)) - trs_discount - trs_paid,2) trs_rest from transportation t                                               
                                                join drivers d on d.dri_id=trs_dri_id
                                                join cars cs on trs_car_id=car_id 
                                                join persons p2 on p2.per_id=d.dri_per_id                                          
                                                left join customer cu on cust_id = trs_cust_id 
                                                left join persons p on p.per_id = cust_per_id                                            
                                                left join places pl on pl.pl_id=t.trs_pl_id 
                                                left join receipt r on t.trs_rec_id=r.rec_id     
                                                left join cement c on cem_id=rec_cem_id
                                                left join units on rec_unit_id = unit_id                                               										                                            
                                                where trs_date>=@SD and trs_date<=@ED order by rec_number;");


                foreach (DataRow row in dt.Rows)
                {
                    Totals[0] += decimal.Parse(row["unit_price"].ToString());
                    Totals[1] += decimal.Parse(row["total_price"].ToString());
                    Totals[2] += decimal.Parse(row["trs_paid"].ToString());
                    Totals[3] += decimal.Parse(row["trs_rest"].ToString());
                    Totals[4] += decimal.Parse(row["trs_discount"].ToString());
                }
                dt.Rows.Add();
                dt.Rows[dt.Rows.Count - 1]["unit_price"] = Totals[0];
                dt.Rows[dt.Rows.Count - 1]["total_price"] = Totals[1];
                dt.Rows[dt.Rows.Count - 1]["trs_paid"] = Totals[2];
                dt.Rows[dt.Rows.Count - 1]["trs_rest"] = Totals[3];
                dt.Rows[dt.Rows.Count - 1]["trs_discount"] = Totals[4];
                Receipt_DG.ItemsSource = dt.DefaultView;
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
                    Fill_Receipt();
            }
            catch
            {

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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StringFormat sf = new StringFormat();
                sf.Alignment = StringAlignment.Far;
                sf.LineAlignment = StringAlignment.Center;

                CPrinting.CPrinting print = new CPrinting.CPrinting();
                print.PrintDocument.DefaultPageSettings.Landscape = true;
                print.header.Add("بيان بالمبيعات عن الفترة من " + From_DTP.Value.Value.Date.ToString("yyyy/MM/dd") + " إلى " + To_DTP.Value.Value.Date.ToString("yyyy/MM/dd"));
                App.Get_Printed_Table(print, Receipt_DG);
                print.fonts[CPrinting.CPrinting.FontElement.ColumnHeader] = new Font("Arial", 14);
                print.columnsWidth.Add("unit_name", 50);
                print.columnsWidth.Add("rec_amount", 50);
                print.columnsFonts.Add("rec_amount", new Font("tahoma", 10));
                print.columnsDirection.Add("rec_amount", sf);
                for (int i = 9; i < 14; i++)
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
                        else if (dr["rec_id"].ToString() == "")
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
                                if (d.Delete())
                                {
                                    var log = new Log();
                                    log.Columns.Add(new Column("رقم الفسح", ((DataRowView)Receipt_DG.SelectedItem)["rec_number"]));
                                    log.CreateLog("الفسوحات");
                                }
                            }
                            else
                            {
                                DB d = new DB("transportation");
                                d.AddCondition("trs_id", dr["rec_id"]);
                                if (d.Delete())
                                {
                                    var log = new Log();
                                    log.Columns.Add(new Column("رقم الفسح", ((DataRowView)Receipt_DG.SelectedItem)["rec_number"]));
                                    log.CreateLog("الفسوحات");
                                }

                            }
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

    }
}
