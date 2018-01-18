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
    public partial class Transportations : Page
    {
        public Transportations()
        {
            InitializeComponent();
            Customer.Get_All_Customers(Customer_CB, Customer_type.مصنع, "الكل");
            Drivers.Get_All_Drivers(Driver_CB, true, "الكل");
            Cement.Get_All_Cement(Cement_CB, "الكل");
            Cement_CB.SelectedIndex = Customer_CB.SelectedIndex = Driver_CB.SelectedIndex = 0;
            Fill_Receipt();
        }

        private void Fill_Receipt()
        {
            DB db = new DB();
            decimal[] Totals = new decimal[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            try
            {
                db.AddCondition("trs_date", From_DTP.Value.Value.Date, false, ">=", "SD");
                db.AddCondition("trs_date", To_DTP.Value.Value.Date, false, "<=", "ED");
                db.AddCondition("rec_number", Receipt_Number_TB.Text, string.IsNullOrEmpty(Receipt_Number_TB.Text), " like ");
                db.AddCondition("trs_dri_id", Driver_CB.SelectedValue, Driver_CB.SelectedIndex < 1);
                db.AddCondition("trs_cust_id", Customer_CB.SelectedValue, Customer_CB.SelectedIndex < 1);
                db.AddCondition("rec_cem_id", Cement_CB.SelectedValue, Cement_CB.SelectedIndex < 1);

                DataTable dt = db.SelectTable(@"select r.*,t.*,c.cem_name,unit_name, pl.pl_name,p.per_name custo,p2.per_name driver,trs_is_editable,
                                             trs_card_number,trs_payment_method,trs_load_type,                                                             
                                             Format(COALESCE(rec_sell_price*rec_amount,0),2) rec_totalPrice,
                                             Format(COALESCE(rec_buy_price*rec_amount,0),2) cem_cost,
                                             Format(COALESCE(rec_sell_price*rec_amount,0)- COALESCE(rec_buy_price*rec_amount,0)-trs_discount,2) cem_net,
                                             Format(COALESCE(rec_amount,1)*trs_buy_price,2) trs_cost,
                                             Format((trs_sell_price*COALESCE(rec_amount,1)),2) trs_total_price, 
                                             Format((trs_sell_price*COALESCE(rec_amount,1))-COALESCE(COALESCE(rec_amount,1)*trs_buy_price,0),2) trs_net from transportation t                                                                                                                                    
                                             join drivers d on d.dri_id=trs_dri_id
                                             join cars cs on trs_car_id=car_id 
                                             join persons p2 on p2.per_id=d.dri_per_id                                         
                                             left join customer cu on cust_id = trs_cust_id 
                                             left join persons p on p.per_id = cust_per_id                                            
                                             left join places pl on pl.pl_id=t.trs_pl_id     
                                             left join receipt r  on t.trs_rec_id=r.rec_id         
                                             left join cement c on cem_id=rec_cem_id 
                                             left join units on rec_unit_id = unit_id                                             										    
                                             where trs_date>=@SD and trs_date<=@ED order by trs_date ,rec_number; ");

                dt.Columns.Add("tot_price"); dt.Columns.Add("tot_cost"); dt.Columns.Add("tot_net"); dt.Columns.Add("Min");

                foreach (DataRow row in dt.Rows)
                {
                    row["trs_total_price"] = row["trs_total_price"].ToString() == "" ? 0 : row["trs_total_price"];
                    row["trs_net"] = row["trs_net"].ToString() == "" ? 0 : row["trs_net"];
                    row["tot_price"] = Math.Round(decimal.Parse(row["rec_totalPrice"].ToString()) + decimal.Parse(row["trs_total_price"].ToString())- decimal.Parse(row["trs_discount"].ToString()), 2);
                    row["tot_cost"] = Math.Round(decimal.Parse(row["cem_cost"].ToString()) + decimal.Parse(row["trs_cost"].ToString()), 2);
                    row["tot_net"] = Math.Round(decimal.Parse(row["cem_net"].ToString()) + decimal.Parse(row["trs_net"].ToString()) , 2);
                    Totals[0] += decimal.Parse(row["rec_totalPrice"].ToString());
                    Totals[1] += decimal.Parse(row["cem_cost"].ToString());
                    Totals[2] += decimal.Parse(row["cem_net"].ToString());
                    Totals[3] += decimal.Parse(row["trs_total_price"].ToString());
                    Totals[4] += decimal.Parse(row["trs_cost"].ToString());
                    Totals[5] += decimal.Parse(row["trs_net"].ToString());
                    Totals[6] += decimal.Parse(row["tot_price"].ToString());
                    Totals[7] += decimal.Parse(row["tot_cost"].ToString());
                    Totals[8] += decimal.Parse(row["tot_net"].ToString());
                    Totals[9] += decimal.Parse(row["trs_paid"].ToString());
                    Totals[10] += decimal.Parse(row["trs_discount"].ToString());
                    row["Min"] = decimal.Parse(row["trs_net"].ToString()) < 0;
                }
                dt.Rows.Add();

                dt.Rows[dt.Rows.Count - 1]["rec_totalPrice"] = Totals[0];
                dt.Rows[dt.Rows.Count - 1]["cem_cost"] = Totals[1];
                dt.Rows[dt.Rows.Count - 1]["cem_net"] = Totals[2];
                dt.Rows[dt.Rows.Count - 1]["trs_total_price"] = Totals[3];
                dt.Rows[dt.Rows.Count - 1]["trs_cost"] = Totals[4];
                dt.Rows[dt.Rows.Count - 1]["trs_net"] = Totals[5];
                dt.Rows[dt.Rows.Count - 1]["tot_price"] = Totals[6];
                dt.Rows[dt.Rows.Count - 1]["tot_cost"] = Totals[7];
                dt.Rows[dt.Rows.Count - 1]["tot_net"] = Totals[8];
                dt.Rows[dt.Rows.Count - 1]["trs_paid"] = Totals[9];
                dt.Rows[dt.Rows.Count - 1]["trs_discount"] = Totals[10];
                Receipt_DG.ItemsSource = dt.DefaultView;
            }
            catch
            {

            }
        }

        private void Receipt_EP_Add(object sender, EventArgs e)
        {
            try
            {
                Cement_Office c = new Cement_Office(new Transprtation());
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
                        if (dr["rec_number"].ToString() != "")
                        {
                            Cement_Office c = new Cement_Office(new Transprtation(dr["trs_id"]));
                            c.ShowDialog();
                        }
                        else
                        {
                            Cement_Office c = new Cement_Office(new Add_Transportation(dr["trs_id"]));
                            c.ShowDialog();
                        }
                        Fill_Receipt();
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
                sf.Alignment = StringAlignment.Center;
                sf.LineAlignment = StringAlignment.Center;

                CPrinting.CPrinting print = new CPrinting.CPrinting();
                print.PrintDocument.DefaultPageSettings.Landscape = true;
                print.header.Add("بيان بالمبيعات عن الفترة من " + From_DTP.Value.Value.Date.ToString("yyyy/MM/dd") + " إلى " + To_DTP.Value.Value.Date.ToString("yyyy/MM/dd"));
                App.Get_Printed_Table(print, Receipt_DG);
                print.fonts[CPrinting.CPrinting.FontElement.ColumnHeader] = new Font("Arial", 12);
                print.columnsWidth.Add("unit_name", 50);

                print.columnsDirection.Add("rec_amount", sf);
                print.printedDataTable[0].Columns.Remove("rec_sell_price");
                print.printedDataTable[0].Columns.Remove("rec_totalPrice");
                print.printedDataTable[0].Columns.Remove("cem_cost");
                print.printedDataTable[0].Columns.Remove("trs_discount");
                print.printedDataTable[0].Columns.Remove("cem_net");
                print.printedDataTable[0].Columns.Remove("trs_card_number");
                for (int i = 8; i < 15; i++)
                {
                    print.columnsWidth.Add(print.printedDataTable[0].Columns[i].ColumnName, 60);
                    print.columnsFonts.Add(print.printedDataTable[0].Columns[i].ColumnName, new Font("tahoma", 8));
                    print.columnsDirection.Add(print.printedDataTable[0].Columns[i].ColumnName, sf);
                }

              
                for (int i = 8; i < 11; i++)
                {
                    print.Groups.Add(print.printedDataTable[0].Columns[i].ColumnName, "النقليات");
                }
                for (int i = 11; i < 14; i++)
                {
                    print.Groups.Add(print.printedDataTable[0].Columns[i].ColumnName, "الإجمالي");
                }
               
                print.columnsWidth.Add("trs_date", 70);
                print.columnsWidth.Add("rec_amount", 50);
                print.columnsFonts.Add("trs_date", new Font("tahoma", 8));
                print.columnsFonts.Add("rec_amount", new Font("tahoma", 8));
                print.columnsDirection.Add("trs_date", sf);

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
                //                try
                //                {
                //                    foreach (DataRowView dr in Receipt_DG.Items)
                //                    {
                //                        if (dr["trs_cust_id"].ToString() != "" && decimal.Parse(dr["trs_buy_price"].ToString()) == 0 && dr["trs_rec_id"].ToString() != "")
                //                        {
                //                            DB db2 = new DB();
                //                            db2.AddCondition("car_id", dr["trs_car_id"]);
                //                            db2.AddCondition("ptr_unit_id", dr["rec_unit_id"]);
                //                            db2.AddCondition("ptr_pl_id", dr["trs_pl_id"]);


                //                            DB DB = new DB("transportation");
                //                            DB.AddColumn("trs_buy_price", db2.Select(@"select COALESCE(ptr_value,0) from person_transportation join persons on per_id=ptr_per_id
                //                                             join car_owner on cor_per_id=per_id  join cars on car_cor_id=cor_id "));
                //                            DB.AddCondition("trs_id", dr["trs_id"]);
                //                            DB.Update();
                //                        }
                //                    }
                //                }
                //                catch
                //                {

                //                }
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

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            try
            {
                Future_Sales f = new Future_Sales(Future.All, Customer_type.مصنع);
                f.ShowDialog();
            }
            catch
            {

            }
        }

    }
}
