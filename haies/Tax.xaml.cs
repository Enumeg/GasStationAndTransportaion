using System;
using System.Data;
using System.Windows;
using Source;

namespace haies
{
    /// <summary>
    /// Interaction logic for outcome.xaml
    /// </summary>
    public partial class Tax : Window
    {
        object Tax_Id, Employee;
        Tax_Bouns Type;
        public Tax(Tax_Bouns type, object emp_Id, object tax_id = null)
        {
            InitializeComponent();        
            switch (type)
            { 
                case Tax_Bouns.Advance:
                    Title = "السلف";
                    break;
                case Tax_Bouns.Bouns:
                    Title = "المكافآت";
                    break;
                case Tax_Bouns.Due:
                    Title = "المستحقات";
                    break;
                case Tax_Bouns.Tax:
                    Title = "الحسومات";
                    break;
            }
            Tax_Id = tax_id;
            Employee = emp_Id;
            Type = type;
            Get_Reasons();
            Get_Tax();

        }

        private void Get_Tax()
        {
            try
            {
                DB db2 = new DB("tax");

                db2.SelectedColumns.Add("*");

                db2.AddCondition("tax_id", Tax_Id);

                DataRow DR = db2.SelectRow();

                Date_TB.Value = DateTime.Parse(DR["tax_date"].ToString());
                Value_TB.Text = DR["tax_value"].ToString();
                Reason_TB.Text = DR["tax_reason"].ToString();

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
                db.AddCondition("tax_type", Type);
                db.Fill(Reason_TB, "tax_reason", "tax_reason", "select tax_reason from tax group by tax_reason order by tax_reason");
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
                    if ((bool)New.IsChecked)
                    {
                        Value_TB.Text = "";
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

                DB DataBase = new DB("tax");

                DataBase.AddColumn("tax_emp_id", Employee);
                DataBase.AddColumn("tax_date", Date_TB.Value.Value.Date);
                DataBase.AddColumn("tax_value", Value_TB.Text);
                DataBase.AddColumn("tax_reason", Reason_TB.Text);



                if (this.Tax_Id == null)
                {
                    DataBase.AddColumn("tax_type", Type);

                    if (DataBase.IsNotExist("tax_id", "tax_emp_id", "tax_date", "tax_value", "tax_reason", "tax_type"))
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
                    DataBase.AddCondition("tax_id", Tax_Id);
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
