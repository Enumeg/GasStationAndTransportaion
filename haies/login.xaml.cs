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
    /// Interaction logic for logIn.xaml
    /// </summary>
    public partial class login : Window
    {
        public login()
        {
            InitializeComponent();
        }
        private void Log_In_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Check_Login();
            }
            catch
            {

            }
        }
        private void Check_Login()
        {
            try
            {
                DB db = new DB("users");
                db.AddCondition("user_name", User_name_TB.Text.Trim());

                DataRow User = db.SelectRow("select * from users");
                
                if (User != null)
                {
                    if (Password_TB.Password.GetHashCode().ToString() == User["user_pass"].ToString())
                    {
                        App.User = new User() { Id = int.Parse(User["User_Id"].ToString()), GroupId =User["user_grp_id"].ToString() , Name = User["user_name"].ToString() };                                                                        
                        Window m = new Window() ;

                        switch (int.Parse(User["user_grp_id"].ToString()))
                        { 
                            case  1://admin
                                                        m = new Main();
                                                        break;
                            case 2://station
                                                        m = new Station();
                                                        break;

                            case 3://factory
                                                        m = new Cement_Office();
                                                        break;

                        }
                       
                        this.Hide();
                        m.ShowDialog();
                                        Application.Current.Shutdown();

                    }
                    else
                    {
                        Message.Show("كلمة المرور غير صحيحة", MessageBoxButton.OK, 10);
                    }
                }
                else
                {
                    Message.Show("إسم المستخدم غير صحيح", MessageBoxButton.OK, 10);
                }
            }
            catch
            {

            }
        }
        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DB db = new DB("users");

                db.AddCondition("user_name", User_name_TB.Text.Trim());
                DataRow User = db.SelectRow("select * from users");
                
                if (User != null)
                {
                    if (Password_TB.Password.GetHashCode().ToString() == User["user_pass"].ToString())
                    {
                        App.User = new User() { Id = int.Parse(User["User_Id"].ToString()), GroupId = User["user_grp_id"].ToString(), Name = User["user_name"].ToString() };
                        Add_User u = new Add_User(Edit_Mode.Change_Password, User["User_Id"]);
                        this.Hide();
                        u.ShowDialog();
                        this.ShowDialog();
                    }
                    else
                    {
                        Message.Show("كلمة المرور غير صحيحة", MessageBoxButton.OK, 10);
                    }
                }
                else
                {
                    Message.Show("إسم المستخدم غير صحيح", MessageBoxButton.OK, 10);
                }
            }
            catch
            {

            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            InputLanguageManager.SetInputLanguage(User_name_TB, new System.Globalization.CultureInfo("ar-EG"));
        }

        private void User_name_TB_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    Password_TB.Focus();
                }
            }
            catch
            {

            }
        }

        private void Password_TB_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    Log_In.Focus();
                }
            }
            catch
            {

            }
        }
    }
}
