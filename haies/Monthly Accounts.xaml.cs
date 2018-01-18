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
using System.Windows.Shapes;
using Source;
using System.Data;

namespace haies
{
    /// <summary>
    /// Interaction logic for Monthly_Accounts.xaml
    /// </summary>
    public partial class Monthly_Accounts : Window
    {
        object Emp_Id;

        public Monthly_Accounts(object emp_Id)
        {
            InitializeComponent();
            Emp_Id = emp_Id;
        }

        private double Get_Vacation()
        {
            double value = 0;
            try
            {
                DateTime from, to, From, To;
                From = new DateTime(Date_TB.Value.Value.Year, Date_TB.Value.Value.Month, 1);
                To = From.AddMonths(1).AddDays(-1);
                DB db = new DB();
                db.AddCondition("SD", From);
                db.AddCondition("ED", To);
                db.AddCondition("emp_id", Emp_Id);
                foreach (DataRow row in db.SelectTable("select vac_from,vac_to from vacation where vac_emp_id=@emp_id and ((vac_from>=@SD and vac_from<=@ED) or (vac_to>=@SD and vac_to<=@ED))").Rows)
                {
                    from = DateTime.Parse(row[0].ToString());
                    to = DateTime.Parse(row[1].ToString());
                    if (from > From)
                    {
                        if (to <= To)
                        {
                            value = (to - from).TotalDays;
                        }
                        else
                        {
                            value = (To - from).TotalDays;
                        }
                    }
                    else
                    {
                        if (to <= To)
                        {
                            value = (to - From).TotalDays;
                        }
                        else
                        {
                            value = (To - From).TotalDays;
                        }
                    }
                }
            }
            catch
            {

            }
            return value == 0 ? 0 : value + 1;
        }

        private int Get_New_No()
        {
            int No = 0;
            try
            {
                DB DataBase = new DB();

                string DR = DataBase.Select("select max(ema_no) from employees_accounts").ToString();

                if (DR == "")
                {
                    No = int.Parse(DateTime.Now.ToString("yy") + "0001");
                }
                else
                {
                    No = (int.Parse(DR) + 1);
                }

            }
            catch
            {


            }
            return No;
        }

        private void Add_Update_Monthly_Accounts_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool saved = false;
                int No = Get_New_No();
                DateTime From, To;
                From = new DateTime(Date_TB.Value.Value.Year, Date_TB.Value.Value.Month, 1);
                To = From.AddMonths(1).AddDays(-1);
                DB db = new DB();
                db.AddCondition("SD", From);
                db.AddCondition("ED", To);
                db.AddCondition("emp_id", Emp_Id);
                DataRow row = db.SelectRow(@"select emp_salary,COALESCE(Sum(trs_dri_motive),0),per_name
                                             from persons p1 join employees on emp_per_id=per_id                                               
                                             left join drivers on dri_per_id = p1.per_id
                                             left join transportation on trs_dri_id=dri_id and trs_date>=@SD and trs_date<=@ED ");

                DB DataBase;

                DataTable accounts = new DataTable();
                accounts.Columns.Add("ID"); accounts.Columns.Add("account"); accounts.Columns.Add("description");
                accounts.Rows.Add(row[0], (int)Account_Types.Salary + 1, string.Format("مرتب شهر {0}", Date_TB.Text));
                accounts.Rows.Add(row[1], (int)Account_Types.Motivations + 1, string.Format("حوافز شهر {0}", Date_TB.Text));
                accounts.Rows.Add(Math.Round((decimal.Parse(row[0].ToString()) / DateTime.DaysInMonth(Date_TB.Value.Value.Year, Date_TB.Value.Value.Month)) * Convert.ToInt16(Get_Vacation()), 2)
                                    , (int)Account_Types.Vacation + 1, string.Format("اجازات شهر {0}", Date_TB.Text));
                foreach (DataRow account in accounts.Rows)
                {
                    DataBase = new DB("employees_accounts");
                    DataBase.AddColumn("ema_no", No);
                    DataBase.AddColumn("ema_acc_id", account[1]);
                    DataBase.AddColumn("ema_date", To);
                    DataBase.AddColumn("ema_value", account[0]);
                    DataBase.AddColumn("ema_description", account[2]);
                    DataBase.AddColumn("ema_emp_id", Emp_Id);
                    saved = DataBase.Insert();
                    No++;
                }
                var log = new Log();
                log.Columns.Add(new Column("التاريخ", To));
                log.Columns.Add(new Column("الموظف", row[2]));
                log.CreateLog("القيم الشهرية", true);
                Confirm.Check(saved);
            }
            catch
            {

            }
        }
    }
}
