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
    public partial class Salary : Window
    {

        public object Car_id;

        public object carOwner_payment = null, carOwner_Maintenance = null;

        public Salary(object car)
        {
            InitializeComponent();
            Car_id = car;
            Get_Car_Owner_Payment_Maintenance();

        }

        private void Get_Car_Owner_Payment_Maintenance()
        {
            try
            {
                DB db2 = new DB("car_owner");               

                db2.AddCondition("c.car_id", Car_id);

                DataRow dr = db2.SelectRow("select co.cor_payment,co.cor_maintenance from car_owner co join cars c on c.car_cor_id=co.cor_id");

                carOwner_payment = dr[0];

                carOwner_Maintenance = dr[1];

            }
            catch
            {


            }

        }

        private void add_update_outcome_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (carOwner_Maintenance.ToString() == "0")
                {
                    Add_Salary("راتب الميكانيكي");
                    Add_Salary("راتب العامل");
                }
                if (carOwner_payment.ToString() == "0")
                {
                    Add_Driver_Salary();
                }
            }
            catch
            {

            }
        }

        public bool Add_Driver_Salary()
        {
            try
            {
                object salary;
                DB DD = new DB();
                DD.AddCondition("c.car_id", Car_id);
                salary = DD.Select("select d.dri_salary from drivers d join cars c on c.car_dri_id=d.dri_id");

                DB DataBase = new DB("car_outcome");

                DataBase.AddColumn("cot_car_id", Car_id);
                DataBase.AddColumn("cot_date", Date_TB.Value.Value.Date);
                DataBase.AddColumn("cot_value", salary);
                DataBase.AddColumn("cot_type", "راتب السائق");

                if (DataBase.IsNotExist("cot_id", "cot_car_id", "cot_date", "cot_value", "cot_type"))
                {
                    if (DataBase.Insert())
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
                    // ye3ny hwa mawgood asln mesh ha3ml 7aga 
                    Message.Show("هذا المستند موجود من قبل", MessageBoxButton.OK, 5);
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        public bool Add_Salary(string type)
        {
            try
            {

                DB DataBase = new DB("car_outcome");
                DataBase.AddCondition("name", type);
                DataBase.AddColumn("cot_car_id", Car_id);
                DataBase.AddColumn("cot_date", Date_TB.Value.Value.Date);
                DataBase.AddColumn("cot_value", DataBase.Select("select salary from Salary "));
                DataBase.AddColumn("cot_type", type);

                if (DataBase.IsNotExist("cot_id", "cot_car_id", "cot_date", "cot_value", "cot_type"))
                {

                    return DataBase.Insert();
                }
                else
                {
                    // ye3ny hwa mawgood asln mesh ha3ml 7aga 
                    Message.Show("هذا المستند موجود من قبل", MessageBoxButton.OK, 5);
                    return false;
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
