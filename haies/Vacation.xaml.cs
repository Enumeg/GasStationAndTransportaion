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

namespace haies
{
    /// <summary>
    /// Interaction logic for Vacation.xaml
    /// </summary>
    public partial class Vacation : Window
    {
        object Vac_Id, Employee;

        public Vacation(object emp_Id, object vac_id = null)
        {
            InitializeComponent();
            Vac_Id = vac_id;
            Employee = emp_Id;
            Get_Reasons();
            Get_Vacation();

        }

        private void Get_Vacation()
        {
            try
            {
                DB db2 = new DB("vacation");

                db2.SelectedColumns.Add("*");

                db2.AddCondition("vac_id", Vac_Id);

                DataRow DR = db2.SelectRow();

                From_DTP.Value = DateTime.Parse(DR["vac_from"].ToString());
                To_DTP.Value = DateTime.Parse(DR["vac_to"].ToString());
                Reason_TB.Text = DR["vac_reason"].ToString();

            }
            catch
            {


            }
        }

        private void Get_Reasons()
        {
            try
            {
                DB db = new DB();
                db.Fill(Reason_TB, "vac_reason", "vac_reason", "select vac_reason from vacation group by vac_reason order by vac_reason");
            }
            catch
            {

            }
        }

        private void add_update_outcome_Click(object sender, RoutedEventArgs e)
        {
            try
            {


                if (Notify.validate("من فضلك ادخل التاريخ البداية", From_DTP.Text, this))
                {
                    return;
                }

                if (Notify.validate("من فضلك ادخل التاريخ النهاية", To_DTP.Text, this))
                {
                    return;
                }
                if (From_DTP.Value.Value.Date > To_DTP.Value.Value.Date)
                {
                    Notify.Show("التاريخ النهاية أقل من تاريخ البداية", this);
                    return;
                }

                if (Add_Update())
                {
                    var log = new Log();
                    log.Columns.Add(new Column("من", From_DTP.Text));
                    log.Columns.Add(new Column("إلى", To_DTP.Text));
                    log.Columns.Add(new Column("السبب", Reason_TB.Text));
                    log.CreateLog("الأجازات", Vac_Id == null);
                    if ((bool)New.IsChecked)
                    {
                        Get_Reasons();
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

                DB DataBase = new DB("vacation");

                DataBase.AddColumn("vac_emp_id", Employee);
                DataBase.AddColumn("vac_from", From_DTP.Value.Value.Date);
                DataBase.AddColumn("vac_to", To_DTP.Value.Value.Date);
                DataBase.AddColumn("vac_reason", Reason_TB.Text);



                if (this.Vac_Id == null)
                {
                    if (DataBase.IsNotExist("vac_id", "vac_emp_id", "vac_from", "vac_to"))
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
                    DataBase.AddCondition("vac_id", Vac_Id);
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
