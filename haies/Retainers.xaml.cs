using System;
using System.Windows;
using System.Windows.Controls;
using Source;
using System.Data;


namespace haies
{
    /// <summary>
    /// Interaction logic for Retainers.xaml
    /// </summary>
    public partial class Retainers : Window
    {
        object Ret_Id;
        public static object Sponser_Id;
        public Retainers()
        {
            InitializeComponent();
            Fill_Retainers_LB();
        }

        private void Fill_Retainers_LB()
        {
            try
            {
                DB db = new DB();
                db.AddCondition("per_name", Name_TB.Text.Trim(), false, " like ");
                db.AddCondition("per_mobile", Mobile_TB.Text.Trim(), false, " like ");
                db.Fill(LB, "per_id", "per_name", "select persons.* from persons join drivers on per_id= spr_per_id group by per_id order by per_name");
            }
            catch
            {

            }
        }

        public static void Get_All_Retainers(ComboBox CB, string All = "")
        {
            try
            {
                DB db = new DB();
                db.Fill(CB, "per_id", "per_name", "select persons.* from persons join drivers on per_id= spr_per_id group by per_id order by per_name", All);
            }
            catch
            {

            }
        }

        private bool Add_Update()
        {
            try
            {

                DB DataBase = new DB("persons");

                DataBase.AddColumn("per_name", Name_TB.Text.Trim());
                DataBase.AddColumn("per_mobile", Mobile_TB.Text.Trim());

                if (Ret_Id == null)
                {
                    if (DataBase.IsNotExist("per_id", "per_name", "per_mobile"))
                    {
                        if (DataBase.Insert())
                        {
                            return Update_Sponser(DataBase.Last_Inserted);
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        Message.Show("لقد تم تسجيل هذا الكفيل من قبل", MessageBoxButton.OK, 5);
                        return false;
                    }


                }
                else
                {
                    DataBase.AddCondition("per_id", this.Ret_Id);
                    return (DataBase.Update());
                }
            }
            catch
            {
                return false;
            }
        }

        private bool Update_Sponser(object sponser_Id)
        {
            try
            {
                DB DB = new DB("drivers");
                DB.AddColumn("spr_per_id", sponser_Id);
                DB.AddCondition("dri_id", 1);
                return (DB.Update());
            }

            catch
            {
                return false;
            }
        }


        private void Save_Save(object sender, EventArgs e)
        {
            try
            {

                if (Confirm.Check(Add_Update()))
                {
                    var log = new Log();
                    log.Columns.Add(new Column("الإسم", Name_TB.Text));
                    log.Columns.Add(new Column("الجوال", Mobile_TB.Text));
                    log.CreateLog("الكفلاء", Ret_Id == null);

                    if (!(bool)New.IsChecked)
                    {
                        Main_GD.RowDefinitions[2].Height = new GridLength(0);
                    }
                    Fill_Retainers_LB();
                    Ret_Id = null;
                    Name_TB.Text = Mobile_TB.Text = "";
                }
            }
            catch
            {
                return;
            }
        }

        private void Save_Cancel(object sender, EventArgs e)
        {
            try
            {
                Main_GD.RowDefinitions[2].Height = new GridLength(0);
                Fill_Retainers_LB();
                Ret_Id = null;
                Name_TB.Text = Mobile_TB.Text = "";
            }
            catch
            {

            }
        }

        private void EditPanel_Add(object sender, EventArgs e)
        {
            try
            {
                Ret_Id = null;
                Main_GD.RowDefinitions[2].Height = new GridLength(35);
            }
            catch
            {

            }
        }

        private void EditPanel_Edit(object sender, EventArgs e)
        {
            try
            {
                Main_GD.RowDefinitions[2].Height = new GridLength(35);
                Ret_Id = LB.SelectedValue;
                Name_TB.Text = ((DataRowView)LB.SelectedItem)["per_name"].ToString();
                Mobile_TB.Text = ((DataRowView)LB.SelectedItem)["per_mobile"].ToString();

            }
            catch
            {

            }
        }

        private void EditPanel_Delete(object sender, EventArgs e)
        {

            try
            {
                if (Message.Show("هل تريد حذف هذا الكفيل", MessageBoxButton.YesNoCancel, 10) == MessageBoxResult.Yes)
                {
                    Ret_Id = LB.SelectedValue;
                    DB db = new DB("persons");
                    db.AddCondition("per_id", Ret_Id);
                    if (db.Delete())
                    {
                        var log = new Log();
                        log.Columns.Add(new Column("الإسم", Name_TB.Text));
                        log.Columns.Add(new Column("الجوال", Mobile_TB.Text));
                        log.CreateLog("الكفلاء");
                        Fill_Retainers_LB();
                    }

                }
            }
            catch
            {

            }
        }

        private void Group_TB_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (Ret_Id == null)
                {
                    Fill_Retainers_LB();
                }
            }
            catch
            {

            }
        }

        private void LB_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                Sponser_Id = LB.SelectedValue;
                this.Close();
            }
            catch
            {

            }
        }
    }
}
