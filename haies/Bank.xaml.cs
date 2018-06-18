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
    public partial class Bank : Page
    {
        object Bank_Id;

        public Bank(object bnk_id = null)
        {
            InitializeComponent();
            Bank_Id = bnk_id;
            Get_Balance();
            Get_Bank_ID();

        }

        private void Get_Bank_ID()
        {
            try
            {
                DB db2 = new DB("bank");

                db2.SelectedColumns.Add("*");

                db2.AddCondition("bnk_id", Bank_Id);

                DataRow DR = db2.SelectRow();

                Date_TB.Value = DateTime.Parse(DR["bnk_date"].ToString());
                Value_TB.Text = DR["bnk_value"].ToString();
                Description_TB.Text = DR["bnk_description"].ToString();

            }
            catch
            {


            }
        }

        private void add_update_outcome_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Notify.validate("من فضلك ادخل التاريخ", Date_TB.Text, Cement_Office.GetWindow(this)))
                {
                    return;
                }

                if (Notify.validate("من فضلك ادخل القيمه", Value_TB.Text, Cement_Office.GetWindow(this)))
                {
                    return;
                }





                if (Add_Update())
                {
                    var log = new Log();
                    log.Columns.Add(new Column("التاريـخ", Date_TB.Value.Value.Date.ToShortDateString()));
                    log.Columns.Add(new Column("القيـمــة", Value_TB.Text));
                    log.Columns.Add(new Column("البيـــان", Description_TB.Text));
                    log.CreateLog("البنك - المحطة", Bank_Id == null);
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

                DB DataBase = new DB("bank");

                DataBase.AddColumn("bnk_date", Date_TB.Text);
                DataBase.AddColumn("bnk_value", Value_TB.Text);
                DataBase.AddColumn("bnk_description", Description_TB.Text);


                if (this.Bank_Id == null)
                {
                    if (DataBase.IsNotExist("bnk_id", "bnk_description", "bnk_date"))
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
                    DataBase.AddCondition("bnk_id", this.Bank_Id);
                    return Confirm.Check(DataBase.Update());
                }
            }
            catch
            {
                return false;
            }
        }

        private void Get_Balance()
        {

            try
            {
                DB db = new DB();
                db.AddCondition("pmr_date", Date_TB.Value.Value.Date, false, "=", "Today");
                Value_TB.Text = decimal.Parse(db.Select(@"select COALESCE(sum(pms_amount*gsp_sellCost),0)   +
                                           (select COALESCE(sum(cstl_value),0) from customer_loans where cstl_date=@Today)-(
                                           (select COALESCE(sum(sin_amount*gsp_sellCost),0) from station_income
                                            join gas_price on sin_price_id = gsp_id where sin_date=@Today)+
                                           (select COALESCE(sum(sout_value),0) from station_outcome where sout_date=@Today))
                                           from pump_sales ps join pumps p on ps.pms_pum_id=p.pum_id
                                           join gas_price on pms_price_id = gsp_id where pms_date=@Today").ToString()).ToString("0.00");

            }
            catch
            {

            }
        }

        //close class
    }
}
