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
    public partial class Pump_read : Page
    {
        object Pump_Read_Id;

        public Pump_read(object pump_Read_Id = null)
        {
            InitializeComponent();
            Pump_Read_Id = pump_Read_Id;

            Gas.Get_All_Gas(Gas_CB);
            Gas_CB.SelectedIndex = 0;
            Pump_CB.SelectedIndex = 0;

            Get_Outcome();

        }

        private void Get_Outcome()
        {
            try
            {
                DB db2 = new DB("pump_read");



                db2.AddCondition("pmr_id", Pump_Read_Id);

                DataRow DR = db2.SelectRow("select p.*,pum_gas_id from pump_read p join pumps on pmr_pum_id=pum_id");

                Date_TB.Value = DateTime.Parse(DR["pmr_date"].ToString());
                Gas_CB.SelectedValue = DR["pum_gas_id"];
                Pumps.Get_All_Pumps(Pump_CB, Gas_CB.SelectedValue);
                Pump_CB.SelectedValue = DR["pmr_pum_id"];
                Value_TB.Text = DR["pmr_value"].ToString();


            }
            catch
            {


            }
        }

        private decimal Get_Pump_Value()
        {
            decimal value = 0, Today_Read = 0, Yesterday_Read = 0;

            try
            {
                Today_Read = decimal.Parse(Value_TB.Text.Trim());
                Yesterday_Read = Get_Amount();
                value = Today_Read >= Yesterday_Read ? (Today_Read - Yesterday_Read) :
                    (Properties.Settings.Default.Pump_Max - Yesterday_Read + Today_Read);
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

                if(Notify.validate("من فضلك ادخل التاريخ", Date_TB.Text, Station.GetWindow(this)))
                {
                    return;
                }

                if(Notify.validate("من فضلك اختر المحروق", Gas_CB.SelectedIndex, Station.GetWindow(this)))
                {
                    return;
                }


                if(Notify.validate("من فضلك اختر العداد", Pump_CB.SelectedIndex, Station.GetWindow(this)))
                {
                    return;
                }

                if(Notify.validate("من فضلك ادخل القراءة", Value_TB.Text, Station.GetWindow(this)))
                {
                    return;
                }

                if(Value_TB.Text == "")
                {
                    if(Pump_CB.SelectedIndex == Pump_CB.Items.Count - 1)
                    {
                        Gas_CB.SelectedIndex++;
                        Pump_CB.SelectedIndex = 0;
                    }
                    else
                    {
                        Pump_CB.SelectedIndex++;
                    }
                }
                else
                {
                    if(Add_Update())
                    {
                        var log = new Log();
                        log.Columns.Add(new Column("التاريخ", Date_TB.Value.Value.ToShortDateString()));
                        log.Columns.Add(new Column("المحروق", Gas_CB.Text));
                        log.Columns.Add(new Column("العداد", Pump_CB.Text));
                        log.Columns.Add(new Column("القراءه", Value_TB.Text));
                        log.CreateLog("قراءه طلمبات", Pump_Read_Id == null);

                        Value_TB.Clear();
                        Value_TB.Focus();
                        if(Station.Mode == Operations.Edit)
                        {
                            Window w = Station.GetWindow(this);
                            w.Close();
                        }
                        if(Pump_CB.SelectedIndex == Pump_CB.Items.Count - 1)
                        {
                            Gas_CB.SelectedIndex++;
                            Pump_CB.SelectedIndex = 0;
                        }
                        else
                        {
                            Pump_CB.SelectedIndex++;
                        }
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
                Source.DB DB1 = new Source.DB("pump_read");

                DB1.AddColumn("pmr_pum_id", Pump_CB.SelectedValue);
                DB1.AddColumn("pmr_date", Date_TB.Text);
                DB1.AddColumn("pmr_value", Value_TB.Text);

                Source.DB DB2 = new Source.DB("pump_sales");
                DB2.AddColumn("pms_amount", Get_Pump_Value());


                if(this.Pump_Read_Id == null)
                {
                    DB2.AddColumn("pms_pmr_id", DB1);
                    DB2.AddColumn("pms_pum_id", Pump_CB.SelectedValue);
                    DB2.AddColumn("pms_date", Date_TB.Text);

                    if(DB1.IsNotExist("pmr_id", "pmr_pum_id", "pmr_date"))
                    {

                        return Confirm.Check(DB1.Execute_Queries(DB1, DB2));

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

                    DB1.AddCondition("pmr_id", this.Pump_Read_Id);
                    DB2.AddCondition("pms_pum_id", Pump_CB.SelectedValue);
                    DB2.AddCondition("pms_date", Date_TB.Text);
                    return Confirm.Check(DB1.Execute_Queries(DB1, DB2));
                }
            }
            catch
            {
                return false;
            }
        }

        //public bool add_update_PMS(object id)
        //{
        //    try
        //    {
        //        DB DataBase = new DB("pump_sales");

        //        if(this.Pump_Read_Id == null)
        //        {
        //            DataBase.AddColumn("pms_pmr_id", id);
        //            DataBase.AddColumn("pms_pum_id", Pump_CB.SelectedValue);
        //            DataBase.AddColumn("pms_date", Date_TB.Text);
        //            DataBase.AddColumn("pms_amount", Get_Pump_Value());
        //            return DataBase.Insert();
        //        }
        //        else
        //        {
        //            DataBase.AddCondition("pms_pum_id", Pump_CB.SelectedValue);
        //            DataBase.AddCondition("pms_date", Date_TB.Text);
        //            DataBase.AddColumn("pms_amount", Get_Pump_Value());
        //            return DataBase.Update();
        //        }
        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //}

        private void Gas_CB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                Pumps.Get_All_Pumps(Pump_CB, Gas_CB.SelectedValue);
            }
            catch
            {

            }
        }

        private decimal Get_Amount()
        {
            decimal value = 0;
            try
            {
                DB db = new DB();
                db.AddCondition("pmr_date", Date_TB.Value.Value.Date.AddDays(-1), false);
                db.AddCondition("pmr_pum_id", Pump_CB.SelectedValue);
                decimal.TryParse(db.Select(@"select pmr_value from pump_read").ToString(), out value);
            }
            catch
            {

            }
            return value;
        }

    }
}
