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
    /// Interaction logic for Station_Outcome.xaml
    /// </summary>
    public partial class Station_Outcome : Page
    {

        object Station_Outcome_Id;

        public Station_Outcome(object car_outcome_id = null)
        {
            InitializeComponent();
            Station_Outcome_Id = car_outcome_id;
            Get_Outcome();
        }

        private void Get_Outcome()
        {
            try
            {
                DB db2 = new DB("station_outcome");

                db2.SelectedColumns.Add("*");

                db2.AddCondition("sout_id", Station_Outcome_Id);

                DataRow DR = db2.SelectRow();

                Date_TB.Value = DateTime.Parse(DR["sout_date"].ToString());
                Value_TB.Text = DR["sout_value"].ToString();
                Description_TB.Text = DR["sout_description"].ToString();
                Type_TB.Text = DR["sout_type"].ToString();

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

                if (Notify.validate("من فضلك ادخل القيمه", Value_TB.Text, Station.GetWindow(this)))
                {
                    return;
                }


                if (Add_Update())
                {
                    var log = new Log();
                    log.Columns.Add(new Column("التاريخ", Date_TB.Value.Value.ToShortDateString()));
                    log.Columns.Add(new Column("النوع", Type_TB.Text));
                    log.Columns.Add(new Column("الوصف", Description_TB.Text));
                    log.Columns.Add(new Column("القيمة", Value_TB.Text));
                    log.CreateLog("مصروفات المحطة", Station_Outcome_Id == null);
                    if (Station.Mode == Operations.Edit)
                    {
                        Window w = Station.GetWindow(this);
                        w.Close();
                    }
                    else
                    {
                        Value_TB.Text = Description_TB.Text = Type_TB.Text = "";
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

                DB DataBase = new DB("station_outcome");

                DataBase.AddColumn("sout_date", Date_TB.Text);
                DataBase.AddColumn("sout_value", Value_TB.Text);
                DataBase.AddColumn("sout_description", Description_TB.Text);
                DataBase.AddColumn("sout_type", Type_TB.Text);

                if (this.Station_Outcome_Id == null)
                {
                    if (DataBase.IsNotExist("sout_id", "sout_description", "sout_type", "sout_value"))
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
                    DataBase.AddCondition("sout_id", this.Station_Outcome_Id);
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
