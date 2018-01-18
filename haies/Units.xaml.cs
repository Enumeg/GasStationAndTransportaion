using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using Source;

namespace haies
{
    /// <summary>
    /// Interaction logic for units.xaml
    /// </summary>
    public partial class Units : Window
    {
        object Unit_Id = null;

        public Units()
        {
            InitializeComponent();
            Fill_Units_LB();
        }

        private void Fill_Units_LB()
        {

            DB db2 = new DB("Units");

            // search by name
            db2.AddCondition("unit_name", "%" + Unit_TB.Text + "%", false, " like ");

            db2.Fill(LB, "unit_id", "unit_name", "select * from units");


        }

        public static void Get_All_Units(ComboBox CB, string All = "")
        {
            try
            {
                DB db2 = new DB("Units");

                db2.Fill(CB, "unit_id", "unit_name", "select * from units", All);
            }

            catch
            {

            }
        }

        private bool Add_Update()
        {
            try
            {

                DB DataBase = new DB("units");
                DataBase.AddColumn("unit_id", int.Parse(DataBase.Select("select Max(unit_id) from units").ToString()) + 1);
                DataBase.AddColumn("unit_name", Unit_TB.Text);

                if (this.Unit_Id == null)
                {
                    if (DataBase.IsNotExist("unit_id", "unit_name"))
                    {
                        return Confirm.Check(DataBase.Insert());
                    }
                    else
                    {
                        Message.Show("لقد تم تسجيل هذه الوحدة من قبل", MessageBoxButton.OK, 5);
                        return false;
                    }


                }
                else
                {
                    DataBase.AddCondition("unit_id", this.Unit_Id);
                    return Confirm.Check(DataBase.Update());
                }
            }
            catch
            {
                //MessageBox.Show("kiki_method");
                return false;
            }
        }

        private void add_update_outcome_Click(object sender, RoutedEventArgs e)
        {
            try
            {


                if (Notify.validate("من فضلك ادخل الوحدة", Unit_TB.Text, this))
                {
                    return;
                }


                if (Add_Update())
                {
                    var log = new Log();
                    log.Columns.Add(new Column("الإسم", Unit_TB.Text));
                    log.CreateLog("الوحدات", Unit_Id == null);
                    Unit_Id = null;
                    Main_GD.RowDefinitions[1].Height = new GridLength(0);
                    Unit_TB.Text = "";
                    Fill_Units_LB();
                }
            }
            catch
            {
                return;
            }
        }

        private void EP_Add(object sender, EventArgs e)
        {
            try
            {
                Unit_Id = null;
                Main_GD.RowDefinitions[1].Height = new GridLength(35);
            }
            catch
            {

            }
        }

        private void EP_Edit(object sender, EventArgs e)
        {
            try
            {
                if (LB.SelectedIndex != -1)
                {
                    Unit_Id = LB.SelectedValue;
                    Unit_TB.Text = ((DataRowView)LB.SelectedItem)[1].ToString();
                    Main_GD.RowDefinitions[1].Height = new GridLength(35);
                }
            }
            catch
            {

            }
        }

        private void EP_Delete(object sender, EventArgs e)
        {
            try
            {
                if (LB.SelectedIndex != -1)
                {
                    if (Message.Show("هل تريد حذف هذه الوحدة", MessageBoxButton.YesNoCancel, 10) == MessageBoxResult.Yes)
                    {
                        Unit_Id = LB.SelectedValue;
                        DB db = new DB("units");
                        db.AddCondition("unit_id", Unit_Id);
                        if (db.Delete())
                        {
                            var log = new Log();
                            log.Columns.Add(new Column("الإسم", Unit_TB.Text));
                            log.CreateLog("الوحدات");

                            Fill_Units_LB();
                        }
                    }
                }
            }
            catch
            {

            }
        }
    }
}
