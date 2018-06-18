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
    public partial class Station_Income : Page
    {
        object Income_Id;

        public Station_Income(object income_Id=null)
        {
            InitializeComponent();
            Income_Id = income_Id;
            Gas.Get_All_Gas(Gas_CB);
            Customer.Get_All_Customers(Customer_CB, Customer_type.محطة);
            Customer_CB.SelectedIndex = 0;
            Get_Income();
            
        }

        private void Get_Income()
        {
            try
            {
                DB db2 = new DB("station_income");

                db2.SelectedColumns.Add("*");

                db2.AddCondition("sin_id", Income_Id);

                DataRow DR = db2.SelectRow();

                Date_TB.Value = DateTime.Parse(DR["sin_date"].ToString());
                Customer_CB.SelectedValue = DR["sin_cust_id"];
                Gas_CB.SelectedValue = DR["sin_gas_id"];
                Amount_TB.Text = DR["sin_amount"].ToString();
             
            }
            catch
            {


            }
        }

        private void add_update_outcome_Click(object sender, RoutedEventArgs e)
        {
            try
            {

               

                if (Amount_TB.Text == "")
                {                  
                    Customer_CB.SelectedIndex++;
                }
                else
                {
                    if (Notify.validate("من فضلك ادخل التاريخ", Date_TB.Text, Station.GetWindow(this)))
                    {
                        return;
                    }

                    if (Notify.validate("من فضلك اختر العميل", Customer_CB.SelectedIndex, Station.GetWindow(this)))
                    {
                        return;
                    }


                    if (Notify.validate("من فضلك اختر المحروق", Gas_CB.SelectedIndex, Station.GetWindow(this)))
                    {
                        return;
                    }

                    if (Notify.validate("من فضلك ادخل الكميه", Amount_TB.Text, Station.GetWindow(this)))
                    {
                        return;
                    }
                    if (Add_Update())
                    {
                        var log = new Log();
                        log.Columns.Add(new Column("التاريخ", Date_TB.Value.Value.ToShortDateString()));
                        log.Columns.Add(new Column("المحروق", Gas_CB.Text));
                        log.Columns.Add(new Column("العــمـيــل", Customer_CB.Text));
                        log.Columns.Add(new Column("الكمـيــــه", Amount_TB.Text));
                        log.CreateLog("إيرادات المحطة", Income_Id == null);
                        Amount_TB.Clear();
                        Amount_TB.Focus();
                        if (Station.Mode == Operations.Edit)
                        {
                            Window w = Station.GetWindow(this);
                            w.Close();
                        }
                        Customer_CB.SelectedIndex++;
                    }
                }

            }
            catch
            {

            }
        }

        private Decimal Get_Sale_Price()
        {
            decimal value = 0;
            try
            {
                var db2 = new DB("gas_price");

                db2.AddCondition("gsp_gas_id", Gas_CB.SelectedValue);
                db2.AddCondition("gas_date", Date_TB.Value.Value.Date);
               var result =  db2.Select("select gsp_Id from gas_price where gsp_date = (select max(gsp_date) from gas_price where gsp_date <= @gas_date) and gsp_gas_id = @gsp_gas_id");

                decimal.TryParse(result.ToString(), out value);


            }
            catch
            {

            }
            return value;
        }

        public bool Add_Update()
        {
            try
            {

                DB DataBase = new DB("station_income");

                DataBase.AddColumn("sin_cust_id", Customer_CB.SelectedValue);
                DataBase.AddColumn("sin_date", Date_TB.Text);
                DataBase.AddColumn("sin_amount", Amount_TB.Text);
                DataBase.AddColumn("sin_gas_id", Gas_CB.SelectedValue);
                DataBase.AddColumn("sin_price_id", Get_Sale_Price());


                if (this.Income_Id == null)
                {
                    if (DataBase.IsNotExist("sin_id", "sin_cust_id", "sin_amount", "sin_date", "sin_gas_id"))
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
                    DataBase.AddCondition("sin_id", this.Income_Id);
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
