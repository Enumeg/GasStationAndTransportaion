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
    /// Interaction logic for Advance.xaml
    /// </summary>
    public partial class Advance : Window
    {
        object Advance_Id;
        DataRowView Person;

        public Advance(DataRowView person_id,object adv_id=null)
        {
            InitializeComponent();
            
            Advance_Id = adv_id;
            Person = person_id;
            Get_Outcome();

        }

        private void Get_Outcome()
        {
            try
            {
                    DB db2 = new DB("advance");

                    db2.SelectedColumns.Add("*");

                    db2.AddCondition("adv_id", Advance_Id);

                    DataRow DR = db2.SelectRow();

                    Date_TB.Value = DateTime.Parse(DR["adv_date"].ToString());
                    Value_TB.Text = DR["adv_value"].ToString();
                    Description_TB.Text = DR["adv_description"].ToString();

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
                    log.Columns.Add(new Column("إسم المالك", Person["per_name"]));
                    log.CreateLog("دفعات ملاك السيارات", Advance_Id == null);

                    if ((bool)New.IsChecked)
                    {

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

                DB DataBase = new DB("advance");

                DataBase.AddColumn("adv_date", Date_TB.Text);
                DataBase.AddColumn("adv_value", Value_TB.Text);
                DataBase.AddColumn("adv_description", Description_TB.Text);
                DataBase.AddColumn("adv_per_id", Person["per_id"]);



                if (this.Advance_Id == null)
                {
                    if (DataBase.IsNotExist("adv_id", "adv_description","adv_per_id"))
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
                    DataBase.AddCondition("adv_id", this.Advance_Id);
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
