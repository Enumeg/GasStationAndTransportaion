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
    public partial class Station_Purchases : Page
    {
        object Purchase_Id;

        public Station_Purchases(object purchase_Id = null)
        {
            InitializeComponent();
            Gas.Get_All_Gas(Gas_CB);
            Purchase_Id = purchase_Id;
            Get_Outcome();
        }

        private void Get_Outcome()
        {
            try
            {
                DB db2 = new DB("purchases");

                db2.SelectedColumns.Add("*");

                db2.AddCondition("pur_id", Purchase_Id);

                DataRow DR = db2.SelectRow();

                Date_TB.Value = DateTime.Parse(DR["pur_date"].ToString());

                Gas_CB.SelectedValue = DR["pur_gas_id"];
                
                Value_TB.Text = DR["pur_amount"].ToString();
            }
            catch
            {


            }
        }

        private decimal Get_Buy()
        {
                decimal value = 0;
                try
                {
                    var db2 = new DB("gas_price");

                    db2.AddCondition("gsp_gas_id", Gas_CB.SelectedValue);
                    db2.AddCondition("gas_date", Date_TB.Value.Value.Date);
                    var result = db2.Select("select gsp_Id from gas_price where gsp_date = (select max(gsp_date) from gas_price where gsp_date <= @gas_date) and gsp_gas_id = @gsp_gas_id");

                    decimal.TryParse(result.ToString(), out value);


                }
                catch
                {

                }
                return value;
            
        }

        private void add_update_outcome_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                if (Notify.validate("من فضلك ادخل التاريخ", Date_TB.Text, Station.GetWindow(this)))
                {
                    return;
                }

                if (Notify.validate("من فضلك اختر المحروق", Gas_CB.SelectedIndex, Station.GetWindow(this)))
                {
                    return;
                }
                if (Notify.validate("من فضلك ادخل الكميه", Value_TB.Text, Station.GetWindow(this)))
                {
                    return;
                }
                
                if (Add_Update())
                {
                    var log = new Log();
                    log.Columns.Add(new Column("التاريخ", Date_TB.Value.Value.ToShortDateString()));
                    log.Columns.Add(new Column("المحروق", Gas_CB.Text));
                    log.Columns.Add(new Column("الكمية", Value_TB.Text));
                    log.CreateLog("مشتريات المحطة", Purchase_Id == null);

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

                DB DataBase = new DB("purchases");

                DataBase.AddColumn("pur_date", Date_TB.Text);
                DataBase.AddColumn("pur_amount", Value_TB.Text);
                DataBase.AddColumn("pur_gas_id", Gas_CB.SelectedValue);
                DataBase.AddColumn("pur_price_id", Get_Buy());


                if (this.Purchase_Id == null)
                {
                    if (DataBase.IsNotExist("pur_id", "pur_gas_id", "pur_amount", "pur_date"))
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
                    DataBase.AddCondition("pur_id", this.Purchase_Id);
                    return Confirm.Check(DataBase.Update());
                }
            }
            catch
            {
                return false;
            }
        }

    }
}
