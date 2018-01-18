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
    public partial class Bank_Office : Window
    {
        object Bank_Id;

        public Bank_Office(object bank_id = null)
        {
            InitializeComponent();

            Bank_Id = bank_id;
            Get_Bank_Office();

        }

        private void Get_Bank_Office()
        {
            try
            {
                DB db2 = new DB("bank_office");

                db2.SelectedColumns.Add("*");

                db2.AddCondition("bnko_id", Bank_Id);

                DataRow DR = db2.SelectRow();

                Date_TB.Value = DateTime.Parse(DR["bnko_date"].ToString());
                Value_TB.Text = DR["bnko_value"].ToString();
                Description_TB.Text = DR["bnko_description"].ToString();

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
                    log.Columns.Add(new Column("التاريـخ", Date_TB.Value.Value.Date.ToShortDateString()));
                    log.Columns.Add(new Column("القيـمــة", Value_TB.Text));
                    log.Columns.Add(new Column("البيـــان", Description_TB.Text));
                    log.CreateLog("البنك - المكتب", Bank_Id == null);

                    if ((bool)New.IsChecked)
                    {
                        Value_TB.Text = "";
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

                DB DataBase = new DB("bank_office");

                DataBase.AddColumn("bnko_date", Date_TB.Text);
                DataBase.AddColumn("bnko_value", Value_TB.Text);
                DataBase.AddColumn("bnko_description", Description_TB.Text);
                if (this.Bank_Id == null)
                {
                    return Confirm.Check(DataBase.Insert());
                }

// hena ye3ny hwa mawgod ba3mel edit
                else
                {
                    DataBase.AddCondition("bnko_id", this.Bank_Id);
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
