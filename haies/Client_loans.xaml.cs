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
using System.IO;

namespace haies
{
    /// <summary>
    /// Interaction logic for outcome.xaml
    /// </summary>
    public partial class Client_loans : Window
    {
        object Customer_loans_Id;

        public Client_loans(object Cust_loan_id = null)
        {
            InitializeComponent();
            Drivers.Get_All_Drivers(Customer_CB, false);
            Customer_loans_Id = Cust_loan_id;
            Get_Receipts();
            Get_Cust_Loan();

        }

        private void Get_Receipts()
        {
            try
            {
                DB db = new DB();
                db.Fill(Receipt_CB, "rec_id", "rec_number", "select rec_id,rec_number from receipt where rec_number is not null", "0");
            }
            catch
            {

            }
        }

        private void Get_Cust_Loan()
        {
            try
            {
                DB db2 = new DB("client_loans");

                db2.SelectedColumns.Add("*");

                db2.AddCondition("cstl_id", Customer_loans_Id);

                DataRow DR = db2.SelectRow();
                Customer_CB.SelectedValue = DR["cstl_dri_id"];
                Date_TB.Value = DateTime.Parse(DR["cstl_date"].ToString());
                Value_TB.Text = DR["cstl_value"].ToString();
                Description_TB.Text = DR["cstl_description"].ToString();
                Receipt_CB.SelectedValue = DR["cstl_rec_id"];
            }
            catch
            {


            }
        }

        private void add_update_outcome_Click(object sender, RoutedEventArgs e)
        {
            try
            {


                if (Notify.validate("من فضلك ادخل التاريخ", Date_TB.Text, this))
                {
                    return;
                }

                if (Notify.validate("من فضلك ادخل القيمه", Value_TB.Text, this))
                {
                    return;
                }


                if (Add_Update())
                {
                    var log = new Log();
                    log.Columns.Add(new Column("السائق", Customer_CB.Text));
                    log.Columns.Add(new Column("التاريخ", Date_TB.Text));
                    log.Columns.Add(new Column("الفسح", Receipt_CB.Text));
                    log.CreateLog("مدفوعات السائقين", Customer_loans_Id == null);
                    if ((bool)New.IsChecked)
                    {

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

        public bool Add_Update()
        {
            try
            {

                DB DataBase = new DB("client_loans");

                DataBase.AddColumn("cstl_dri_id", Customer_CB.SelectedValue);
                DataBase.AddColumn("cstl_date", Date_TB.Text);
                DataBase.AddColumn("cstl_value", Value_TB.Text);
                DataBase.AddColumn("cstl_description", Description_TB.Text);
                DataBase.AddColumn("cstl_rec_id", Receipt_CB.SelectedValue);

                if (this.Customer_loans_Id == null)
                {
                    if (DataBase.IsNotExist("cstl_id", "cstl_dri_id", "cstl_description", "cstl_rec_id"))
                    {
                        return Confirm.Check(DataBase.Insert());
                    }
                    else
                    {

                        Message.Show("هذا المستند موجود من قبل", MessageBoxButton.OK, 5);
                        return false;
                    }


                }

// hena ye3ny hwa mawgod ba3mel edit
                else
                {
                    DataBase.AddCondition("cstl_id", this.Customer_loans_Id);
                    return Confirm.Check(DataBase.Update());
                }
            }
            catch
            {
                return false;
            }
        }


        //close class
    }
}
