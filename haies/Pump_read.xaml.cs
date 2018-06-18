using System;
using System.Windows;
using System.Windows.Controls;
using Source;

namespace haies
{
    /// <summary>
    /// Interaction logic for outcome.xaml
    /// </summary>
    public partial class PumpRead
    {
        readonly object _pumpReadId;

        public PumpRead(object pumpReadId = null)
        {
            InitializeComponent();
            _pumpReadId = pumpReadId;

            Gas.Get_All_Gas(Gas_CB);
            Gas_CB.SelectedIndex = 0;
            Pump_CB.SelectedIndex = 0;

            Get_Outcome();

        }

        private void Get_Outcome()
        {
            try
            {
                var db2 = new DB("pump_read");



                db2.AddCondition("pmr_id", _pumpReadId);

                var dr = db2.SelectRow("select p.*,pum_gas_id from pump_read p join pumps on pmr_pum_id=pum_id");

                Date_TB.Value = DateTime.Parse(dr["pmr_date"].ToString());
                Gas_CB.SelectedValue = dr["pum_gas_id"];
                Pumps.Get_All_Pumps(Pump_CB, Gas_CB.SelectedValue);
                Pump_CB.SelectedValue = dr["pmr_pum_id"];
                Value_TB.Text = dr["pmr_value"].ToString();


            }
            catch
            {


            }
        }

        private decimal Get_Pump_Value()
        {
            decimal value = 0, todayRead = 0, yesterdayRead = 0;

            try
            {
                todayRead = decimal.Parse(Value_TB.Text.Trim());
                yesterdayRead = Get_Amount();
                value = todayRead >= yesterdayRead ? (todayRead - yesterdayRead) :
                    (Properties.Settings.Default.Pump_Max - yesterdayRead + todayRead);
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

                if(Notify.validate("من فضلك ادخل التاريخ", Date_TB.Text, Window.GetWindow(this)))
                {
                    return;
                }

                if(Notify.validate("من فضلك اختر المحروق", Gas_CB.SelectedIndex, Window.GetWindow(this)))
                {
                    return;
                }


                if(Notify.validate("من فضلك اختر العداد", Pump_CB.SelectedIndex, Window.GetWindow(this)))
                {
                    return;
                }

                if(Notify.validate("من فضلك ادخل القراءة", Value_TB.Text, Window.GetWindow(this)))
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
                        log.CreateLog("قراءه طلمبات", _pumpReadId == null);

                        Value_TB.Clear();
                        Value_TB.Focus();
                        if(Station.Mode == Operations.Edit)
                        {
                            var w = Window.GetWindow(this);
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
                var db1 = new DB("pump_read");

                db1.AddColumn("pmr_pum_id", Pump_CB.SelectedValue);
                db1.AddColumn("pmr_date", Date_TB.Text);
                db1.AddColumn("pmr_value", Value_TB.Text);

                var db2 = new DB("pump_sales");
                var salesAmount = Get_Pump_Value();
                db2.AddColumn("pms_amount", salesAmount);
                db2.AddColumn("pms_price_id", Get_Sale_Price());


                if (_pumpReadId == null)
                {
                    db2.AddColumn("pms_pmr_id", db1);
                    db2.AddColumn("pms_pum_id", Pump_CB.SelectedValue);
                    db2.AddColumn("pms_date", Date_TB.Text);

                    if(db1.IsNotExist("pmr_id", "pmr_pum_id", "pmr_date"))
                    {

                        return Confirm.Check(db1.Execute_Queries(db1, db2));

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

                    db1.AddCondition("pmr_id", _pumpReadId);
                    db2.AddCondition("pms_pum_id", Pump_CB.SelectedValue);
                    db2.AddCondition("pms_date", Date_TB.Text);
                    return Confirm.Check(db1.Execute_Queries(db1, db2));
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
                var db = new DB();
                db.AddCondition("pmr_date", Date_TB.Value.Value.Date.AddDays(-1), false);
                db.AddCondition("pmr_pum_id", Pump_CB.SelectedValue);
                decimal.TryParse(db.Select(@"select pmr_value from pump_read").ToString(), out value);
            }
            catch
            {

            }
            return value;
        }
        private Decimal Get_Sale_Price()
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
    }
}
