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
    public partial class Factory_Installments : Window
    {
        object Factory_Id;
        object Sup_Id;

        public Factory_Installments(object sup_id, object fac_id = null)
        {
            InitializeComponent();
            Sup_Id = sup_id;
            Factory_Id = fac_id;
            Get_Factory();

        }

        private void Get_Factory()
        {
            try
            {
                DB db = new DB("factory_installment");

                db.SelectedColumns.Add("*");

                db.AddCondition("fac_id", Factory_Id);              
                DataRow DR = db.SelectRow();

                Date_TB.Value = DateTime.Parse(DR["fac_date"].ToString());
                Value_TB.Text = DR["fac_value"].ToString();
                Description_TB.Text = DR["fac_description"].ToString();

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
                    log.Columns.Add(new Column("التاريـخ", Date_TB.Value.Value.ToShortDateString()));
                    log.Columns.Add(new Column("القيـمــة", Value_TB.Text));
                    log.Columns.Add(new Column("البيـــان", Description_TB.Text));

                    log.CreateLog("أقساط المصنع", this.Factory_Id == null);
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

                DB DataBase = new DB("factory_installment");

                DataBase.AddColumn("fac_date", Date_TB.Text);
                DataBase.AddColumn("fac_value", Value_TB.Text);
                DataBase.AddColumn("fac_description", Description_TB.Text);




                if (this.Factory_Id == null)
                {
                    DataBase.AddColumn("fac_sup_id", Sup_Id);
                    if (DataBase.IsNotExist("fac_id", "fac_description", "fac_date"))
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
                    DataBase.AddCondition("fac_id", this.Factory_Id);
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
