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
    public partial class Outcome_Office : Window
    {
        object Outcome_Id;


        public Outcome_Office(object outcome_id = null)
        {
            InitializeComponent();

            Outcome_Id = outcome_id;
            Get_Outcome();

        }

        private void Get_Outcome()
        {
            try
            {
                DB db2 = new DB("outcome_office");

                db2.SelectedColumns.Add("*");

                db2.AddCondition("oto_id", Outcome_Id);

                DataRow DR = db2.SelectRow();

                Date_TB.Value = DateTime.Parse(DR["oto_date"].ToString());
                Value_TB.Text = DR["oto_value"].ToString();
                Description_TB.Text = DR["oto_description"].ToString();

            }
            catch
            {


            }
        }

        private void add_update_outcome_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                if (Add_Update())
                {
                    var log = new Log();
                    log.Columns.Add(new Column("التاريـخ", Date_TB.Value.Value.ToShortDateString()));
                    log.Columns.Add(new Column("القيـمــة", Value_TB.Text));
                    log.Columns.Add(new Column("البيـــان", Description_TB.Text));
                    log.CreateLog("المصروفات", this.Outcome_Id == null);
                    if ((bool)New.IsChecked)
                    {
                        Value_TB.Text = "";
                        Description_TB.Text = "";
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

                DB DataBase = new DB("outcome_office");

                DataBase.AddColumn("oto_date", Date_TB.Text);
                DataBase.AddColumn("oto_value", Value_TB.Text);
                DataBase.AddColumn("oto_description", Description_TB.Text);

                if (this.Outcome_Id == null)
                {
                    if (DataBase.IsNotExist("oto_id", "oto_description", "oto_date", "oto_value"))
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
                    DataBase.AddCondition("oto_id", this.Outcome_Id);
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
