using System;
using System.Windows;
using System.Windows.Controls;
using Source;
using System.Data;
using System.Drawing.Printing;
using System.Drawing;

namespace haies
{
    /// <summary>
    /// Interaction logic for Station_Accounts.xaml
    /// </summary>
    public partial class Station_Accounts : Page
    {
        private readonly CPrinting.PrintedDocument _pd;

        private readonly DataTable _pumps = new DataTable();

        private DataTable _gasPrice = new DataTable();

        private DataTable _outcomeSales;

        int _rowIndex = 0, page_num = 0;
        public Station_Accounts()
        {
            InitializeComponent();
            _pumps.Columns.Add("pmr_id"); _pumps.Columns.Add("gas_name"); _pumps.Columns.Add("pum_number");
            _pumps.Columns.Add("pmr_today", typeof(decimal)); _pumps.Columns.Add("pmr_yesterday", typeof(decimal));
            _pumps.Columns.Add("pmr_amount", typeof(decimal)); _pumps.Columns.Add("pmr_value", typeof(decimal));
            Get_Balance();
            Get_Gas_Balance();
            Fill_Pumps();
            Fill_Sales();
            Fill_Outcome();
            _pd = new CPrinting.PrintedDocument();
            _pd.PrintPage += new PrintPageEventHandler(PD_PrintPage);
        }

        private void Get_Balance()
        {
            decimal Total_Sales = 0, Last_Balance = 0;
            try
            {
                DB db = new DB();
                db.AddCondition("pmr_date", Date_TB.Value.Value.Date, false, "=", "Today");
                DataSet ds = db.SelectSet(@"select COALESCE(sum(sin_cost),0) from station_income where sin_date=@Today;
                                            select COALESCE(sum(sout_value),0) from station_outcome where sout_date=@Today;
                                            select COALESCE(sum(pur_totalCost),0) from purchases where pur_date=@Today;
                                            select COALESCE(sum(bnk_value),0) from bank where bnk_date=@Today;
                                            select COALESCE(sum(cstl_value),0) from customer_loans join customer on cust_id=cstl_cust_id and cust_type=1 where cstl_date=@Today;
                                            select COALESCE(sum(pms_amount*gsp_sellCost),0) from pump_sales ps join pumps p on ps.pms_pum_id=p.pum_id
                                            join gas g on g.gas_id=p.pum_gas_id join gas_price on gsp_gas_id = gas_id where pms_date=@Today;");
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
                db.AddCondition("pmr_date", Date_TB.Value.Value.Date, false, "<", "Today");
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

        private void Get_Gas_Balance()
        {
            decimal Current_Balance = 0;
            object purchases, sales;
            DataTable dt = new DataTable();
            //dt.Columns.Add("gas_name");
            //dt.Columns.Add("raseed", typeof(decimal));
            //dt.Columns.Add("purchases", typeof(decimal));
            //dt.Columns.Add("sales", typeof(decimal));
            //dt.Columns.Add("loose", typeof(decimal));
            //dt.Columns.Add("raseed_now", typeof(decimal));
            dt.Columns.Add("type");
            dt.Columns.Add("gas1");
            dt.Columns.Add("gas2");
            dt.Columns.Add("gas3");
            dt.Rows.Add("Balance");
            dt.Rows.Add("Purchases");
            dt.Rows.Add("sales");
            dt.Rows.Add("loose");
            dt.Rows.Add("Balance");
            try
            {
                int i = 1;
                DB db = new DB();
                foreach (DataRow Row in db.SelectTable("select gas_id,gas_name from gas group by gas_name order by gas_id").Rows)
                {
                    DB sell = new DB("pump_sales");
                    sell.AddCondition("pms_date", Date_TB.Value.Value.Date, false, "<", "SD");
                    sell.AddCondition("gas_id", Row[0]);
                    decimal.TryParse(sell.Select(@"select COALESCE(sum(pur_amount),0) -
                                      ((select COALESCE(sum(ps.pms_amount),0) from pump_sales ps join pumps p on ps.pms_pum_id=p.pum_id
                                       join gas g on g.gas_id=p.pum_gas_id where pms_date<@SD and pum_gas_id=@gas_id)+
                                      (select COALESCE(sum(gls_amount),0) from gas_lose where gls_date<@SD and gls_gas_id=@gas_id))
                                       from purchases where pur_date<@SD and pur_gas_id=@gas_id;").ToString(), out Current_Balance);

                    DB pur = new DB();
                    pur.AddCondition("pur_date", Date_TB.Value.Value.Date, false, "=", "SD");
                    pur.AddCondition("gas_id", Row[0]);
                    DataSet ds1 = pur.SelectSet(@"select COALESCE(sum(pur_amount),0) pur_amount from purchases where pur_date=@SD and pur_gas_id=@gas_id;
                                                  select COALESCE(sum(gls_amount),0) gls_amount from gas_lose where gls_date=@SD and gls_gas_id=@gas_id;
                                                  select COALESCE(sum(ps.pms_amount),0) gas_amount from pump_sales ps join pumps p on ps.pms_pum_id=p.pum_id
                                                  join gas g on g.gas_id=p.pum_gas_id where pms_date=@SD and pum_gas_id=@gas_id ;");


                    purchases = ds1.Tables[0].Rows.Count > 0 ? ds1.Tables[0].Rows[0][0] : 0;
                    sales = ds1.Tables[1].Rows.Count > 0 ? ds1.Tables[1].Rows[0][0] : 0;



                    DataRow r = ds1.Tables[2].Rows[0];





                    dt.Rows[0][i] = Current_Balance;
                    dt.Rows[1][i] = purchases;
                    dt.Rows[2][i] = r["gas_amount"];
                    dt.Rows[3][i] = sales;
                    dt.Rows[4][i] = Current_Balance + decimal.Parse(purchases.ToString()) - decimal.Parse(sales.ToString()) - decimal.Parse(r["gas_amount"].ToString());
                    i++;

                }

                Gas_DG.ItemsSource = dt.DefaultView;
            }
            catch
            {

            }
        }

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
            _pumps.Rows.Clear();
            DateTime date = Date_TB.Value.Value.Date;
            decimal value = 0;
            object read = 0;
            string gas = "";
            decimal TGA = 0, TGV = 0;
            try
            {
                DB db1 = new DB();
                _gasPrice = db1.SelectTable("select gsp_sellCost from gas_price join gas on gsp_gas_id=gas_id join pumps on gas_id=pum_gas_id group by pum_id order by  gas_id,pum_id");
                DB db = new DB();
                DataSet ds = new DataSet();
                db.AddCondition("pmr_date", Date_TB.Value.Value.Date, false, "=", "Today");
                db.AddCondition("pmr_date", Date_TB.Value.Value.Date.AddDays(-1), false, "=", "Yrday");


                ds = db.SelectSet(@"select pmr_id,pmr_date,gas_name,pum_number,pmr_value from pump_read join pumps on pum_id=pmr_pum_id join gas on gas_id=pum_gas_id 
                                        where pmr_date=@Today order by gas_id,pmr_pum_id; 
                                        select COALESCE(pmr_value,0) pmr_value from pump_read join pumps on pum_id=pmr_pum_id where pmr_date=@Yrday order by pum_gas_id,pmr_pum_id; ");
                if (ds.Tables[1].Rows.Count > 0)
                {
                    gas = ds.Tables[0].Rows[0]["gas_name"].ToString();
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        if (gas != ds.Tables[0].Rows[i]["gas_name"].ToString())
                        {
                            _pumps.Rows.Add(null, gas, null, null, null, TGA, TGV);
                            TGA = 0; TGV = 0;
                        }
                        read = ds.Tables[1].Rows.Count == i ? 0 : ds.Tables[1].Rows[i]["pmr_value"];
                        value = Get_Pump_Value(ds.Tables[0].Rows[i]["pmr_value"], read);
                        _pumps.Rows.Add(ds.Tables[0].Rows[i]["pmr_id"], ds.Tables[0].Rows[i]["gas_name"], ds.Tables[0].Rows[i]["pum_number"],
                           ds.Tables[0].Rows[i]["pmr_value"], read, value, Math.Round(value * decimal.Parse(_gasPrice.Rows[i][0].ToString()), 2));
                        TGA += value;
                        TGV += Math.Round(value * decimal.Parse(_gasPrice.Rows[i][0].ToString()), 2);
                        gas = ds.Tables[0].Rows[i]["gas_name"].ToString();
                    }
                    _pumps.Rows.Add(null, gas, null, null, null, TGA, TGV);
                }
                else
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        value = Get_Pump_Value(ds.Tables[0].Rows[i]["pmr_value"], 0);
                        _pumps.Rows.Add(ds.Tables[0].Rows[i]["pmr_id"], ds.Tables[0].Rows[i]["gas_name"], ds.Tables[0].Rows[i]["pum_number"],
                           ds.Tables[0].Rows[i]["pmr_value"], 0, value, Math.Round(value * decimal.Parse(_gasPrice.Rows[i][0].ToString()), 2));
                    }
                }
                //DataRow dr1 = Pumps.NewRow(); DataRow dr2 = Pumps.NewRow(); DataRow dr3 = Pumps.NewRow();

                //dr1["gas_name"] = Pumps.Rows[0]["gas_name"];
                //dr1["pmr_amount"] = decimal.Parse(Pumps.Rows[0]["pmr_amount"].ToString()) + decimal.Parse(Pumps.Rows[1]["pmr_amount"].ToString());
                //dr1["pmr_value"] = decimal.Parse(Pumps.Rows[0]["pmr_value"].ToString()) + decimal.Parse(Pumps.Rows[1]["pmr_value"].ToString());

                //dr2["gas_name"] = Pumps.Rows[2]["gas_name"];
                //dr2["pmr_amount"] = decimal.Parse(Pumps.Rows[2]["pmr_amount"].ToString()) + decimal.Parse(Pumps.Rows[3]["pmr_amount"].ToString());
                //dr2["pmr_value"] = decimal.Parse(Pumps.Rows[2]["pmr_value"].ToString()) + decimal.Parse(Pumps.Rows[3]["pmr_value"].ToString());

                //dr3["gas_name"] = Pumps.Rows[4]["gas_name"];
                //dr3["pmr_amount"] = decimal.Parse(Pumps.Rows[4]["pmr_amount"].ToString()) + decimal.Parse(Pumps.Rows[5]["pmr_amount"].ToString());
                //dr3["pmr_value"] = decimal.Parse(Pumps.Rows[4]["pmr_value"].ToString()) + decimal.Parse(Pumps.Rows[5]["pmr_value"].ToString());

                //Pumps.Rows.InsertAt(dr1, 2); Pumps.Rows.InsertAt(dr2, 5); Pumps.Rows.InsertAt(dr3, 8);
                Pumps_DG.ItemsSource = _pumps.DefaultView;
            }
            catch
            {

            }
        }

        private void Fill_Sales()
        {
            DB db = new DB();
            try
            {
                db.AddCondition("sin_date", Date_TB.Value.Value.Date);

                DataSet ds = db.SelectSet(@"select si.*,g.gas_name,p.per_name custo from station_income si join gas g on gas_id=sin_gas_id                                             
                                            join customer cu on cust_id=sin_cust_id join persons p on per_id = cust_per_id where sin_date=@sin_date;
                                            select COALESCE(sum(sin_cost),0) from station_income where sin_date=@sin_date");
                ds.Tables[0].Rows.Add();
                ds.Tables[0].Rows[ds.Tables[0].Rows.Count - 1]["sin_cost"] = ds.Tables[1].Rows[0][0];
                Sales_DG.ItemsSource = ds.Tables[0].DefaultView;
            }
            catch
            {

            }
        }

        private void Fill_Outcome()
        {
            DB db = new DB();
            try
            {

                db.AddCondition("trs_date", Date_TB.Value.Value.Date);


                DataSet ds = db.SelectSet(@"select s.* from station_outcome s where sout_date=@trs_date ;
                                            select COALESCE(sum(sout_value),0) from station_outcome where sout_date=@trs_date ;");
                ds.Tables[0].Rows.Add();
                ds.Tables[0].Rows[ds.Tables[0].Rows.Count - 1]["sout_value"] = ds.Tables[1].Rows[0][0];
                Outcome_DG.ItemsSource = ds.Tables[0].DefaultView;
            }
            catch
            {

            }
        }

        private void add_update_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Window w = Station.GetWindow(this);
                w.Close();
            }
            catch
            {

            }
        }

        private void Date_TB_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            try
            {
                if (this.IsLoaded)
                {
                    Get_Balance();
                    Get_Gas_Balance();
                    Fill_Pumps();
                    Fill_Sales();
                    Fill_Outcome();
                }
            }
            catch
            {

            }
        }

        private void PD_PrintPage(object sender, PrintPageEventArgs e)
        {
            try
            {
                e.HasMorePages = Print(e);
            }
            catch
            {

            }
        }

        private bool Print(PrintPageEventArgs e)
        {
            
            StringFormat Center = new StringFormat();
            Center.LineAlignment = StringAlignment.Center;
            Center.Alignment = StringAlignment.Center;
            StringFormat Right = new StringFormat();
            Right.LineAlignment = StringAlignment.Center;
            Right.Alignment = StringAlignment.Far; ;
            Font Header_Font = new Font("Cambria", 16);
            Font Cell_Font = new Font("Cambria", 14);
            Font Numbers_Font = new Font("Tahoma", 10);
            Brush Foreground = Brushes.Black;
            Pen Border_Pen = new Pen(Foreground, 2);
            Pen Cells_Pen = new Pen(Foreground, 1);
            Pen Spliter_Pen = new Pen(Foreground, 3);
            float Temp_Height = e.MarginBounds.Top, Temp_Width = e.MarginBounds.Left , pumps_height = 0;
            int j = 0;
            int[] Columns_width = new int[] { 30, 30, 110, 110, 72, 72, 78, 75, 75, 75 };
            try
            {
                if (_rowIndex == 0)
                {
                    e.Graphics.DrawString("Station Accounts For " + Date_TB.Value.Value.ToString("dd/MM/yyyy"), Header_Font, Foreground, new RectangleF(e.MarginBounds.Left, Temp_Height, e.MarginBounds.Width, 30), Center);
                    Temp_Height += 30;
                    e.Graphics.DrawString("Counter Reads", Header_Font, Foreground, new RectangleF(e.MarginBounds.Left, Temp_Height, 424, 30), Center);
                    e.Graphics.DrawString("Gas Balance", Header_Font, Foreground, new RectangleF(e.MarginBounds.Left + 424, Temp_Height, e.MarginBounds.Width - 424, 30), Center);
                    e.Graphics.DrawLine(Border_Pen, e.MarginBounds.Left, Temp_Height, e.MarginBounds.Right, Temp_Height);
                    Temp_Height += 30;
                    e.Graphics.DrawLine(Border_Pen, e.MarginBounds.Left, Temp_Height, e.MarginBounds.Right, Temp_Height);
                    for (int i = 1; i < 7; i++)
                    {
                        e.Graphics.DrawString(Pumps_DG.Columns[i].Header.ToString(), Cell_Font, Foreground, new RectangleF(Temp_Width, Temp_Height, Columns_width[i - 1], 45), Center);
                        Temp_Width += Columns_width[i - 1];
                    }
                    for (int i = 0; i < 4; i++)
                    {
                        e.Graphics.DrawString(Gas_DG.Columns[i].Header.ToString(), Cell_Font, Foreground, new RectangleF(Temp_Width, Temp_Height, Columns_width[i + 6], 45), Center);
                        Temp_Width += Columns_width[i + 6];
                    }
                    Temp_Height += 45;
                    e.Graphics.DrawLine(Cells_Pen, e.MarginBounds.Left, Temp_Height, e.MarginBounds.Right, Temp_Height);
                    Temp_Width = e.MarginBounds.Left;

                    foreach (DataRowView row in Pumps_DG.Items)
                    {
                        if (row[2].ToString() == "")
                            e.Graphics.FillRectangle(Brushes.DarkGray, new RectangleF(e.MarginBounds.Left, Temp_Height + 1, 424, 29));


                        for (int i = 1; i < 3; i++)
                        {
                            e.Graphics.DrawString(row[i].ToString(), Cell_Font, Foreground, new RectangleF(Temp_Width, Temp_Height, Columns_width[i - 1], 30), Center);
                            Temp_Width += Columns_width[i - 1];
                        }
                        for (int i = 3; i < 7; i++)
                        {
                            e.Graphics.DrawString(row[i].ToString(), Numbers_Font, Foreground, new RectangleF(Temp_Width, Temp_Height, Columns_width[i - 1] - 3, 30), Right);
                            Temp_Width += Columns_width[i - 1];
                        }
                        if (j < 5)
                        {
                            e.Graphics.DrawString(((DataRowView)Gas_DG.Items[j])[0].ToString(), Numbers_Font, Foreground, new RectangleF(Temp_Width, Temp_Height, Columns_width[6], 30), Center);
                            Temp_Width += Columns_width[6];

                            for (int i = 1; i < 4; i++)
                            {
                                e.Graphics.DrawString(((DataRowView)Gas_DG.Items[j])[i].ToString(), Numbers_Font, Foreground, new RectangleF(Temp_Width, Temp_Height, Columns_width[i + 6] - 3, 30), Right);
                                Temp_Width += Columns_width[i + 6];
                            }
                        }
                        Temp_Height += 30;
                        e.Graphics.DrawLine(Cells_Pen, e.MarginBounds.Left, Temp_Height, e.MarginBounds.Right, Temp_Height);
                        Temp_Width = e.MarginBounds.Left;
                        j++;
                    }
                    pumps_height = Temp_Height+30;
                    foreach (int i in Columns_width)
                    {
                        Temp_Width += i;
                        e.Graphics.DrawLine(Cells_Pen, Temp_Width, e.MarginBounds.Top + 60, Temp_Width, Temp_Height);
                    }
                    e.Graphics.DrawLine(Border_Pen, e.MarginBounds.Left, e.MarginBounds.Top + 30, e.MarginBounds.Left, Temp_Height);
                    e.Graphics.DrawLine(Border_Pen, e.MarginBounds.Right, e.MarginBounds.Top + 30, e.MarginBounds.Right, Temp_Height);
                    e.Graphics.DrawLine(Spliter_Pen, e.MarginBounds.Left + 424, e.MarginBounds.Top + 30, e.MarginBounds.Left + 424, Temp_Height);
                    e.Graphics.DrawLine(Border_Pen, e.MarginBounds.Left, Temp_Height, e.MarginBounds.Right, Temp_Height);
                    _outcomeSales = App.Get_Printed_Table(new CPrinting.CPrinting(), Sales_DG, Outcome_DG, new string[] { "", "" });
                }
                Columns_width = new int[] { 245, 30, 79, 70, 200, 103 };
                Temp_Width = e.MarginBounds.Left;
                e.Graphics.DrawString("Outcome", Header_Font, Foreground, new RectangleF(e.MarginBounds.Left, Temp_Height, 424, 30), Center);
                e.Graphics.DrawString("Sales", Header_Font, Foreground, new RectangleF(e.MarginBounds.Left + 424, Temp_Height, e.MarginBounds.Width - 424, 30), Center);
                Temp_Height += 30;
                e.Graphics.DrawLine(Border_Pen, e.MarginBounds.Left, Temp_Height, e.MarginBounds.Right, Temp_Height);

                for (int i = 1; i < 5; i++)
                {
                    if (Sales_DG.Columns[i].Header.ToString() == "Gas")
                    {
                        e.Graphics.DrawString("Gas", new Font("Cambria", 10), Foreground, new RectangleF(Temp_Width, Temp_Height, Columns_width[i - 1], 35), Center);
                    }
                    else
                    {
                        e.Graphics.DrawString(Sales_DG.Columns[i].Header.ToString(), Cell_Font, Foreground, new RectangleF(Temp_Width, Temp_Height, Columns_width[i - 1], 35), Center);
                    }
                    Temp_Width += Columns_width[i - 1];
                }
                for (int i = 0; i < 2; i++)
                {
                    e.Graphics.DrawString(Outcome_DG.Columns[i].Header.ToString(), Cell_Font, Foreground, new RectangleF(Temp_Width, Temp_Height, Columns_width[i + 4], 35), Center);
                    Temp_Width += Columns_width[i + 4];
                }
                Temp_Height += 35;
                e.Graphics.DrawLine(Cells_Pen, e.MarginBounds.Left, Temp_Height, e.MarginBounds.Right, Temp_Height);
                while (_rowIndex < _outcomeSales.Rows.Count)
                {

                    if (Temp_Height + 30 > e.MarginBounds.Bottom)
                    {
                        Temp_Width = e.MarginBounds.Left;
                        foreach (int i in Columns_width)
                        {
                            Temp_Width += i;
                            e.Graphics.DrawLine(Cells_Pen, Temp_Width,  pumps_height, Temp_Width, Temp_Height);
                        }
                        e.Graphics.DrawLine(Border_Pen, e.MarginBounds.Left, e.MarginBounds.Top + 375, e.MarginBounds.Left, Temp_Height);
                        e.Graphics.DrawLine(Border_Pen, e.MarginBounds.Right, e.MarginBounds.Top + 375, e.MarginBounds.Right, Temp_Height);
                        e.Graphics.DrawLine(Spliter_Pen, e.MarginBounds.Left + 424, e.MarginBounds.Top + 375, e.MarginBounds.Left + 424, Temp_Height);
                        e.Graphics.DrawLine(Border_Pen, e.MarginBounds.Left, Temp_Height, e.MarginBounds.Right, Temp_Height);
                        page_num++;
                        return true;
                    }

                    DataRow row = _outcomeSales.Rows[_rowIndex];

                    Temp_Width = e.MarginBounds.Left;
                    j = 0;
                    foreach (DataColumn column in _outcomeSales.Columns)
                    {
                        switch (j)
                        {
                            case 0:
                            case 4:
                                e.Graphics.DrawString(row[column].ToString(), new Font("Cambria", 12), Foreground, new RectangleF(Temp_Width, Temp_Height, Columns_width[j], 30), Center);
                                break;
                            default:
                                e.Graphics.DrawString(row[column].ToString(), Numbers_Font, Foreground, new RectangleF(Temp_Width - 3, Temp_Height, Columns_width[j], 30), Right);
                                break;
                        }
                        Temp_Width += Columns_width[j];
                        j++;
                    }
                    Temp_Height += 28;
                    e.Graphics.DrawLine(Cells_Pen, e.MarginBounds.Left, Temp_Height, e.MarginBounds.Right, Temp_Height);
                    _rowIndex++;
                }

                Temp_Width = e.MarginBounds.Left;
                if (page_num == 0)
                {
                    foreach (int i in Columns_width)
                    {
                        Temp_Width += i;
                        e.Graphics.DrawLine(Cells_Pen, Temp_Width,  pumps_height, Temp_Width, Temp_Height);
                    }
                    e.Graphics.DrawLine(Border_Pen, e.MarginBounds.Left, e.MarginBounds.Top + 375, e.MarginBounds.Left, Temp_Height);
                    e.Graphics.DrawLine(Border_Pen, e.MarginBounds.Right, e.MarginBounds.Top + 375, e.MarginBounds.Right, Temp_Height);
                    e.Graphics.DrawLine(Spliter_Pen, e.MarginBounds.Left + 424, e.MarginBounds.Top + 375, e.MarginBounds.Left + 424, Temp_Height);
                    e.Graphics.DrawLine(Border_Pen, e.MarginBounds.Left, Temp_Height, e.MarginBounds.Right, Temp_Height);
                }
                else
                {
                    foreach (int i in Columns_width)
                    {
                        Temp_Width += i;
                        e.Graphics.DrawLine(Cells_Pen, Temp_Width, e.MarginBounds.Top + 30, Temp_Width, Temp_Height);
                    }
                    e.Graphics.DrawLine(Border_Pen, e.MarginBounds.Left, e.MarginBounds.Top, e.MarginBounds.Left, Temp_Height);
                    e.Graphics.DrawLine(Border_Pen, e.MarginBounds.Right, e.MarginBounds.Top, e.MarginBounds.Right, Temp_Height);
                    e.Graphics.DrawLine(Spliter_Pen, e.MarginBounds.Left + 424, e.MarginBounds.Top, e.MarginBounds.Left + 424, Temp_Height);
                    e.Graphics.DrawLine(Border_Pen, e.MarginBounds.Left, Temp_Height, e.MarginBounds.Right, Temp_Height);
                    e.Graphics.DrawLine(Border_Pen, e.MarginBounds.Left, e.MarginBounds.Top, e.MarginBounds.Right, e.MarginBounds.Top);

                }
                Temp_Height += 20;
                e.Graphics.DrawString("Money To be Deposit in bank: " + Total_Income_TK.Text, Header_Font, Foreground, new RectangleF(e.MarginBounds.Left, Temp_Height, e.MarginBounds.Width, 30));
                _rowIndex = 0;
                page_num = 0;
                return false;
            }
            catch
            {
                return false;
            }

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _pd.DefaultPageSettings.Margins = new Margins(50, 50, 50, 50);
                CPrinting.CPrinting Printer = new CPrinting.CPrinting();
                Printer.PrintDocument = _pd;
                Printer.print();
            }
            catch
            {

            }
        }
    }
}
