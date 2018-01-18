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
    /// Interaction logic for newUser.xaml
    /// </summary>
    public partial class Add_User : Window
    {

        object User_Id;
        Edit_Mode EM;

        public Add_User(Edit_Mode e_m = Edit_Mode.Add, object id = null)
        {
            InitializeComponent();
            User_Id = id;
            fill_Types_combobox();

            EM = e_m;
        }

        private void fill_Types_combobox()
        {

            try
            {
                DB db2 = new DB("groups");

                db2.Fill(Type_CB, "grp_id", "grp_name", "select * from groups");
            }

            catch
            {

            }


        }


        private void Get_User()
        {
            try
            {
                DB db2 = new DB("users");

                db2.SelectedColumns.Add("*");

                db2.AddCondition("user_id", User_Id);

                DataRow DR = db2.SelectRow();

                Name_TB.Text = DR["user_name"].ToString();

                Type_CB.SelectedValue = int.Parse(DR["user_grp_id"].ToString());

            }
            catch
            {

            }
        }
        private void add_update_user_Click(object sender, RoutedEventArgs e)
        {

            try
            {


                if (Notify.validate("من فضلك ادخل الاسم", Name_TB.Text, this))
                {
                    return;
                }
                if (EM == Edit_Mode.Add)
                {
                    if (Notify.validate("من فضلك ادخل كلمه السر", Password_TB.Text, this))
                    {
                        return;
                    }


                    if (Notify.validate("من فضلك ادخل كلمه السر الثانيه", RePassword_TB.Text, this))
                    {
                        return;
                    }
                }
                if (Notify.validate("من فضلك اختر نوع المستخدم", Type_CB.SelectedIndex, this))
                {
                    return;
                }

                if (Add_Update())
                {
                    var log = new Log();
                    log.Columns.Add(new Column("الإسم", Name_TB.Text));
                    log.Columns.Add(new Column("المجموعة", Type_CB.Text));
                    log.CreateLog("المستخدمين", User_Id == null);
                    if ((bool)New.IsChecked)
                    {

                    }
                    else
                    {                                                                                                                   
                        this.Close();
                    }
                }


            }
            catch
            {
                // MessageBox.Show("kiki");
                return;
            }
        }
        public bool Add_Update()
        {

            try
            {

                DB DataBase = new DB("users");


                if (Password_TB.Text.Equals(RePassword_TB.Text))
                {

                    if (this.User_Id == null)
                    {
                        DataBase.AddColumn("user_name", Name_TB.Text);
                        DataBase.AddColumn("user_pass", Password_TB.Text.Trim().GetHashCode());
                        DataBase.AddColumn("user_grp_id", Type_CB.SelectedValue);

                        if (DataBase.IsNotExist("user_id", "user_name"))
                        {
                            return Confirm.Check(DataBase.Insert());
                        }
                        else
                        {
                            // ye3ny hwa mawgood asln mesh ha3ml 7aga 
                            Message.Show("هذا الاسم مستخدم من قبل", MessageBoxButton.OK, 5);
                            return false;
                            //DataBase.AddCondition("pl_id", this.placeId);
                            //DataBase.Update();

                        }


                    }




        // hena ye3ny hwa mawgod ba3mel edit

                    else
                    {
                        switch (EM)
                        {
                            case Edit_Mode.Edit:
                                DataBase.AddColumn("user_name", Name_TB.Text);
                                DataBase.AddColumn("user_grp_id", Type_CB.SelectedValue);
                                break;
                            case Edit_Mode.Change_Password:
                                DataBase.AddColumn("user_pass", Password_TB.Text.Trim().GetHashCode());
                                break;
                        }

                        DataBase.AddCondition("user_id", this.User_Id);
                        return Confirm.Check(DataBase.Update());

                    }


                }
                else
                {
                    Message.Show("كلمة المرور غير متطابقة", MessageBoxButton.OK, 10);
                    return false;
                }


            }
            catch
            {
                //MessageBox.Show("kiki_method");
                return false;
            }

        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            switch (EM)
            {
                case Edit_Mode.Edit:
                    gridy.RowDefinitions[1].Height = gridy.RowDefinitions[2].Height = new GridLength(0);
                    New.Visibility = System.Windows.Visibility.Collapsed;
                    break;
                case Edit_Mode.Change_Password:
                    New.Visibility = System.Windows.Visibility.Collapsed;
                    gridy.RowDefinitions[0].Height = gridy.RowDefinitions[3].Height = new GridLength(0);
                    break;
            }
            Get_User();
        }


    }
}
