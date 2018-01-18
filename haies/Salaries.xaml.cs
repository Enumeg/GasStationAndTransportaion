using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Source;
namespace haies
{
    /// <summary>
    /// Interaction logic for Product.xaml
    /// </summary>

    public partial class Salaries : Page
    {
        int SelectedIndex = 0, RN = 0;

        DataTable Salaries_Table, Emps;

        public object stay, passport, person;

        PrintDocument PD;

        public Salaries()
        {
            InitializeComponent();
            Fill_Employees_LB();
            Emps = new DataTable();
            Salaries_Table = new DataTable();
            Salaries_Table.Columns.Add("م");
            Salaries_Table.Columns.Add("الإسم");
            Salaries_Table.Columns.Add("الأساسي");
            Salaries_Table.Columns.Add("الحوافز");
            Salaries_Table.Columns.Add("المكافآت");
            Salaries_Table.Columns.Add("الإجمالي");
            Salaries_Table.Columns.Add("الجزاءات");
            Salaries_Table.Columns.Add("الإستقطاعات");
            Salaries_Table.Columns.Add("السلف");
            Salaries_Table.Columns.Add("الإجازات");
            Salaries_Table.Columns.Add("الإجمالى");
            Salaries_Table.Columns.Add("الصافي");
            Salaries_Table.Columns.Add("ملاحظات");
            Salaries_Table.Columns.Add("التوقيع");
            PD = new PrintDocument();
            PD.PrintPage += new PrintPageEventHandler(PD_PrintPage);
            PD.BeginPrint += new PrintEventHandler(PD_BeginPrint);
        }

        private void Fill_Employees_LB()
        {
            try
            {
                DB db2 = new DB("persons");

                // search by name
                db2.AddCondition("per_name", "%" + Name_Search_TB.Text + "%", false, " like ", "per_name");

                // search by passport
                db2.AddCondition("p2.pap_number", "%" + Passport_Search_TB.Text + "%", false, " like ");


                // search by stay
                db2.AddCondition("p1.pap_number", "%" + Stay_Search_TB.Text + "%", false, " like ");

                db2.Fill(LB, "emp_id", "name", @"select emp.*,p1.pap_id stay_id,p2.pap_id passport,per.per_id person_id,
                              p1.pap_number stay_number,p1.pap_start stay_start,p1.pap_end stay_end,p2.pap_number passport_number,p2.pap_start pass_start,p2.pap_end pass_end, 
                              per.per_name name,per.per_address,per.per_mobile
                              from employees emp join paper p1 on p1.pap_id=emp.emp_stay_id
                              join paper p2 on p2.pap_id=emp.emp_passport
                              join persons per on per_id=emp_per_id order by per_name");

            }
            catch
            {

            }
        }

        private void Fill_Receipts()
        {
            DB db = new DB();
            decimal Total = 0;
            try
            {
                db.AddCondition("trs_date", From_DTP.Value.Value.Date, false, ">=", "SD");
                db.AddCondition("trs_date", To_DTP.Value.Value.Date, false, "<=", "ED");
                db.AddCondition("p1.per_id", ((DataRowView)LB.SelectedItem)["person_id"], false, " = ", "per_id");
                DataTable dt = db.SelectTable(@"select rec_number,trs_date,trs_dri_motive,p2.per_name customer,car_number,pl_name place from receipt r                                                                                                                                                                                                                                                                                 
                                                join transportation t on t.trs_rec_id=r.rec_id   
                                                join drivers d on d.dri_id=trs_dri_id
                                                join cars cs on trs_car_id=car_id 
                                                join persons p1 on p1.per_id=d.dri_per_id   
       								            left join customer c on  c.cust_id= t.trs_cust_id    
										     	left join persons p2 on p2.per_id=c.cust_per_id  
												left join places p on p.pl_id=t.trs_pl_id                         
                                                where trs_date>=@SD and trs_date<=@ED order by trs_date,rec_number;");
                foreach (DataRow row in dt.Rows)
                {
                    Total += decimal.Parse(row["trs_dri_motive"].ToString());
                }
                dt.Rows.Add();
                dt.Rows[dt.Rows.Count - 1]["trs_dri_motive"] = Total;
                Receipt_DG.ItemsSource = dt.DefaultView;

            }
            catch
            {

            }
        }

        #region Vacation
        private void Fill_Vacation()
        {
            try
            {
                DB db = new DB();
                db.AddCondition("emp_id", LB.SelectedValue);
                db.AddCondition("SD", From_DTP.Value.Value.Date);
                db.AddCondition("ED", To_DTP.Value.Value.Date);
                Vacation_DG.ItemsSource = db.SelectTableView("select * from vacation where vac_emp_id=@emp_id and ((vac_from>=@SD and vac_from<=@ED) or (vac_to>=@SD and vac_to<=@ED))");

            }
            catch
            {

            }
        }

        private void Vacation_EP_Add(object sender, EventArgs e)
        {
            try
            {
                Vacation vac = new Vacation(LB.SelectedValue);
                vac.ShowDialog();
                Fill_Vacation();
            }
            catch
            {

            }
        }

        private void Vacation_EP_Edit(object sender, EventArgs e)
        {
            try
            {
                Vacation vac = new Vacation(LB.SelectedValue, ((DataRowView)Vacation_DG.SelectedItem)["vac_id"]);
                vac.ShowDialog();
                Fill_Vacation();
            }
            catch
            {

            }
        }

        private void Vacation_EP_Delete(object sender, EventArgs e)
        {
            try
            {
                if (Message.Show("هل تريد الحذف", MessageBoxButton.YesNoCancel, 10) == MessageBoxResult.Yes)
                {
                    var row = ((DataRowView)Vacation_DG.SelectedItem);
                    DB db = new DB("vacation");
                    db.AddCondition("vac_id", row["vac_id"]);
                    if (db.Delete())
                    {
                        var log = new Log();
                        log.Columns.Add(new Column("من",   row["vac_from"]));
                        log.Columns.Add(new Column("إلى",  row["vac_to"]));
                        log.Columns.Add(new Column("السبب",row["vac_reason"]));
                        log.CreateLog("الأجازات");
                        Fill_Vacation(); 
                    }
                }
            }
            catch
            {

            }
        }

        #endregion

        #region Tax_Bouns

        private void Fill_All()
        {
            try
            {
                decimal balance = 0;
                DataRow Row = App.Accounts.Rows[Account_CB.SelectedIndex - 3];
                int[] accounts = new int[2];
                accounts[0] = int.Parse(Row[1].ToString());
                accounts[1] = int.Parse(Row[2].ToString());
                balance = Account_CB.SelectedIndex == 4 ? Get_Balance(From_DTP.Value.Value.Date, accounts[1], accounts[0]) : Get_Balance(From_DTP.Value.Value.Date, accounts[0], accounts[1]);
                Tax_Bouns_DG.SetValue(Grid.ColumnProperty, Account_CB.SelectedIndex);

                DataTable dt = new DataTable();
                dt.Columns.Add("ema_id");
                dt.Columns.Add("مدين");
                dt.Columns.Add("دائن");
                dt.Columns.Add("الرصيد");
                dt.Columns.Add("البيان");
                dt.Columns.Add("السند");
                dt.Columns.Add("التاريخ", typeof(DateTime));



                DB db = new DB();
                db.AddCondition("ema_emp_id", LB.SelectedValue);
                db.AddCondition("ema_acc_id", accounts[0], false, "=", "acc_id_1");
                db.AddCondition("ema_acc_id", accounts[1], false, "=", "acc_id_2");
                db.AddCondition("ema_date", From_DTP.Value.Value.Date, false, ">=", "SD");
                db.AddCondition("ema_date", To_DTP.Value.Value.Date, false, "<=", "ED");
                dt.Rows.Add();
                dt.Rows[0]["الرصيد"] = balance;
                foreach (DataRow row in db.SelectTable(@"SELECT * FROM employees_accounts where (ema_acc_id = @acc_id_1 OR ema_acc_id = @acc_id_2)").Rows)
                {
                    if (Account_CB.SelectedIndex == 4)
                    {

                        if (row["ema_acc_id"].ToString() == accounts[0].ToString())
                        {
                            balance += decimal.Parse(row["ema_value"].ToString());
                            dt.Rows.Add(row["ema_id"], null, row["ema_value"], balance, row["ema_description"], row["ema_no"], row["ema_date"]);
                        }
                        else
                        {
                            balance -= decimal.Parse(row["ema_value"].ToString());
                            dt.Rows.Add(row["ema_id"], row["ema_value"], null, balance, row["ema_description"], row["ema_no"], row["ema_date"]);
                        }
                    }
                    else
                    {
                        if (row["ema_acc_id"].ToString() == accounts[0].ToString())
                        {
                            balance -= decimal.Parse(row["ema_value"].ToString());
                            dt.Rows.Add(row["ema_id"], null, row["ema_value"], balance, row["ema_description"], row["ema_no"], row["ema_date"]);
                        }
                        else
                        {
                            balance += decimal.Parse(row["ema_value"].ToString());
                            dt.Rows.Add(row["ema_id"], row["ema_value"], null, balance, row["ema_description"], row["ema_no"], row["ema_date"]);
                        }
                    }
                }
                dt.Rows.Add();
                dt.Rows[dt.Rows.Count - 1]["الرصيد"] = balance;
                Tax_Bouns_DG.ItemsSource = dt.DefaultView;
            }
            catch
            {

            }
        }

        private decimal Get_Balance(DateTime Date, object acc1, object acc2)
        {
            decimal value = 0;
            try
            {
                DB db = new DB();
                db.AddCondition("ema_date", Date);
                db.AddCondition("ema_acc_id", acc1, false,"=", "acc_id_1");
                db.AddCondition("ema_acc_id", acc2, false,"=", "acc_id_2");
                value = decimal.Parse(db.Select(@"select COALESCE(sum(ema_value),0) -(select COALESCE(sum(ema_value),0) 
                                                  from employees_accounts where ema_date < @ema_date and ema_acc_id = @acc_id_1)
                                                  from employees_accounts where ema_date < @ema_date and ema_acc_id = @acc_id_2").ToString());

            }
            catch
            {

            }
            return value;
        }

        private void Fill6_All()
        {
            try
            {
                decimal total = 0;
                Tax_Bouns_DG.SetValue(Grid.ColumnProperty, Account_CB.SelectedIndex);
                //All_EP.SetValue(Grid.ColumnProperty, Account_CB.SelectedIndex);
                DB db = new DB();
                db.AddCondition("tax_emp_id", LB.SelectedValue);
                db.AddCondition("tax_type", Account_CB.SelectedIndex - 2);
                db.AddCondition("tax_date", From_DTP.Value.Value.Date, false, ">=", "SD");
                db.AddCondition("tax_date", To_DTP.Value.Value.Date, false, "<=", "ED");
                DataTable dt = db.SelectTable("select * from tax ");
                foreach (DataRow row in dt.Rows)
                {
                    total += decimal.Parse(row["tax_value"].ToString());
                }
                dt.Rows.Add();
                dt.Rows[dt.Rows.Count - 1]["tax_value"] = total;
                Tax_Bouns_DG.ItemsSource = dt.DefaultView;
            }
            catch
            {

            }
        }

        private void Bonus_EP_Add(object sender, EventArgs e)
        {
            try
            {
                Tax t = new Tax((Tax_Bouns)Account_CB.SelectedIndex - 2, LB.SelectedValue);
                t.ShowDialog();
                Fill_All();
            }
            catch
            {

            }
        }
        private void Bonus_EP_Edit(object sender, EventArgs e)
        {
            try
            {
                Tax t = new Tax((Tax_Bouns)Account_CB.SelectedIndex - 2, LB.SelectedValue, ((DataRowView)Tax_Bouns_DG.SelectedItem)["tax_id"]);
                t.ShowDialog();
                Fill_All();
            }
            catch
            {

            }
        }
        private void Bonus_EP_Delete(object sender, EventArgs e)
        {
            try
            {
                if (Message.Show("هل تريد الحذف", MessageBoxButton.YesNoCancel, 10) == MessageBoxResult.Yes)
                {
                    var row =((DataRowView)Tax_Bouns_DG.SelectedItem);
                    DB db = new DB("tax");
                    db.AddCondition("tax_id", row["tax_id"]);                    
                    if (db.Delete())
                    {
                          var log = new Log();
                        log.Columns.Add(new Column("من",   row["vac_from"]));
                        log.Columns.Add(new Column("إلى",  row["vac_to"]));
                        log.Columns.Add(new Column("السبب",row["vac_reason"]));
                        log.CreateLog("الأجازات");
                        Fill_All(); 
                    }
                }
            }
            catch
            {

            }
        }
        private void Discount_EP_Add(object sender, EventArgs e)
        {
            try
            {
                Tax t = new Tax(Tax_Bouns.Tax, LB.SelectedValue);
                t.ShowDialog();
                Fill_All();
            }
            catch
            {

            }
        }
        private void Discount_EP_Edit(object sender, EventArgs e)
        {
            try
            {
                Tax t = new Tax(Tax_Bouns.Tax, LB.SelectedValue, ((DataRowView)Tax_Bouns_DG.SelectedItem)["tax_id"]);
                t.ShowDialog();
                Fill_All();
            }
            catch
            {

            }
        }
        private void Discount_EP_Delete(object sender, EventArgs e)
        {
            try
            {
                if (Message.Show("هل تريد الحذف", MessageBoxButton.YesNoCancel, 10) == MessageBoxResult.Yes)
                {
                    DB db = new DB("tax");
                    db.AddCondition("tax_id", ((DataRowView)Tax_Bouns_DG.SelectedItem)["tax_id"]);
                    db.Delete();
                    Fill_All();
                }
            }
            catch
            {

            }
        }

        #endregion

        #region Salaries
        private void Get_Salaries()
        {
            try
            {
                decimal[] values,
                totals = new decimal[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                int i = 1;
                Salaries_Table.Rows.Clear();
                DB db = new DB();
                db.AddCondition("SD", From_DTP.Value.Value.Date);
                db.AddCondition("ED", To_DTP.Value.Value.Date);
                DataTable dt = db.SelectTable(@"select p1.per_name,emp_salary,COALESCE(Sum(trs_dri_motive),0) trs_dri_motive,emp_id 
                                                from persons p1 join employees on emp_per_id=per_id                                               
                                                left join drivers on dri_per_id = p1.per_id
                                                left join transportation on trs_dri_id=dri_id and trs_date>=@SD and trs_date<=@ED 
                                                group by emp_id order by p1.per_name");
                foreach (DataRow row in dt.Rows)
                {
                    values = Get_Salary(row["emp_id"], row["emp_salary"], row["trs_dri_motive"]);
                    Salaries_Table.Rows.Add(i, row[0], values[0], values[1], values[2], values[3], values[4], values[5], values[6], values[7], values[8], values[9]);
                    for (int j = 0; j < 10; j++)
                    {
                        totals[j] += values[j];
                    }
                    i++;
                }
                Salaries_Table.Rows.Add(null, "الإجمالي", totals[0], totals[1], totals[2], totals[3], totals[4], totals[5], totals[6], totals[7], totals[8], totals[9]);
            }
            catch
            {

            }
        }
        private double Get_Vacation(object emp_Id)
        {
            double value = 0;
            try
            {
                DateTime from, to;
                DB db = new DB();
                db.AddCondition("SD", From_DTP.Value.Value.Date);
                db.AddCondition("ED", To_DTP.Value.Value.Date);
                db.AddCondition("emp_id", emp_Id);
                foreach (DataRow row in db.SelectTable("select vac_from,vac_to from vacation where vac_emp_id=@emp_id and ((vac_from>=@SD and vac_from<=@ED) or (vac_to>=@SD and vac_to<=@ED))").Rows)
                {
                    from = DateTime.Parse(row[0].ToString());
                    to = DateTime.Parse(row[1].ToString());
                    if (from > From_DTP.Value.Value.Date)
                    {
                        if (to <= To_DTP.Value.Value.Date)
                        {
                            value = (to - from).TotalDays;
                        }
                        else
                        {
                            value = (To_DTP.Value.Value.Date - from).TotalDays;
                        }
                    }
                    else
                    {
                        if (to <= To_DTP.Value.Value.Date)
                        {
                            value = (to - From_DTP.Value.Value.Date).TotalDays;
                        }
                        else
                        {
                            value = (To_DTP.Value.Value.Date - From_DTP.Value.Value.Date).TotalDays;
                        }
                    }
                }
            }
            catch
            {

            }
            return value == 0 ? 0 : value + 1;
        }
        private decimal Get_Account(object emp_Id, int account_Id)
        {
            decimal value = 0;
            try
            {
                DB db = new DB();
                db.AddCondition("ema_date", From_DTP.Value.Value.Date, false, ">=", "SD");
                db.AddCondition("ema_date", To_DTP.Value.Value.Date, false, "<=", "ED");
                db.AddCondition("ema_emp_id", emp_Id);
                db.AddCondition("ema_acc_id", account_Id + 1);
                value = decimal.Parse(db.Select("select COALESCE(sum(ema_value),0) from employees_accounts").ToString());
            }
            catch
            {

            }
            return value;
        }

        private void Get_Emps()
        {
            try
            {
                DB db = new DB();
                db.AddCondition("emp_id", LB.SelectedValue, LB.SelectedIndex == -1);
                Emps = db.SelectTable(@"select p1.per_id,p1.per_name,e.emp_id,e.emp_nationality nationality,e.emp_job job,p.pap_number Num,date_format(p.pap_end,'%Y/%m/%d') ED
                                        from employees e 
                                        join paper p on e.emp_stay_id=p.pap_id 
                                        join persons p1 on p1.per_id=e.emp_per_id");
            }
            catch
            {

            }
        }
        private void Get_Emps_Salaries(PrintPageEventArgs e, DataRow info)
        {
            StringFormat Center = new StringFormat();
            Center.LineAlignment = StringAlignment.Center;
            Center.Alignment = StringAlignment.Center;
            StringFormat Right = new StringFormat();
            Right.LineAlignment = StringAlignment.Center;
            Right.Alignment = StringAlignment.Far;
            Font Header_Font = new Font("Arial", 16);
            Font Column_Font = new Font("Arial", 14);
            Font Cell_Font = new Font("Arial", 12);
            Font Numbers_Font = new Font("Tahoma", 10);
            Brush Foreground = Brushes.Black;
            Pen Border_Pen = new Pen(Foreground, 2);
            Pen Cells_Pen = new Pen(Foreground, 1);
            Pen Spliter_Pen = new Pen(Foreground, 3);
            float Temp_Height = e.MarginBounds.Top - 50, Temp_Width = e.MarginBounds.Left, tw = 0;
            decimal[] values;
            float[] widths = new float[] { };
            DB db = new DB();
            DataTable dt = new DataTable();
            try
            {
                e.Graphics.DrawString(string.Format("بيان تفصيلي براتب {0} \r\n عن الفترة من {1} إلى {2}", info["per_name"], From_DTP.Value.Value.ToString("yyyy/MM/dd"),
                To_DTP.Value.Value.ToString("yyyy/MM/dd")), Header_Font, Foreground, new RectangleF(e.MarginBounds.Left, Temp_Height, e.MarginBounds.Width, 50), Center);
                Temp_Height += 60;
                e.Graphics.DrawString("الجنسية : " + info["nationality"], Column_Font, Foreground, new RectangleF(e.MarginBounds.Left + e.MarginBounds.Width / 2, Temp_Height + 2, e.MarginBounds.Width / 2, 30), Right);
                e.Graphics.DrawString("رقم الإقــــامة :" + info["num"], Column_Font, Foreground, new RectangleF(e.MarginBounds.Left, Temp_Height + 2, e.MarginBounds.Width / 2, 30), Right);
                Temp_Height += 30;
                e.Graphics.DrawString("الوظيفة : " + info["job"], Column_Font, Foreground, new RectangleF(e.MarginBounds.Left + e.MarginBounds.Width / 2, Temp_Height + 2, e.MarginBounds.Width / 2, 30), Right);
                e.Graphics.DrawString("تاريخ الإنتهاء : " + info["ed"], Column_Font, Foreground, new RectangleF(e.MarginBounds.Left, Temp_Height + 2, e.MarginBounds.Width / 2, 30), Right);
                Temp_Height += 50;

                #region Salary
                Salaries_Table.Rows.Clear();
                db.AddCondition("SD", From_DTP.Value.Value.Date);
                db.AddCondition("ED", To_DTP.Value.Value.Date);
                db.AddCondition("emp_id", info["emp_id"]);
                DataRow Row = db.SelectRow(@"select emp_salary,COALESCE(Sum(trs_dri_motive),0) 
                                             from persons p1 join employees on emp_per_id=per_id                                               
                                             left join drivers on dri_per_id = p1.per_id
                                             left join transportation on trs_dri_id=dri_id and trs_date>=@SD and trs_date<=@ED ");
                values = Get_Salary(info["emp_id"], Row[0], Row[1]);
                Salaries_Table.Rows.Add(null, null, values[0], values[1], values[2], values[3], values[4], values[5], values[6], values[7], values[8], values[9]);
                e.Graphics.DrawString("الراتب", Header_Font, Foreground, new RectangleF(e.MarginBounds.Left, Temp_Height + 2, e.MarginBounds.Width, 30), Center);
                e.Graphics.DrawRectangle(Border_Pen, e.MarginBounds.Left, Temp_Height, e.MarginBounds.Width, 30);
                Temp_Height += 30;
                tw = (e.MarginBounds.Width / 10.0f);
                for (int i = 2; i < 12; i++)
                {
                    Temp_Width = e.MarginBounds.Right - ((i - 1) * tw);
                    e.Graphics.DrawString(Salaries_Table.Columns[i].ColumnName, Column_Font, Foreground, new RectangleF(Temp_Width, Temp_Height + 30, tw, 30), Center);
                    e.Graphics.DrawString(Salaries_Table.Rows[0][i].ToString(), Numbers_Font, Foreground, new RectangleF(Temp_Width, Temp_Height + 60, tw, 30), Center);
                    if (Salaries_Table.Columns[i].ColumnName == "الإجمالي" || Salaries_Table.Columns[i].ColumnName == "الإجمالى")
                    {
                        e.Graphics.DrawLine(Border_Pen, Temp_Width, Temp_Height, Temp_Width, Temp_Height + 90);
                    }
                    else
                    {
                        e.Graphics.DrawLine(Cells_Pen, Temp_Width, Temp_Height, Temp_Width, Temp_Height + 90);
                    }
                }
                e.Graphics.FillRectangle(Brushes.White, new RectangleF(e.MarginBounds.Left + tw * 6 + 1, Temp_Height + 1, tw * 4 - 2, 28));
                e.Graphics.DrawString("المستحقات", Column_Font, Foreground, new RectangleF(e.MarginBounds.Left + tw * 6, Temp_Height + 1, tw * 4, 28), Center);
                e.Graphics.FillRectangle(Brushes.White, new RectangleF(e.MarginBounds.Left + tw + 1, Temp_Height + 1, tw * 5 - 2, 28));
                e.Graphics.DrawString("الخصومات", Column_Font, Foreground, new RectangleF(e.MarginBounds.Left + tw, Temp_Height + 1, tw * 5, 28), Center);

                e.Graphics.DrawLine(Border_Pen, e.MarginBounds.Right, Temp_Height, e.MarginBounds.Right, Temp_Height + 90);
                e.Graphics.DrawLine(Border_Pen, e.MarginBounds.Left, Temp_Height, e.MarginBounds.Left, Temp_Height + 90);
                Temp_Height += 60;
                e.Graphics.DrawLine(Cells_Pen, e.MarginBounds.Left, Temp_Height - 30, e.MarginBounds.Right, Temp_Height - 30);
                e.Graphics.DrawLine(Cells_Pen, e.MarginBounds.Left, Temp_Height, e.MarginBounds.Right, Temp_Height);
                Temp_Height += 30;
                e.Graphics.DrawLine(Border_Pen, e.MarginBounds.Left, Temp_Height, e.MarginBounds.Right, Temp_Height);
                #endregion

                #region Motives

                db = new DB();
                db.AddCondition("trs_date", From_DTP.Value.Value.Date, false, ">=", "SD");
                db.AddCondition("trs_date", To_DTP.Value.Value.Date, false, "<=", "ED");
                db.AddCondition("p1.per_id", info["per_id"], false, " = ", "per_id");
                dt = db.SelectTable(@"select date_format(trs_date,'%Y/%m/%d'),rec_number,pl_name place,p2.per_name customerr,car_number,trs_dri_motive from receipt r                                                                                                                                                                                                                                                                                 
                                                join transportation t on t.trs_rec_id=r.rec_id   
                                                join drivers d on d.dri_id=trs_dri_id
                                                join cars cs on trs_car_id=car_id 
                                                join persons p1 on p1.per_id=d.dri_per_id   
       								            left join customer c on  c.cust_id= t.trs_cust_id    
												left join persons p2 on p2.per_id=c.cust_per_id    
												left join places p on p.pl_id=t.trs_pl_id                         
                                                where trs_date>=@SD and trs_date<=@ED order by trs_date,rec_number;");
                if (dt.Rows.Count > 0)
                {
                    e.Graphics.DrawString("الحوافز", Header_Font, Foreground, new RectangleF(e.MarginBounds.Left, Temp_Height, e.MarginBounds.Width, 30), Center);
                    e.Graphics.DrawRectangle(Border_Pen, e.MarginBounds.Left, Temp_Height, e.MarginBounds.Width, 30);
                    Temp_Height += 30;
                    tw = Temp_Height;
                    widths = new float[] { 120, 80, 117, 150, 100, 60 };
                    Temp_Width = e.MarginBounds.Right;
                    for (int i = 0; i < Receipt_DG.Columns.Count; i++)
                    {
                        Temp_Width -= widths[i];
                        e.Graphics.DrawString(Receipt_DG.Columns[i].Header.ToString(), Column_Font, Foreground, new RectangleF(Temp_Width, Temp_Height, widths[i], 30), Center);
                    }
                    Temp_Height += 30;
                    e.Graphics.DrawLine(Cells_Pen, e.MarginBounds.Left, Temp_Height, e.MarginBounds.Right, Temp_Height);
                    foreach (DataRow row in dt.Rows)
                    {
                        Temp_Width = e.MarginBounds.Right;
                        for (int i = 0; i < Receipt_DG.Columns.Count; i++)
                        {
                            Temp_Width -= widths[i];
                            e.Graphics.DrawString(row[i].ToString(), Cell_Font, Foreground, new RectangleF(Temp_Width, Temp_Height, widths[i], 27), Center);
                        }
                        Temp_Height += 27;
                        e.Graphics.DrawLine(Cells_Pen, e.MarginBounds.Left, Temp_Height, e.MarginBounds.Right, Temp_Height);
                    }
                    Temp_Width = e.MarginBounds.Right;
                    for (int i = 0; i < Receipt_DG.Columns.Count; i++)
                    {
                        Temp_Width -= widths[i];
                        e.Graphics.DrawLine(Cells_Pen, Temp_Width, tw, Temp_Width, Temp_Height);
                    }
                    e.Graphics.DrawLine(Cells_Pen, Temp_Width, tw, Temp_Width, Temp_Height);
                    e.Graphics.DrawLine(Border_Pen, e.MarginBounds.Right, tw, e.MarginBounds.Right, Temp_Height);
                    e.Graphics.DrawLine(Border_Pen, e.MarginBounds.Left, tw, e.MarginBounds.Left, Temp_Height);
                }
                #endregion

                foreach (DataRow account in App.Accounts.Rows)
                {
                    dt = Get_Accounts(info["emp_id"], int.Parse(account[1].ToString()));
                    if (dt.Rows.Count > 0)
                    {
                        e.Graphics.DrawString(account[0].ToString(), Header_Font, Foreground, new RectangleF(e.MarginBounds.Left, Temp_Height + 3, e.MarginBounds.Width, 30), Center);
                        e.Graphics.DrawRectangle(Border_Pen, e.MarginBounds.Left, Temp_Height, e.MarginBounds.Width, 30);
                        Temp_Height += 30;
                        tw = Temp_Height;
                        widths = new float[] { 127, 100, 400 };
                        Temp_Width = e.MarginBounds.Right - widths[0];
                        e.Graphics.DrawString("التاريخ", Column_Font, Foreground, new RectangleF(Temp_Width, Temp_Height, widths[0], 30), Center);
                        Temp_Width -= widths[1];
                        e.Graphics.DrawString("القيمة", Column_Font, Foreground, new RectangleF(Temp_Width, Temp_Height, widths[1], 30), Center);
                        Temp_Width -= widths[2];
                        e.Graphics.DrawString("السبب", Column_Font, Foreground, new RectangleF(Temp_Width, Temp_Height, widths[2], 30), Center);
                        Temp_Height += 30;
                        e.Graphics.DrawLine(Cells_Pen, e.MarginBounds.Left, Temp_Height, e.MarginBounds.Right, Temp_Height);
                        foreach (DataRow row in dt.Rows)
                        {
                            Temp_Width = e.MarginBounds.Right - widths[0];
                            e.Graphics.DrawString(DateTime.Parse(row["ema_date"].ToString()).ToString("yyyy/MM/dd"), Cell_Font, Foreground, new RectangleF(Temp_Width, Temp_Height, widths[0], 30), Center);
                            Temp_Width -= widths[1];
                            e.Graphics.DrawString(row["ema_value"].ToString(), Cell_Font, Foreground, new RectangleF(Temp_Width, Temp_Height, widths[1], 30), Center);
                            Temp_Width -= widths[2];
                            e.Graphics.DrawString(row["ema_description"].ToString(), Cell_Font, Foreground, new RectangleF(Temp_Width, Temp_Height, widths[2], 30), Center);
                            Temp_Height += 30;
                            e.Graphics.DrawLine(Cells_Pen, e.MarginBounds.Left, Temp_Height, e.MarginBounds.Right, Temp_Height);
                        }
                        Temp_Width = e.MarginBounds.Right - widths[0];
                        e.Graphics.DrawLine(Cells_Pen, Temp_Width, tw, Temp_Width, Temp_Height);
                        Temp_Width -= widths[1];
                        e.Graphics.DrawLine(Cells_Pen, Temp_Width, tw, Temp_Width, Temp_Height);
                        Temp_Width -= widths[2];
                        e.Graphics.DrawLine(Cells_Pen, Temp_Width, tw, Temp_Width, Temp_Height);
                        e.Graphics.DrawLine(Border_Pen, e.MarginBounds.Right, tw, e.MarginBounds.Right, Temp_Height);
                        e.Graphics.DrawLine(Border_Pen, e.MarginBounds.Left, tw, e.MarginBounds.Left, Temp_Height);
                    }
                }

                #region Tax
                ////db = new DB();
                ////db.AddCondition("tax_emp_id", info["emp_id"]);
                ////db.AddCondition("tax_type", Tax_Bouns.Tax);
                ////db.AddCondition("tax_date", From_DTP.Value.Value.Date, false, ">=", "SD");
                ////db.AddCondition("tax_date", To_DTP.Value.Value.Date, false, "<=", "ED");

                //dt = Get_Accounts(info["emp_id"], (int)Account_Types.Discount);
                //if (dt.Rows.Count > 0)
                //{
                //    e.Graphics.DrawString("الحسومات", Header_Font, Foreground, new RectangleF(e.MarginBounds.Left, Temp_Height, e.MarginBounds.Width, 30), Center);
                //    e.Graphics.DrawRectangle(Border_Pen, e.MarginBounds.Left, Temp_Height, e.MarginBounds.Width, 30);
                //    Temp_Height += 30;
                //    tw = Temp_Height;
                //    widths = new float[] { 127, 100, 400 };
                //    Temp_Width = e.MarginBounds.Right - widths[0];
                //    e.Graphics.DrawString("التاريخ", Column_Font, Foreground, new RectangleF(Temp_Width, Temp_Height, widths[0], 30), Center);
                //    Temp_Width -= widths[1];
                //    e.Graphics.DrawString("القيمة", Column_Font, Foreground, new RectangleF(Temp_Width, Temp_Height, widths[1], 30), Center);
                //    Temp_Width -= widths[2];
                //    e.Graphics.DrawString("السبب", Column_Font, Foreground, new RectangleF(Temp_Width, Temp_Height, widths[2], 30), Center);
                //    Temp_Height += 30;
                //    e.Graphics.DrawLine(Cells_Pen, e.MarginBounds.Left, Temp_Height, e.MarginBounds.Right, Temp_Height);
                //    foreach (DataRow row in dt.Rows)
                //    {
                //        Temp_Width = e.MarginBounds.Right - widths[0];
                //        e.Graphics.DrawString(DateTime.Parse(row["tax_date"].ToString()).ToString("yyyy/MM/dd"), Cell_Font, Foreground, new RectangleF(Temp_Width, Temp_Height, widths[0], 30), Center);
                //        Temp_Width -= widths[1];
                //        e.Graphics.DrawString(row["tax_value"].ToString(), Cell_Font, Foreground, new RectangleF(Temp_Width, Temp_Height, widths[1], 30), Center);
                //        Temp_Width -= widths[2];
                //        e.Graphics.DrawString(row["tax_reason"].ToString(), Cell_Font, Foreground, new RectangleF(Temp_Width, Temp_Height, widths[2], 30), Center);
                //        Temp_Height += 30;
                //        e.Graphics.DrawLine(Cells_Pen, e.MarginBounds.Left, Temp_Height, e.MarginBounds.Right, Temp_Height);
                //    }
                //    Temp_Width = e.MarginBounds.Right - widths[0];
                //    e.Graphics.DrawLine(Cells_Pen, Temp_Width, tw, Temp_Width, Temp_Height);
                //    Temp_Width -= widths[1];
                //    e.Graphics.DrawLine(Cells_Pen, Temp_Width, tw, Temp_Width, Temp_Height);
                //    Temp_Width -= widths[2];
                //    e.Graphics.DrawLine(Cells_Pen, Temp_Width, tw, Temp_Width, Temp_Height);
                //    e.Graphics.DrawLine(Border_Pen, e.MarginBounds.Right, tw, e.MarginBounds.Right, Temp_Height);
                //    e.Graphics.DrawLine(Border_Pen, e.MarginBounds.Left, tw, e.MarginBounds.Left, Temp_Height);
                //}
                //#endregion

                //#region Due

                //db.Conditions[1].Value = Tax_Bouns.Due;
                //dt = Get_Accounts(info["emp_id"], (int)Account_Types.Due_Off);
                //if (dt.Rows.Count > 0)
                //{
                //    e.Graphics.DrawString("الإستقطاعات", Header_Font, Foreground, new RectangleF(e.MarginBounds.Left, Temp_Height + 3, e.MarginBounds.Width, 30), Center);
                //    e.Graphics.DrawRectangle(Border_Pen, e.MarginBounds.Left, Temp_Height, e.MarginBounds.Width, 30);
                //    Temp_Height += 30;
                //    tw = Temp_Height;
                //    widths = new float[] { 127, 100, 400 };
                //    Temp_Width = e.MarginBounds.Right - widths[0];
                //    e.Graphics.DrawString("التاريخ", Column_Font, Foreground, new RectangleF(Temp_Width, Temp_Height, widths[0], 30), Center);
                //    Temp_Width -= widths[1];
                //    e.Graphics.DrawString("القيمة", Column_Font, Foreground, new RectangleF(Temp_Width, Temp_Height, widths[1], 30), Center);
                //    Temp_Width -= widths[2];
                //    e.Graphics.DrawString("السبب", Column_Font, Foreground, new RectangleF(Temp_Width, Temp_Height, widths[2], 30), Center);
                //    Temp_Height += 30;
                //    e.Graphics.DrawLine(Cells_Pen, e.MarginBounds.Left, Temp_Height, e.MarginBounds.Right, Temp_Height);
                //    foreach (DataRow row in dt.Rows)
                //    {
                //        Temp_Width = e.MarginBounds.Right - widths[0];
                //        e.Graphics.DrawString(DateTime.Parse(row["tax_date"].ToString()).ToString("yyyy/MM/dd"), Cell_Font, Foreground, new RectangleF(Temp_Width, Temp_Height, widths[0], 30), Center);
                //        Temp_Width -= widths[1];
                //        e.Graphics.DrawString(row["tax_value"].ToString(), Cell_Font, Foreground, new RectangleF(Temp_Width, Temp_Height, widths[1], 30), Center);
                //        Temp_Width -= widths[2];
                //        e.Graphics.DrawString(row["tax_reason"].ToString(), Cell_Font, Foreground, new RectangleF(Temp_Width, Temp_Height, widths[2], 30), Center);
                //        Temp_Height += 30;
                //        e.Graphics.DrawLine(Cells_Pen, e.MarginBounds.Left, Temp_Height, e.MarginBounds.Right, Temp_Height);
                //    }
                //    Temp_Width = e.MarginBounds.Right - widths[0];
                //    e.Graphics.DrawLine(Cells_Pen, Temp_Width, tw, Temp_Width, Temp_Height);
                //    Temp_Width -= widths[1];
                //    e.Graphics.DrawLine(Cells_Pen, Temp_Width, tw, Temp_Width, Temp_Height);
                //    Temp_Width -= widths[2];
                //    e.Graphics.DrawLine(Cells_Pen, Temp_Width, tw, Temp_Width, Temp_Height);
                //    e.Graphics.DrawLine(Border_Pen, e.MarginBounds.Right, tw, e.MarginBounds.Right, Temp_Height);
                //    e.Graphics.DrawLine(Border_Pen, e.MarginBounds.Left, tw, e.MarginBounds.Left, Temp_Height);
                //}
                //#endregion

                //#region Advance

                //db.Conditions[1].Value = Tax_Bouns.Advance;
                //dt = db.SelectTable("select * from tax");
                //if (dt.Rows.Count > 0)
                //{
                //    e.Graphics.DrawString("السلف", Header_Font, Foreground, new RectangleF(e.MarginBounds.Left, Temp_Height + 3, e.MarginBounds.Width, 30), Center);
                //    e.Graphics.DrawRectangle(Border_Pen, e.MarginBounds.Left, Temp_Height, e.MarginBounds.Width, 30);
                //    Temp_Height += 30;
                //    tw = Temp_Height;
                //    widths = new float[] { 127, 100, 400 };
                //    Temp_Width = e.MarginBounds.Right - widths[0];
                //    e.Graphics.DrawString("التاريخ", Column_Font, Foreground, new RectangleF(Temp_Width, Temp_Height, widths[0], 30), Center);
                //    Temp_Width -= widths[1];
                //    e.Graphics.DrawString("القيمة", Column_Font, Foreground, new RectangleF(Temp_Width, Temp_Height, widths[1], 30), Center);
                //    Temp_Width -= widths[2];
                //    e.Graphics.DrawString("السبب", Column_Font, Foreground, new RectangleF(Temp_Width, Temp_Height, widths[2], 30), Center);
                //    Temp_Height += 30;
                //    e.Graphics.DrawLine(Cells_Pen, e.MarginBounds.Left, Temp_Height, e.MarginBounds.Right, Temp_Height);
                //    foreach (DataRow row in dt.Rows)
                //    {
                //        Temp_Width = e.MarginBounds.Right - widths[0];
                //        e.Graphics.DrawString(DateTime.Parse(row["tax_date"].ToString()).ToString("yyyy/MM/dd"), Cell_Font, Foreground, new RectangleF(Temp_Width, Temp_Height, widths[0], 30), Center);
                //        Temp_Width -= widths[1];
                //        e.Graphics.DrawString(row["tax_value"].ToString(), Cell_Font, Foreground, new RectangleF(Temp_Width, Temp_Height, widths[1], 30), Center);
                //        Temp_Width -= widths[2];
                //        e.Graphics.DrawString(row["tax_reason"].ToString(), Cell_Font, Foreground, new RectangleF(Temp_Width, Temp_Height, widths[2], 30), Center);
                //        Temp_Height += 30;
                //        e.Graphics.DrawLine(Cells_Pen, e.MarginBounds.Left, Temp_Height, e.MarginBounds.Right, Temp_Height);
                //    }
                //    Temp_Width = e.MarginBounds.Right - widths[0];
                //    e.Graphics.DrawLine(Cells_Pen, Temp_Width, tw, Temp_Width, Temp_Height);
                //    Temp_Width -= widths[1];
                //    e.Graphics.DrawLine(Cells_Pen, Temp_Width, tw, Temp_Width, Temp_Height);
                //    Temp_Width -= widths[2];
                //    e.Graphics.DrawLine(Cells_Pen, Temp_Width, tw, Temp_Width, Temp_Height);
                //    e.Graphics.DrawLine(Border_Pen, e.MarginBounds.Right, tw, e.MarginBounds.Right, Temp_Height);
                //    e.Graphics.DrawLine(Border_Pen, e.MarginBounds.Left, tw, e.MarginBounds.Left, Temp_Height);
                //}
                //#endregion

                //#region Bouns

                //db.Conditions[1].Value = Tax_Bouns.Bouns;
                //dt = db.SelectTable("select * from tax");
                //if (dt.Rows.Count > 0)
                //{
                //    e.Graphics.DrawString("المكافآت", Header_Font, Foreground, new RectangleF(e.MarginBounds.Left, Temp_Height + 3, e.MarginBounds.Width, 30), Center);
                //    e.Graphics.DrawRectangle(Border_Pen, e.MarginBounds.Left, Temp_Height, e.MarginBounds.Width, 30);
                //    Temp_Height += 30;
                //    tw = Temp_Height;
                //    widths = new float[] { 127, 100, 400 };
                //    Temp_Width = e.MarginBounds.Right - widths[0];
                //    e.Graphics.DrawString("التاريخ", Column_Font, Foreground, new RectangleF(Temp_Width, Temp_Height, widths[0], 30), Center);
                //    Temp_Width -= widths[1];
                //    e.Graphics.DrawString("القيمة", Column_Font, Foreground, new RectangleF(Temp_Width, Temp_Height, widths[1], 30), Center);
                //    Temp_Width -= widths[2];
                //    e.Graphics.DrawString("السبب", Column_Font, Foreground, new RectangleF(Temp_Width, Temp_Height, widths[2], 30), Center);
                //    Temp_Height += 30;
                //    e.Graphics.DrawLine(Cells_Pen, e.MarginBounds.Left, Temp_Height, e.MarginBounds.Right, Temp_Height);
                //    foreach (DataRow row in dt.Rows)
                //    {
                //        Temp_Width = e.MarginBounds.Right - widths[0];
                //        e.Graphics.DrawString(DateTime.Parse(row["tax_date"].ToString()).ToString("yyyy/MM/dd"), Cell_Font, Foreground, new RectangleF(Temp_Width, Temp_Height, widths[0], 30), Center);
                //        Temp_Width -= widths[1];
                //        e.Graphics.DrawString(row["tax_value"].ToString(), Cell_Font, Foreground, new RectangleF(Temp_Width, Temp_Height, widths[1], 30), Center);
                //        Temp_Width -= widths[2];
                //        e.Graphics.DrawString(row["tax_reason"].ToString(), Cell_Font, Foreground, new RectangleF(Temp_Width, Temp_Height, widths[2], 30), Center);
                //        Temp_Height += 30;
                //        e.Graphics.DrawLine(Cells_Pen, e.MarginBounds.Left, Temp_Height, e.MarginBounds.Right, Temp_Height);
                //    }
                //    Temp_Width = e.MarginBounds.Right - widths[0];
                //    e.Graphics.DrawLine(Cells_Pen, Temp_Width, tw, Temp_Width, Temp_Height);
                //    Temp_Width -= widths[1];
                //    e.Graphics.DrawLine(Cells_Pen, Temp_Width, tw, Temp_Width, Temp_Height);
                //    Temp_Width -= widths[2];
                //    e.Graphics.DrawLine(Cells_Pen, Temp_Width, tw, Temp_Width, Temp_Height);
                //    e.Graphics.DrawLine(Border_Pen, e.MarginBounds.Right, tw, e.MarginBounds.Right, Temp_Height);
                //    e.Graphics.DrawLine(Border_Pen, e.MarginBounds.Left, tw, e.MarginBounds.Left, Temp_Height);
                //}
                #endregion

                #region Vacation
                db = new DB();
                db.AddCondition("emp_id", info["emp_id"]);
                db.AddCondition("SD", From_DTP.Value.Value.Date);
                db.AddCondition("ED", To_DTP.Value.Value.Date);
                dt = db.SelectTable("select * from vacation where vac_emp_id=@emp_id and ((vac_from>=@SD and vac_from<=@ED) or (vac_to>=@SD and vac_to<=@ED))");
                if (dt.Rows.Count > 0)
                {
                    e.Graphics.DrawString("الأجازات", Header_Font, Foreground, new RectangleF(e.MarginBounds.Left, Temp_Height + 3, e.MarginBounds.Width, 30), Center);
                    e.Graphics.DrawRectangle(Border_Pen, e.MarginBounds.Left, Temp_Height, e.MarginBounds.Width, 30);
                    Temp_Height += 30;
                    tw = Temp_Height;
                    widths = new float[] { 120, 120, 387 };
                    Temp_Width = e.MarginBounds.Right - widths[0];
                    e.Graphics.DrawString("من", Column_Font, Foreground, new RectangleF(Temp_Width, Temp_Height, widths[0], 30), Center);
                    Temp_Width -= widths[1];
                    e.Graphics.DrawString("إلى", Column_Font, Foreground, new RectangleF(Temp_Width, Temp_Height, widths[1], 30), Center);
                    Temp_Width -= widths[2];
                    e.Graphics.DrawString("السبب", Column_Font, Foreground, new RectangleF(Temp_Width, Temp_Height, widths[2], 30), Center);
                    Temp_Height += 30;
                    e.Graphics.DrawLine(Cells_Pen, e.MarginBounds.Left, Temp_Height, e.MarginBounds.Right, Temp_Height);
                    foreach (DataRow row in dt.Rows)
                    {
                        Temp_Width = e.MarginBounds.Right - widths[0];
                        e.Graphics.DrawString(DateTime.Parse(row["vac_from"].ToString()).ToString("yyyy/MM/dd"), Cell_Font, Foreground, new RectangleF(Temp_Width, Temp_Height, widths[0], 30), Center);
                        Temp_Width -= widths[1];
                        e.Graphics.DrawString(DateTime.Parse(row["vac_to"].ToString()).ToString("yyyy/MM/dd"), Cell_Font, Foreground, new RectangleF(Temp_Width, Temp_Height, widths[1], 30), Center);
                        Temp_Width -= widths[2];
                        e.Graphics.DrawString(row["vac_reason"].ToString(), Cell_Font, Foreground, new RectangleF(Temp_Width, Temp_Height, widths[2], 30), Center);
                        Temp_Height += 30;
                        e.Graphics.DrawLine(Cells_Pen, e.MarginBounds.Left, Temp_Height, e.MarginBounds.Right, Temp_Height);
                    }
                    Temp_Width = e.MarginBounds.Right - widths[0];
                    e.Graphics.DrawLine(Cells_Pen, Temp_Width, tw, Temp_Width, Temp_Height);
                    Temp_Width -= widths[1];
                    e.Graphics.DrawLine(Cells_Pen, Temp_Width, tw, Temp_Width, Temp_Height);
                    Temp_Width -= widths[2];
                    e.Graphics.DrawLine(Cells_Pen, Temp_Width, tw, Temp_Width, Temp_Height);
                    e.Graphics.DrawLine(Border_Pen, e.MarginBounds.Right, tw, e.MarginBounds.Right, Temp_Height);
                    e.Graphics.DrawLine(Border_Pen, e.MarginBounds.Left, tw, e.MarginBounds.Left, Temp_Height);
                }
                #endregion



            }
            catch
            {

            }
        }
        private void Get_Emp_Salary()
        {

            decimal[] values;

            DB db = new DB();
            DataTable dt = new DataTable();
            try
            {
                Salaries_Table.Rows.Clear();
                db.AddCondition("SD", From_DTP.Value.Value.Date);
                db.AddCondition("ED", To_DTP.Value.Value.Date);
                db.AddCondition("emp_id", LB.SelectedValue);
                DataRow row = db.SelectRow(@"select emp_salary,COALESCE(Sum(trs_dri_motive),0) 
                                             from persons p1 join employees on emp_per_id=per_id                                               
                                             left join drivers on dri_per_id = p1.per_id
                                             left join transportation on trs_dri_id=dri_id and trs_date>=@SD and trs_date<=@ED ");
                values = Get_Salary(LB.SelectedValue, row[0], row[1]);
                Salaries_Table.Rows.Add(null, null, values[0], values[1], values[2], values[3], values[4], values[5], values[6], values[7], values[8], values[9]);
                Salary_DG.ItemsSource = Salaries_Table.DefaultView;
            }
            catch
            {

            }
        }
        private DataTable Get_Accounts(object emp_Id, int account_Id)
        {
            DataTable Table = new DataTable();
            try
            {
                DB db = new DB();
                db.AddCondition("ema_date", From_DTP.Value.Value.Date, false, ">=", "SD");
                db.AddCondition("ema_date", To_DTP.Value.Value.Date, false, "<=", "ED");
                db.AddCondition("ema_emp_id", emp_Id);
                db.AddCondition("ema_acc_id", account_Id);
                Table = db.SelectTable("select * from employees_accounts");
            }
            catch
            {

            }
            return Table;
        }
        private decimal[] Get_Salary(object emp_Id, object salary, object motive)
        {

            decimal[] values = new decimal[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            try
            {
                values[0] = decimal.Parse(salary.ToString());
                values[1] = decimal.Parse(motive.ToString());
                values[2] = Get_Account(emp_Id, (int)Account_Types.Reward);
                values[3] = values[0] + values[1] + values[2];
                values[4] = Get_Account(emp_Id, (int)Account_Types.Discount);
                values[5] = Get_Account(emp_Id, (int)Account_Types.Due_Off);
                values[6] = Get_Account(emp_Id, (int)Account_Types.Advance_Off);
                values[7] = Math.Round((decimal.Parse(salary.ToString()) / DateTime.DaysInMonth(From_DTP.Value.Value.Year, From_DTP.Value.Value.Month)) * Convert.ToInt16(Get_Vacation(emp_Id)), 2);
                values[8] = values[4] + values[5] + values[6] + values[7];
                values[9] = values[3] - values[8];
            }
            catch
            {

            }
            return values;
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StringFormat sf = new StringFormat();
                sf.Alignment = StringAlignment.Center;
                sf.LineAlignment = StringAlignment.Center;
                Get_Salaries();
                CPrinting.CPrinting print = new CPrinting.CPrinting();
                print.PrintDocument.DefaultPageSettings.Margins = new System.Drawing.Printing.Margins(40, 65, 165, 100);
                print.BackgroundImage = System.Drawing.Image.FromFile(@".\PL.jpeg");
                print.header.Add(string.Format("مسير الرواتب  عن الفترة من {0} إلى {1}", From_DTP.Value.Value.ToString("yyyy/MM/dd"), To_DTP.Value.Value.ToString("yyyy/MM/dd")));
                print.fonts[CPrinting.CPrinting.FontElement.ColumnHeader] = new Font("Arial", 12);
                print.printedDataTable.Add(Salaries_Table);
                print.PrintDocument.DefaultPageSettings.Landscape = true;
                print.columnsWidth.Add("م", 40);
                print.columnsDirection.Add("م", sf);
                print.ColumnsLine.Add("الإجمالي");
                print.ColumnsLine.Add("الإجمالى");
                for (int i = 2; i < 6; i++)
                {
                    print.Groups.Add(print.printedDataTable[0].Columns[i].ColumnName, "المستحقات");
                }
                for (int i = 6; i < 11; i++)
                {
                    print.Groups.Add(print.printedDataTable[0].Columns[i].ColumnName, "الخصومات");
                }
                for (int i = 2; i < 11; i++)
                {
                    print.columnsWidth.Add(print.printedDataTable[0].Columns[i].ColumnName, 75);
                    print.columnsFonts.Add(print.printedDataTable[0].Columns[i].ColumnName, new Font("Tahome", 9));
                    print.columnsDirection.Add(print.printedDataTable[0].Columns[i].ColumnName, sf);
                }
                print.print();
            }
            catch
            {

            }
        }
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {

                PD.DefaultPageSettings.Margins = new Margins(50, 50, 100, 50);
                System.Windows.Forms.PrintPreviewDialog PPD = new System.Windows.Forms.PrintPreviewDialog();
                PPD.Document = PD;
                PPD.ShowDialog();
            }
            catch
            {

            }
        }
        private void PD_PrintPage(object sender, PrintPageEventArgs e)
        {
            try
            {

                Get_Emps_Salaries(e, Emps.Rows[RN]);
                RN++;
                if (Emps.Rows.Count > RN)
                {
                    e.HasMorePages = true;
                }
                else
                {
                    RN = 0;
                    Emps.Rows.Clear();
                }

            }
            catch
            {

            }
        }
        private void PD_BeginPrint(object sender, PrintEventArgs e)
        {
            try
            {
                Get_Emps();
            }
            catch
            {

            }
        }
        #endregion

        #region Filters
        private void Name_Search_TB_TextChanged(object sender, TextChangedEventArgs e)
        {
            Fill_Employees_LB();
        }

        private void LB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {

                switch (Account_CB.SelectedIndex)
                {
                    case 0:
                        Get_Accounts();
                        break;
                    case 1:
                        Get_Emp_Salary();
                        break;
                    case 2:
                        Fill_Receipts();
                        break;
                    case 3:
                    case 4:
                    case 5:
                    case 6:
                        Fill_All();
                        break;
                    case 7:
                        Fill_Vacation();
                        break;
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

                    switch (Account_CB.SelectedIndex)
                    {
                        case 0:
                            Get_Accounts();
                            break;
                        case 1:
                            Get_Emp_Salary();
                            break;
                        case 2:
                            Fill_Receipts();
                            break;
                        case 3:
                        case 4:
                        case 5:
                        case 6:
                            Fill_All();
                            break;
                        case 7:
                            Fill_Vacation();
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
                        Get_Accounts();
                        break;
                    case 1:
                        Get_Emp_Salary();
                        break;
                    case 2:
                        Fill_Receipts();
                        break;
                    case 3:
                    case 4:
                    case 5:
                    case 6:
                        Fill_All();
                        break;
                    case 7:
                        Fill_Vacation();
                        break;
                }



            }
            catch
            {

            }
        }


        #endregion

        #region Accounts

        private void Get_Accounts()
        {
            try
            {
                decimal balance = 0;
                balance = Get_Balance(From_DTP.Value.Value.Date);
                DB db = new DB();
                db.AddCondition("ema_emp_id", LB.SelectedValue);
                db.AddCondition("ema_date", From_DTP.Value.Value.Date, false, ">=", "SD");
                db.AddCondition("ema_date", To_DTP.Value.Value.Date, false, "<=", "ED");
                DataTable dt = new DataTable();
                dt.Columns.Add("ema_id");
                dt.Columns.Add("مدين");
                dt.Columns.Add("دائن");
                dt.Columns.Add("الرصيد");
                dt.Columns.Add("النوع");
                dt.Columns.Add("البيان");
                dt.Columns.Add("السند");
                dt.Columns.Add("التاريخ", typeof(DateTime));
                dt.Rows.Add();
                dt.Rows[0]["الرصيد"] = balance;
                foreach (DataRow row in db.SelectTable("select * from employees_accounts join accounts_types on acc_id = ema_acc_id ").Rows)
                {
                    if (row["acc_type"].ToString() == "0")
                    {
                        balance -= decimal.Parse(row["ema_value"].ToString());
                        dt.Rows.Add(row["ema_id"], null, row["ema_value"], balance, row["acc_name"], row["ema_description"], row["ema_no"], row["ema_date"]);
                    }
                    else
                    {
                        balance += decimal.Parse(row["ema_value"].ToString());
                        dt.Rows.Add(row["ema_id"], row["ema_value"], null, balance, row["acc_name"], row["ema_description"], row["ema_no"], row["ema_date"]);
                    }
                }
                Accounts_DG.ItemsSource = dt.DefaultView;
            }
            catch
            {

            }
        }

        private decimal Get_Balance(DateTime Date)
        {
            decimal value = 0;
            try
            {
                DB db = new DB();
                db.AddCondition("ema_date", Date);
                value = decimal.Parse(db.Select(@"select COALESCE(sum(ema_value),0) -(select COALESCE(sum(ema_value),0) 
                                                  from employees_accounts join accounts_types on acc_id = ema_acc_id and acc_type = 1 where ema_date < @ema_date)
                                                  from employees_accounts join accounts_types on acc_id = ema_acc_id and acc_type = 0 where ema_date < @ema_date").ToString());

            }
            catch
            {

            }
            return value;
        }

        private void Account_EP_Add(object sender, EventArgs e)
        {
            try
            {
                if (LB.SelectedIndex != -1)
                {
                    Add_Emp_Account a = new Add_Emp_Account(LB.SelectedValue);
                    a.ShowDialog();
                    Get_Accounts();
                }
            }
            catch
            {

            }
        }

        private void Account_EP_Edit(object sender, EventArgs e)
        {
            try
            {
                Add_Emp_Account a = new Add_Emp_Account(LB.SelectedValue, ((DataRowView)Accounts_DG.SelectedItem)["ema_id"]);
                a.ShowDialog();
                Get_Accounts();

            }
            catch
            {

            }
        }

        private void Account_EP_Delete(object sender, EventArgs e)
        {
            try
            {
                if (Message.Show("هل تريد الحذف", MessageBoxButton.YesNoCancel, 10) == MessageBoxResult.Yes)
                {
                    DB db = new DB("employees_accounts");
                    var row = ((DataRowView)Accounts_DG.SelectedItem);
                    db.AddCondition("ema_id", row["ema_id"]);
                    if (db.Delete())
                    {
                        var log = new Log();
                        log.Columns.Add(new Column("التاريـخ", row["التاريخ"]));
                        log.Columns.Add(new Column("الرقــــم", row["السند"]));
                        log.Columns.Add(new Column("الحساب", row["النوع"]));
                        log.Columns.Add(new Column("القيـمــة", row["مدين"] == null ? row["دائن"] : row["مدين"]));
                        log.Columns.Add(new Column("البيـــان", row["البيان"]));
                        log.CreateLog("حسابات الموظفين");
                        Get_Accounts();
                    }
                }
            }
            catch
            {

            }
        }
        #endregion

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            try
            {
                CPrinting.CPrinting print = new CPrinting.CPrinting();
                print.header.Add("بيان بحوافز " + ((DataRowView)LB.SelectedItem)["name"] + " \r\nعن الفترة من " + From_DTP.Value.Value.Date.ToString("yyyy/MM/dd") + " إلى " + To_DTP.Value.Value.Date.ToString("yyyy/MM/dd"));
                App.Get_Printed_Table(print, Receipt_DG);
                print.fonts[CPrinting.CPrinting.FontElement.ColumnHeader] = new Font("Arial", 14);
                print.print();
            }
            catch
            {

            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            try
            {
                if (LB.SelectedIndex != -1)
                {
                    Monthly_Accounts m = new Monthly_Accounts(LB.SelectedValue);
                    m.ShowDialog();
                    Account_CB.SelectedIndex = 2;
                    Account_CB.SelectedIndex = 0;
                }
                else
                {
                    Message.Show("من فضلك اختار الموظف", MessageBoxButton.OK, 5);
                }
            }
            catch
            {

            }
        }

    }
}
