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
    public partial class Customer_Pay : Page
    {
        object Income_Id;


        public Customer_Pay(object income_Id = null)
        {
            InitializeComponent();
            Income_Id = income_Id;

            Customer.Get_All_Customers(Customer_CB, Customer_type.محطة);

            Customer_CB.SelectedIndex = 0;
            Get_Customer_Payment();

        }

        private void Get_Customer_Payment()
        {
            try
            {
                DB db2 = new DB("customer_loans");

                db2.SelectedColumns.Add("*");

                db2.AddCondition("cstl_id", Income_Id);

                DataRow DR = db2.SelectRow();

                Date_TB.Value = DateTime.Parse(DR["cstl_date"].ToString());
                Customer_CB.SelectedValue = DR["cstl_cust_id"];
                Value_TB.Text = DR["cstl_value"].ToString();
                Pay_TB.Text = DR["cstl_description"].ToString();

            }
            catch
            {


            }
        }

        private void add_update_outcome_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                if (Notify.validate("من فضلك ادخل التاريخ", Date_TB.Text, Station.GetWindow(this)))
                {
                    return;
                }

                if (Notify.validate("من فضلك اختر العميل", Customer_CB.SelectedIndex, Station.GetWindow(this)))
                {
                    return;
                }


                if (Notify.validate("من فضلك ادخل طريقه الدفع", Pay_TB.Text, Station.GetWindow(this)))
                {
                    return;
                }

                if (Notify.validate("من فضلك ادخل القيمه", Value_TB.Text, Station.GetWindow(this)))
                {
                    return;
                }


                if (Add_Update())
                {

                    var log = new Log();
                    log.Columns.Add(new Column("التاريخ", Date_TB.Text));
                    log.Columns.Add(new Column("القيمه", Value_TB.Text));
                    log.Columns.Add(new Column("العــمـيــل", Customer_CB.Text));
                    log.Columns.Add(new Column("طريقه الدفع", Pay_TB.Text));
                    log.CreateLog("مدفوعات العملاء", Income_Id == null);
                    if (Station.Mode == Operations.Edit)
                    {
                        Window w = Station.GetWindow(this);
                        w.Close();
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
                DataBase.AddColumn("cstl_description", Pay_TB.Text);



                if (this.Income_Id == null)
                {
                    if (DataBase.IsNotExist("cstl_id", "cstl_cust_id", "cstl_value", "cstl_date", "cstl_description"))
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
                    DataBase.AddCondition("cstl_id", this.Income_Id);
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
