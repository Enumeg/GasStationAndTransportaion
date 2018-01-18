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
    public partial class Car_Outcome : Window
    {
        object Car_Outcome_Id;
          DataRowView  Car;

        public Car_Outcome(DataRowView Car_Id, object car_outcome_id = null)
        {
            InitializeComponent();

            Car_Outcome_Id = car_outcome_id;
            Car = Car_Id;
            Get_Types();
            Get_Outcome();

        }

        private void Get_Outcome()
        {
            try
            {
                DB db2 = new DB("car_outcome");

                db2.SelectedColumns.Add("*");

                db2.AddCondition("cot_id", Car_Outcome_Id);

                DataRow DR = db2.SelectRow();

                Date_TB.Value = DateTime.Parse(DR["cot_date"].ToString());
                Value_TB.Text = DR["cot_value"].ToString();
                Type_TB.Text = DR["cot_type"].ToString();

            }
            catch
            {


            }
        }

        private void Get_Types()
        {
            try
            {
                DB db = new DB();
                db.Fill(Type_TB, "cot_type", "cot_type", "select cot_type from car_outcome group by cot_type order by cot_type");
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
                if (Notify.validate("من فضلك ادخل النوع", Type_TB.Text, this))
                {
                    return;
                }


                if (Add_Update())
                {
                    var log = new Log();
                    log.Columns.Add(new Column("السيارة", Car["car_name"].ToString()));
                    log.Columns.Add(new Column("التاريـخ", Date_TB.Value.Value.Date.ToShortDateString()));
                    log.Columns.Add(new Column("القيـمــة", Value_TB.Text));
                    log.Columns.Add(new Column("النوع", Type_TB.Text));
                    log.CreateLog("مصروفات السيارات", Car_Outcome_Id == null);

                    if ((bool)New.IsChecked)
                    {
                        Value_TB.Text = "";
                        Get_Types();
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

                DB DataBase = new DB("car_outcome");

                DataBase.AddColumn("cot_date", Date_TB.Text);
                DataBase.AddColumn("cot_value", Value_TB.Text);
                DataBase.AddColumn("cot_type", Type_TB.Text);
                DataBase.AddColumn("cot_car_id", Car["car_id"]);



                if (this.Car_Outcome_Id == null)
                {

                    return Confirm.Check(DataBase.Insert());

                }

// hena ye3ny hwa mawgod ba3mel edit
                else
                {
                    DataBase.AddCondition("cot_id", this.Car_Outcome_Id);
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
