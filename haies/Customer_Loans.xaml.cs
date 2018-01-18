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
    public partial class Customer_Loans : Window
    {
        object Customer_loans_Id;

        public Customer_Loans(object cust = null, object Cust_loan_id = null)
        {
            InitializeComponent();
            Customer.Get_All_Customers(Customer_CB);
            Customer_loans_Id = Cust_loan_id;
            if (cust != null)
            {
                Customer_CB.SelectedValue = cust;
                Main_GD.RowDefinitions[0].Height = new GridLength(0);
            }
            Get_Cust_Loan();

        }

        private void Get_Cust_Loan()
        {
            try
            {
                DB db2 = new DB("customer_loans");

                db2.SelectedColumns.Add("*");

                db2.AddCondition("cstl_id", Customer_loans_Id);

                DataRow DR = db2.SelectRow();
                Customer_CB.SelectedValue = DR["cstl_cust_id"];
                Date_TB.Value = DateTime.Parse(DR["cstl_date"].ToString());
                Value_TB.Text = DR["cstl_value"].ToString();
                Description_TB.Text = DR["cstl_description"].ToString();

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
                    log.Columns.Add(new Column("التاريـخ", Date_TB.Value.Value.Date.ToShortDateString()));
                    log.Columns.Add(new Column("القيـمــة", Value_TB.Text));
                    log.Columns.Add(new Column("البيـــان", Description_TB.Text));
                    log.Columns.Add(new Column("إسم العميل", Customer_CB.Text));
                    log.CreateLog("دفعات العملاء", Customer_loans_Id == null);

                    if (!(bool)New.IsChecked)
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

                DB DataBase = new DB("customer_loans");

                DataBase.AddColumn("cstl_cust_id", Customer_CB.SelectedValue);
                DataBase.AddColumn("cstl_date", Date_TB.Text);
                DataBase.AddColumn("cstl_value", Value_TB.Text);
                DataBase.AddColumn("cstl_description", Description_TB.Text);




                if (this.Customer_loans_Id == null)
                {
                    if (DataBase.IsNotExist("cstl_id", "cstl_cust_id", "cstl_description"))
                    {
                        return Confirm.Check(DataBase.Insert());
                    }
                    else
                    {
                        // ye3ny hwa mawgood asln mesh ha3ml 7aga 
                        Message.Show("هذا المستند موجود من قبل", MessageBoxButton.OK, 5);
                        return false;
                        //DataBase.AddCondition("pl_id", this.placeId);
                        //DataBase.Update();

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
