using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Source;
namespace haies
{
    /// <summary>
    /// Interaction logic for Product.xaml
    /// </summary>

    public partial class Employees : Page
    {

        DataTable Emps;

        public object stay, passport, person;

        public Employees()
        {
            InitializeComponent();
            Fill_Employees_LB();
            Emps = new DataTable();
        }
        public static void Get_All_Employees(ComboBox CB, string all = "")
        {

            try
            {
                DB db2 = new DB("persons");
                db2.AddCondition("per_status", Status.فعال);


                db2.Fill(CB, "emp_id", "per_name", "select p.*,c.* from persons p join employees c on c.emp_per_id=p.per_id order by per_name", all);

            }

            catch
            {

            }
        }


        private void Fill_Employees_LB()
        {
            try
            {
                DB db2 = new DB("persons");

                // search by name
                db2.AddCondition("per_name", "%" + Name_Search_TB.Text + "%", false, " like ", "per_name");

                // search by passport
                db2.AddCondition("p2.pap_number", "%" + Passport_Search_TB.Text + "%", false, " like ");


                // search by stay
                db2.AddCondition("p1.pap_number", "%" + Stay_Search_TB.Text + "%", false, " like ");

                db2.Fill(LB, "emp_id", "name", @"select emp.*,p1.pap_id stay_id,p2.pap_id passport,per.per_id person_id,
                              p1.pap_number stay_number,p1.pap_start stay_start,p1.pap_end stay_end,p2.pap_number passport_number,p2.pap_start pass_start,p2.pap_end pass_end, 
                              per.per_name name,per.per_address,per.per_mobile
                              from employees emp join paper p1 on p1.pap_id=emp.emp_stay_id
                              join paper p2 on p2.pap_id=emp.emp_passport
                              join persons per on per_id=emp_per_id order by per_name");

            }
            catch
            {

            }
        }

        private void GAge()
        {
            List<string> sb = new List<string>();
            try
            {
                DateTime ag = new DateTime(DateTime.Now.Date.Subtract(EnrollmentDate_TB.Value.Value.Date).Ticks);
                ag = ag.AddMonths(-1);
                ag = ag.AddYears(-1);
                Service.Text = ag.Month == 12 ? ag.AddYears(1).ToString("yy") : ag.ToString("MM - yy");
            }
            catch
            {

            }
        }

        private DateTime Get_Hijri()
        {

            System.Globalization.CultureInfo info = new System.Globalization.CultureInfo("ar-sa");
            string s = DateTime.Now.ToString("yyyy/MM/dd", info.DateTimeFormat);
            DateTime d = new DateTime(int.Parse(s.Split('/')[0]), int.Parse(s.Split('/')[1]), int.Parse(s.Split('/')[2]));

            return d;
        }

        #region Add_Update_Methods

        private bool Add_Update_Stay_Paper()
        {
            try
            {

                DB DataBase = new DB("paper");

                DataBase.AddColumn("pap_number", StayNumber_TB.Text);
                DataBase.AddColumn("pap_start", Stay_Start_TB.Text);
                DataBase.AddColumn("pap_end", Stay_End_TB.Text);

                if (LB.SelectedIndex == -1)
                {
                    if (DataBase.IsNotExist("pap_id", "pap_number", "pap_start", "pap_end"))
                    {
                        if (DataBase.Insert())
                        {
                            stay = DataBase.Last_Inserted;

                            if (Add_Update_Passport_Paper())
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
                        // Notify.Show("هذا الاسم مسجل من فضلك اختار اسم اخر ", this);

                        return false;
                    }

                }

// hena ye3ny hwa mawgod ba3mel edit

                else
                {

                    stay = ((DataRowView)LB.SelectedItem)["stay_id"];

                    DataBase.AddCondition("pap_id", stay);

                    if (DataBase.Update())
                    {
                        if (Add_Update_Passport_Paper())
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





            }
            catch
            {
                return false;
            }

        }

        private bool Add_Update_Passport_Paper()
        {
            try
            {

                DB DataBase = new DB("paper");

                DataBase.AddColumn("pap_number", PassportNumber_TB.Text);
                DataBase.AddColumn("pap_start", Passport_Start_TB.Text);
                DataBase.AddColumn("pap_end", Passport_End_TB.Text);

                if (LB.SelectedIndex == -1)
                {
                    if (DataBase.IsNotExist("pap_id", "pap_number", "pap_start", "pap_end") == false)
                    {
                        if (DataBase.Insert())
                        {
                            passport = DataBase.Last_Inserted;
                            if (Add_Update_Person())
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
                        // Notify.Show("هذا الاسم مسجل من فضلك اختار اسم اخر ", this);

                        return false;
                    }

                }

// hena ye3ny hwa mawgod ba3mel edit

                else
                {

                    passport = ((DataRowView)LB.SelectedItem)["passport"];

                    DataBase.AddCondition("pap_id", passport);

                    if (DataBase.Update())
                    {
                        if (Add_Update_Person())
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






            }
            catch
            {
                return false;
            }

        }

        private bool Add_Update_Person()
        {
            try
            {

                DB DataBase = new DB("persons");

                DataBase.AddColumn("per_name", Name_TB.Text);
                DataBase.AddColumn("per_address", Address_TB.Text);
                DataBase.AddColumn("per_mobile", Mobile_TB.Text);

                if (LB.SelectedIndex == -1)
                {
                    if (DataBase.IsNotExist("per_id", "per_name", "per_address", "per_mobile") == false)
                    {
                        if (DataBase.Insert())
                        {
                            person = DataBase.Last_Inserted;
                            if (Add_Update_Employee())
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
                        // Notify.Show("هذا الاسم مسجل من فضلك اختار اسم اخر ", this);
                        DataBase.AddCondition("per_name", Name_TB.Text);
                        DataBase.AddCondition("per_address", Address_TB.Text);
                        DataBase.AddCondition("per_mobile", Mobile_TB.Text);
                        DataBase.SelectedColumns.Add("per_id");
                        person = DataBase.Select();
                        if (Add_Update_Employee())
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }

                }

// hena ye3ny hwa mawgod ba3mel edit

                else
                {

                    person = ((DataRowView)LB.SelectedItem)["person_id"];

                    DataBase.AddCondition("per_id", person);

                    if (DataBase.Update())
                    {
                        if (Add_Update_Employee())
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



            }
            catch
            {
                return false;
            }

        }

        private bool Add_Update_Employee()
        {
            try
            {

                DB DataBase = new DB("employees");

                DataBase.AddColumn("emp_per_id", person);
                DataBase.AddColumn("emp_stay_id", stay);
                DataBase.AddColumn("emp_passport", passport);
                DataBase.AddColumn("emp_nationality", Nationality_TB.Text);
                DataBase.AddColumn("emp_salary", Salary_TB.Text);
                DataBase.AddColumn("emp_enrollmentDate", EnrollmentDate_TB.Value.Value.Date);
                DataBase.AddColumn("emp_job", Job_TB.Text);

                if (LB.SelectedIndex == -1)
                {
                    if (DataBase.IsNotExist("emp_id", "emp_per_id", "emp_stay_id", "emp_passport", "emp_salary", "emp_nationality", "emp_enrollmentDate", "emp_job") == false)
                    {
                        if (DataBase.Insert())
                        {
                            if (Job_TB.Text == "سائق")
                            {
                                if (Add_Update_Driver())
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
                                return true;
                            }
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

// hena ye3ny hwa mawgod ba3mel edit

                else
                {

                    DataBase.AddCondition("emp_id", LB.SelectedValue);

                    if (DataBase.Update())
                    {
                        if (Job_TB.Text == "سائق")
                        {
                            if (Add_Update_Driver())
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
                            return true;
                        }
                    }
                    else
                    {
                        return false;
                    }

                }



            }
            catch
            {
                return false;
            }

        }

        private bool Add_Update_Driver()
        {
            try
            {
                DB DataBase = new DB("drivers");

                DataBase.AddColumn("dri_per_id", person);
                DataBase.AddColumn("spr_per_id", 1);
                DataBase.AddColumn("dri_salary", Salary_TB.Text);

                if (LB.SelectedIndex == -1)
                {
                    if (DataBase.IsNotExist("dri_id", "dri_per_id", "dri_salary") == false)
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

                        return true;
                    }

                }

// hena ye3ny hwa mawgod ba3mel edit

                else
                {

                    DataBase.AddCondition("dri_per_id", LB.SelectedValue);

                    if (DataBase.Update())
                    {

                        return true;

                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch
            {
                return false;
            }

        }

        #endregion

        #region Edit_Panel
        private void EditPanel_Add(object sender, EventArgs e)
        {
            try
            {
                App.Set_Style(Main_Grid, Operations.Add);
                Save_Panel.Visibility = System.Windows.Visibility.Visible;
                LB.IsEnabled = false;
                LB.SelectedIndex = -1;
                Stay_Start_TB.Value = Stay_End_TB.Value = Get_Hijri();
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
                    Save_Panel.Visibility = System.Windows.Visibility.Visible;
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
                    if (Message.Show("هل تريد حذف هذا الموظف", MessageBoxButton.YesNoCancel, 5) == MessageBoxResult.Yes)
                    {
                        DB db = new DB("person");

                        db.AddCondition("per_id", LB.SelectedValue);
                        if (db.Delete())
                        {
                            var log = new Log();
                            log.Columns.Add(new Column("الإسم", Name_TB.Text));
                            log.Columns.Add(new Column("الجوال", Mobile_TB.Text));
                            log.CreateLog("الموظفين");

                            Fill_Employees_LB();
                        }

                    }
                }
            }
            catch
            {

            }
        }

        #endregion

        #region Save_Panel
        private void Save_Panel_Save(object sender, EventArgs e)
        {
            try
            {

                if (Notify.validate("من فضلك ادخل الاسم", Name_TB.Text, MainWindow.GetWindow(this)))
                {
                    return;
                }

                if (Notify.validate("من فضلك ادخل الجنسيه", Nationality_TB.Text, MainWindow.GetWindow(this)))
                {
                    return;
                }


                if (Notify.validate("من فضلك ادخل الوظيفه", Job_TB.Text, MainWindow.GetWindow(this)))
                {
                    return;
                }

                if (Notify.validate("من فضلك ادخل المرتب", Salary_TB.Text, MainWindow.GetWindow(this)))
                {
                    return;
                }



                if (Notify.validate("من فضلك ادخل تاريخ التعيين", EnrollmentDate_TB.Text, MainWindow.GetWindow(this)))
                {
                    return;
                }


                if (Notify.validate("من فضلك ادخل رقم جواز السفر", PassportNumber_TB.Text, MainWindow.GetWindow(this)))
                {
                    return;
                }

                if (Notify.validate("من فضلك ادخل رقم الاقامه", StayNumber_TB.Text, MainWindow.GetWindow(this)))
                {
                    return;
                }



                if (Notify.validate("من فضلك ادخل تاريخ بدايه جواز السفر", Passport_Start_TB.Text, MainWindow.GetWindow(this)))
                {
                    return;
                }

                if (Notify.validate("من فضلك ادخل تاريخ نهايه جواز السفر", Passport_End_TB.Text, MainWindow.GetWindow(this)))
                {
                    return;
                }


                if (Notify.validate("من فضلك ادخل تاريخ بدايه الاقامه", Stay_Start_TB.Text, MainWindow.GetWindow(this)))
                {
                    return;
                }

                if (Notify.validate("من فضلك ادخل تاريخ نهايه الاقامه", Stay_End_TB.Text, MainWindow.GetWindow(this)))
                {
                    return;
                }


                if (Confirm.Check(Add_Update_Stay_Paper()))
                {
                    var log = new Log();
                    log.Columns.Add(new Column("الإسم", Name_TB.Text));
                    log.Columns.Add(new Column("الجوال", Mobile_TB.Text));
                    log.CreateLog("الموظفين", LB.SelectedIndex == -1);
                    App.Set_Style(Main_Grid, Operations.View);
                    Save_Panel.Visibility = System.Windows.Visibility.Collapsed;
                    LB.IsEnabled = true;
                    int i = LB.SelectedIndex;
                    Fill_Employees_LB();
                    LB.SelectedIndex = i;
                }
            }
            catch
            {

            }
        }

        private void Save_Panel_Cancel(object sender, EventArgs e)
        {
            try
            {
                App.Set_Style(Main_Grid, Operations.View);
                Save_Panel.Visibility = System.Windows.Visibility.Collapsed;
                LB.IsEnabled = true;
            }
            catch
            {

            }
        }

        #endregion

        #region Vacation
        private void Fill_Vacation()
        {
            try
            {
                DB db = new DB();
                db.AddCondition("emp_id", LB.SelectedValue);
                db.AddCondition("SD", From_DTP.Value.Value.Date);
                db.AddCondition("ED", To_DTP.Value.Value.Date);
                Vacation_DG.ItemsSource = db.SelectTableView("select * from vacation where vac_emp_id=@emp_id and ((vac_from>=@SD and vac_from<=@ED) or (vac_to>=@SD and vac_to<=@ED))");

            }
            catch
            {

            }
        }

        private void Vacation_EP_Add(object sender, EventArgs e)
        {
            try
            {
                Vacation vac = new Vacation(LB.SelectedValue);
                vac.ShowDialog();
                Fill_Vacation();
            }
            catch
            {

            }
        }

        private void Vacation_EP_Edit(object sender, EventArgs e)
        {
            try
            {
                Vacation vac = new Vacation(LB.SelectedValue, ((DataRowView)Vacation_DG.SelectedItem)["vac_id"]);
                vac.ShowDialog();
                Fill_Vacation();
            }
            catch
            {

            }
        }

        private void Vacation_EP_Delete(object sender, EventArgs e)
        {
            try
            {
                if (Message.Show("هل تريد الحذف", MessageBoxButton.YesNoCancel, 10) == MessageBoxResult.Yes)
                {
                    DB db = new DB("vacation");
                    var row = ((DataRowView)Vacation_DG.SelectedItem);

                    db.AddCondition("vac_id", row["vac_id"]);
                    if (db.Delete())
                    {
                        var log = new Log();
                        log.Columns.Add(new Column("من", row["vac_from"]));
                        log.Columns.Add(new Column("إلى", row["vac_to"]));
                        log.Columns.Add(new Column("السبب", row["vac_reason"]));
                        log.Columns.Add(new Column("إسم الموظف", ((DataRowView)LB.SelectedItem)["per_name"]));
                        log.CreateLog("الأجازات");
                        Fill_Vacation();
                    }
                }
            }
            catch
            {

            }
        }

        #endregion

        #region Filters

        private void Name_Search_TB_TextChanged(object sender, TextChangedEventArgs e)
        {
            Fill_Employees_LB();
        }

        private void LB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                GAge();
                if (this.IsLoaded)
                {
                    Fill_Vacation();
                }
            }
            catch
            {

            }
        }

        private void From_DTP_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            try
            {
                if (this.IsLoaded)
                {
                    Fill_Vacation();
                }
            }
            catch
            {

            }
        }

        #endregion

    }
}
