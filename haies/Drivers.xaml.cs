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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Source;


namespace haies
{
    /// <summary>
    /// Interaction logic for Drivers.xaml
    /// </summary>
    public partial class Drivers : Page
    {
        public Drivers()
        {
            InitializeComponent();

            Status_CB.ItemsSource = Enum.GetValues(typeof(Status));
            Status_CB.SelectedIndex = 1;
        }

        private void Fill_Drivers_LB()
        {

            try
            {
                DB db2 = new DB("person");

                // Only company drivers                
                db2.AddCondition("spr_per_id", 1);

                // search by name
                db2.AddCondition("per_name", "%" + Name_Search_TB.Text + "%", false, " like ");

                // search by mobile
                db2.AddCondition("per_mobile", "%" + Mobile_Search_TB.Text + "%", false, " like ");


                // search by Car Number
                //db2.AddCondition("car_number", "%" +Car_Search_TB.Text, false, " like ");
                db2.AddCondition("per_status", Status_CB.SelectedIndex);


                db2.Fill(LB, "per_id", "per_name", "select distinct(per_id), p.*,d.* from persons p join drivers d on per_id=dri_per_id left join cars on car_dri_id=dri_id");

            }
            catch
            {

            }


        }

        public static void Get_All_Drivers(ComboBox CB, bool Is_Company_Driver, string All = "")
        {

            try
            {
                DB db2 = new DB();
                // Only company drivers                
                db2.AddCondition("spr_per_id", 1, !Is_Company_Driver);
                db2.AddCondition("per_status", Status.فعال);

                db2.Fill(CB, "dri_id", "per_name", "select p.*,d.* from persons p join drivers d on per_id=dri_per_id order by per_name", All);
            }
            catch
            {

            }
        }

        public bool Add_Update_Person()
        {

            try
            {
                DB DataBase = new DB("persons");

                DataBase.AddColumn("per_name", Name_TB.Text.Trim());
                DataBase.AddColumn("per_address", Address_TB.Text.Trim());
                DataBase.AddColumn("per_mobile", Mobile_TB.Text.Trim());

                if (LB.SelectedIndex == -1)
                {
                    if (DataBase.IsNotExist("per_id", "per_name", "per_mobile"))
                    {
                        if (DataBase.Insert())
                        {
                            if (Add_Update_Driver(DataBase.Last_Inserted))
                            {
                                return true;
                            }

                            else
                            {
                                DB d = new DB("persons");
                                d.AddCondition("per_id", DataBase.Last_Inserted);
                                d.Delete();
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

                        Message.Show("هذا الاسم مسجل من فضلك اختار اسم اخر ", MessageBoxButton.OK, 5);
                        return false;
                    }


                }

// hena ye3ny hwa mawgod ba3mel edit
                else
                {
                    DataBase.AddCondition("per_id", LB.SelectedValue);
                    if (DataBase.Update())
                    {
                        return (Add_Update_Driver(LB.SelectedValue));
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch
            {
                //MessageBox.Show("kiki_method");
                return false;
            }
        }

        public bool Add_Update_Driver(object person_idd)
        {

            try
            {
                DB DB = new DB("drivers");

                DB.AddColumn("dri_per_id", person_idd);
                DB.AddColumn("spr_per_id", 1);
                DB.AddColumn("dri_salary", Salary_TB.Text.Trim());

                if (DB.IsNotExist("dri_id", "dri_per_id"))//5
                {
                    return Confirm.Check(DB.Insert());

                }
                else
                {
                    // 3alshan hwa mawgod 2abl keda
                    DB.AddCondition("dri_per_id", person_idd);
                    return Confirm.Check(DB.Update());
                }

            }
            catch
            {
                //MessageBox.Show("hena el error");
                return false;
            }
        }

        private void EditPanel_Add(object sender, EventArgs e)
        {
            try
            {
                App.Set_Style(Main_Grid, Operations.Add);
                Main_Grid.RowDefinitions[2].Height = new GridLength(35);
                LB.IsEnabled = false;
                LB.SelectedIndex = -1;
            }
            catch
            {

            }
        }

        private void EditPanel_Edit(object sender, EventArgs e)
        {
            try
            {
                if (LB.SelectedIndex != -1)
                {
                    App.Set_Style(Main_Grid, Operations.Edit);
                    Main_Grid.RowDefinitions[2].Height = new GridLength(35);
                    LB.IsEnabled = false;
                }
            }
            catch
            {

            }
        }

        private void EditPanel_Delete(object sender, EventArgs e)
        {
            try
            {
                if (LB.SelectedIndex != -1)
                {
                    if (Message.Show("هل تريد حذف هذا السائق", MessageBoxButton.YesNoCancel, 5) == MessageBoxResult.Yes)
                    {


                        DB db = new DB("persons");
                        db.AddCondition("per_id", LB.SelectedValue);
                        if (db.Delete())
                        {
                            var log = new Log();
                            log.Columns.Add(new Column("الإسم", Name_TB.Text));
                            log.Columns.Add(new Column("الجوال", Mobile_TB.Text));
                            log.CreateLog("السائقين");

                        Fill_Drivers_LB();

                        }

                    }
                }
            }
            catch
            {

            }
        }

        private void Name_Search_TB_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                Fill_Drivers_LB();
            }
            catch
            {

            }
        }

        private void Save_Save(object sender, EventArgs e)
        {
            try
            {
                if (Notify.validate("من فضلك ادخل الاسم", Name_TB.Text, MainWindow.GetWindow(this)))
                {
                    return;
                }

                if (Notify.validate("من فضلك ادخل الراتب", Salary_TB.Text, MainWindow.GetWindow(this)))
                {
                    return;
                }

                if (Add_Update_Person())
                {
                    var log = new Log();
                    log.Columns.Add(new Column("الإسم", Name_TB.Text));
                    log.Columns.Add(new Column("الجوال", Mobile_TB.Text));
                    log.CreateLog("السائقين", LB.SelectedIndex == -1);
                    App.Set_Style(Main_Grid, Operations.View);

                    Main_Grid.RowDefinitions[2].Height = new GridLength(0);

                    LB.IsEnabled = true;
                    int i = LB.SelectedIndex;
                    Fill_Drivers_LB();
                    LB.SelectedIndex = i;
                }
            }
            catch
            {

            }
        }

        private void Save_Cancel(object sender, EventArgs e)
        {
            try
            {
                App.Set_Style(Main_Grid, Operations.View);
                Main_Grid.RowDefinitions[2].Height = new GridLength(0);
                LB.IsEnabled = true;
            }
            catch
            {

            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (LB.SelectedIndex != -1)
                {
                    Places_prices p = new Places_prices(null, "");
                    p.ShowDialog();
                }
            }
            catch
            {

            }
        }
        private void Archive_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ChangeStatus_BTN.Content = Status_CB.SelectedIndex == 1 ? "أرشيف" : "تفعيل";
            ChangeStatus_BTN.Tag = Status_CB.SelectedIndex == 0 ? "/haies;component/Images/Activate.ico" : "/haies;component/Images/Archive.ico";

            Fill_Drivers_LB();
        }
        private void ChangeStatus_BTN_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var status = Status_CB.SelectedIndex == 0 ? 1 : 0;
                DB db = new DB("persons");
                db.AddColumn("per_status", status);
                db.AddCondition("per_id", LB.SelectedValue);
                Confirm.Check(db.Update());
                Fill_Drivers_LB();
            }
            catch
            {


            }
        }



    }
}
