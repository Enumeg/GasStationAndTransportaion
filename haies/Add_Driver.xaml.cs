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
    /// Interaction logic for driver_customer.xaml
    /// </summary>
    public partial class Add_Driver : Window
    {
        object Driver_Id;

        public Add_Driver(object driver_Id = null)
        {
            InitializeComponent();
            Driver_Id = driver_Id;
            Retainers.Get_All_Retainers(Sponser_CB, "الكفلاء");
            Get_Driver();
        }


        private void Get_Driver()
        {
            try
            {
                DB db = new DB();
                db.AddCondition("dri_id", Driver_Id);
                DataRow dr = db.SelectRow(@"select p1.*,c.car_number,d.spr_per_id from drivers d
                                            join persons p1 on p1.per_id= d.dri_per_id                                            
                                            left join cars c on car_dri_id = dri_id ");
                Driver_Name_TB.Tag = dr[0];
                Driver_Name_TB.Text = dr[1].ToString();
                Driver_Address_TB.Text = dr[2].ToString();
                Driver_Mobile_TB.Text = dr[3].ToString();
                Car_Number.Text = dr[4].ToString();
                Sponser_CB.SelectedValue = dr[5];
            }
            catch
            {

            }
        }

        private bool Add_Update()
        {
            object Sponser_ID = Sponser_CB.SelectedValue, Driver_ID = null;
            try
            {
                if (Driver_Id == null)
                {
                    Driver_ID = Add_Update_Person(Driver_Name_TB.Text, Driver_Address_TB.Text, Driver_Mobile_TB.Text);
                    if (Driver_ID != null)
                    {
                        if (Add_Update_Driver(Sponser_ID, Driver_ID, Operations.Add))
                        {
                            return true;
                        }

                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }

                }
                else
                {
                    return (Add_Update_Person(Driver_Name_TB.Text, Driver_Address_TB.Text, Driver_Mobile_TB.Text, Driver_Name_TB.Tag) != null &&
                       Add_Update_Driver(Sponser_ID, Driver_Id, Operations.Edit)
                     && Add_Update_Cars(Driver_Id));
                }
            }
            catch
            {
                return false;
            }
        }

        private bool Add_Update_Driver(object sponser_Id, object driver_Id, Operations operation)
        {
            try
            {
                DB DB = new DB("drivers");
                DB.AddColumn("spr_per_id", sponser_Id);
                if (operation == Operations.Add)
                {
                    DB.AddColumn("dri_per_id", driver_Id);
                    DB.AddColumn("dri_salary", 0);
                    if (DB.Insert())
                    {
                        return Add_Update_Cars(DB.Last_Inserted);
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    DB.AddCondition("dri_id", driver_Id);
                    return (DB.Update());
                }
            }
            catch
            {
                //MessageBox.Show("hena el error");
                return false;
            }
        }

        private object Add_Update_Person(string Name, string Address, string Mobile, object per_id = null)
        {
            object Per_Id = null;
            try
            {
                DB DataBase = new DB("persons");
                DataBase.AddColumn("per_name", Name.Trim());
                DataBase.AddColumn("per_address", Address.Trim());
                DataBase.AddColumn("per_mobile", Mobile.Trim());
                if (per_id == null)
                {
                    if (DataBase.IsNotExist("per_id", "per_name"))
                    {
                        if (DataBase.Insert())
                        {
                            Per_Id = DataBase.Last_Inserted;
                        }
                    }
                    else
                    {
                        DB d = new DB();
                        d.AddCondition("per_name", Name.Trim());
                        Per_Id = d.Select("select per_id from persons ");
                    }
                }
                else
                {
                    DataBase.AddCondition("per_id", per_id);
                    if (DataBase.Update())
                    {
                        Per_Id = per_id;
                    }
                }

            }
            catch
            {

            }
            return Per_Id;
        }

        private bool Add_Update_Cars(object driver_Id)
        {
            try
            {
                if (Car_Number.Text != "")
                {
                    DB db = new DB("cars");
                    db.AddColumn("car_number", Car_Number.Text.Trim());
                    db.AddColumn("car_name", Car_Number.Text.Trim());
                    db.AddColumn("car_dri_id", driver_Id);
                    if (db.IsNotExist("car_id", "car_number"))
                    {
                        return (db.Insert());
                    }
                    else
                    {
                        db.AddCondition("car_number", Car_Number.Text.Trim());
                        return db.Update();
                    }
                }
                else
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        private void add_update_outcome_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Notify.validate("من فضلك ادخل الاسم", Driver_Name_TB.Text, this))
                {
                    return;
                }



                if (Confirm.Check(Add_Update()))
                {
                    var log = new Log();
                    log.Columns.Add(new Column("الإسم", Driver_Name_TB.Text));
                    log.CreateLog("السائقين", Driver_Id == null);
                    Close();
                }

            }
            catch
            {

            }
        }

        private void Sponser_CB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (Sponser_CB.SelectedIndex == 0)
                {
                    Retainers r = new Retainers();
                    r.ShowDialog();
                    Retainers.Get_All_Retainers(Sponser_CB, "");
                    Sponser_CB.SelectedValue = Retainers.Sponser_Id;

                }
            }
            catch
            {

            }
        }

    }
}
