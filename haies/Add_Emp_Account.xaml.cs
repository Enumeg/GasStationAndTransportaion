using System;
using System.Data;
using System.Windows;
using Source;

namespace haies
{
    /// <summary>
    /// Interaction logic for Add_Emp_Account.xaml
    /// </summary>
    public partial class Add_Emp_Account : Window
    {
        object Ema_Id, Emp_Id;

        public Add_Emp_Account(object emp_Id, object ema_Id = null)
        {
            InitializeComponent();

            Ema_Id = ema_Id;
            Emp_Id = emp_Id;
            Get_New_No();
            Get_Accounts_Types();
            if (ema_Id != null)
            {
                Get_Account_Info();
            }
        }

        private void Get_New_No()
        {
            try
            {
                DB DataBase = new DB();

                string DR = DataBase.Select("select max(ema_no) from employees_accounts").ToString();

                if (DR == "")
                {
                    No_TB.Text = DateTime.Now.ToString("yy") + "0001";
                }
                else
                {
                    No_TB.Text = (int.Parse(DR) + 1).ToString();
                }

            }
            catch
            {


            }
        }

        private void Get_Account_Info()
        {
            try
            {
                DB DataBase = new DB("employees_accounts");

                DataBase.SelectedColumns.Add("*");

                DataBase.AddCondition("ema_id", Ema_Id);

                DataRow DR = DataBase.SelectRow();

                No_TB.Text = DR["ema_no"].ToString();
                Accounts_CB.SelectedValue = DR["ema_acc_id"];
                Date_DTP.Value = DateTime.Parse(DR["ema_date"].ToString());
                Value_TB.Text = DR["ema_value"].ToString();
                Description_TB.Text = DR["ema_description"].ToString();

            }
            catch
            {


            }
        }

        private void Get_Accounts_Types()
        {
            try
            {
                DB DataBase = new DB("accounts_types");
                DataBase.Fill(Accounts_CB, "acc_id", "acc_name", "select * from accounts_types");

            }
            catch
            {


            }
        }

        private bool Add_Update()
        {
            try
            {

                DB DataBase = new DB("employees_accounts");

                DataBase.AddColumn("ema_no", No_TB.Text.Trim());
                DataBase.AddColumn("ema_acc_id", Accounts_CB.SelectedValue);
                DataBase.AddColumn("ema_date", Date_DTP.Value.Value.Date);
                DataBase.AddColumn("ema_value", Value_TB.Text.Trim());
                DataBase.AddColumn("ema_description", Description_TB.Text.Trim());
                if (Ema_Id == null)
                {
                    DataBase.AddColumn("ema_emp_id", Emp_Id);
                    return DataBase.Insert();

                }
                else
                {

                    DataBase.AddCondition("ema_id", Ema_Id);
                    return DataBase.Update();
                }
            }
            catch
            {
                return false;
            }
        }

        private void Add_Update_Account_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Confirm.Check(Add_Update()))
                {
                    var log = new Log();
                    log.Columns.Add(new Column("التاريـخ", Date_DTP.Value.Value.Date.ToShortDateString()));
                    log.Columns.Add(new Column("الرقــــم", No_TB.Text));
                    log.Columns.Add(new Column("الحساب", Accounts_CB.Text));
                    log.Columns.Add(new Column("القيـمــة", Value_TB.Text));
                    log.Columns.Add(new Column("البيـــان", Description_TB.Text));
                    log.CreateLog("حسابات الموظفين", Ema_Id == null);

                    if ((bool)New.IsChecked)
                    {
                        No_TB.Text = Value_TB.Text = Description_TB.Text = "";
                        Accounts_CB.SelectedIndex = -1;
                        Get_New_No();
                    }
                    else
                    {
                        this.Close();
                    }
                }
            }
            catch
            {

            }
        }

    }
}
