using System.Windows;
using System.Windows.Controls;
using Source;
using System.Collections.Generic;
using System.Data;


namespace haies
{
    /// <summary>
    /// Interaction logic for Users.xaml
    /// </summary>
    public partial class Users : Window
    {
        string[] Pages_Names = new[] { "العملاء", "خدمات العملاء", "أقساط العملاء", "الشركات", "مدفوعات الشركات", "الوصلات", "المصروفات", "الخدمات", "المستخدمين", "المندوبيين", "الحسابات" };
        List<string> Pages = new List<string>();
        DataTable Group_Privileges_Table;

        public Users()
        {
            InitializeComponent();
            Groups.Get_All_Groups(Groups_CB, "الكل");
            Groups_CB.SelectedIndex = 0;
            Group_Privileges_Table = new DataTable();


            Pages.Add("LineParteners");
            Pages.Add("PartnerServices");
            Pages.Add("Payment");
            Pages.Add("Company");
            Pages.Add("PayCompany");
            Pages.Add("Receipt");
            Pages.Add("Outcome");
            Pages.Add("Services");
            Pages.Add("Users");
            Pages.Add("collector");
            Pages.Add("Accounting");


            Group_Privileges_Table.Columns.Add("Page");
            Group_Privileges_Table.Columns.Add("Window");
            Group_Privileges_Table.Columns.Add("Add_String");
            Group_Privileges_Table.Columns.Add("Edit_String");
            Group_Privileges_Table.Columns.Add("Del_String");
            Group_Privileges_Table.Columns.Add("View_String");
            Group_Privileges_Table.Columns.Add("Add");
            Group_Privileges_Table.Columns.Add("Edit");
            Group_Privileges_Table.Columns.Add("Del");
            Group_Privileges_Table.Columns.Add("View");

            for (int i = 0; i < Pages.Count; i++)
            {
                Group_Privileges_Table.Rows.Add(Pages[i], Pages_Names[i], Pages[i] + "_Add", Pages[i] + "_Edit", Pages[i] + "_Del", Pages[i] + "_View", false, false, false, false);
            }
            Group_Privileges_DG.ItemsSource = Group_Privileges_Table.DefaultView;
        }

        private void Fill_Users_LB()
        {

            DB db2 = new DB("users");

            // search by name
            db2.AddCondition("user_name", "%" + user_name_ser_TB.Text + "%", false, " like ");

            // search by position
            db2.AddCondition("user_grp_id", Groups_CB.SelectedValue, Groups_CB.SelectedIndex < 1);


            db2.Fill(LB, "user_id", "user_name", "select * from users");


        }

        private void Fill_Group_Privileges_DG()
        {

            try
            {
                for (int i = 0; i < Pages.Count; i++)
                {
                    Group_Privileges_Table.Rows[i][6] = false;
                    Group_Privileges_Table.Rows[i][7] = false;
                    Group_Privileges_Table.Rows[i][8] = false;
                    Group_Privileges_Table.Rows[i][9] = false;
                }
            }
            catch
            {

            }
        }

        private void Fill_Group_Privileges()
        {

            try
            {
                DB d = new DB("privilages");
                d.SelectedColumns.Add("prv_page");
                d.AddCondition("prv_grp_id", Groups_CB.SelectedValue, Groups_CB.SelectedIndex < 1);
                foreach (DataRow row in d.SelectTable().Rows)
                {
                    Group_Privileges_Table.Rows[Pages.IndexOf(row[0].ToString().Split('_')[0])][row[0].ToString().Split('_')[1]] = true;
                }
                Group_Privileges_DG.ItemsSource = Group_Privileges_Table.DefaultView;

            }
            catch
            {

            }
        }

        private void user_name_ser_TB_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                Fill_Users_LB();
            }
            catch
            {

            }
        }

        private void user_type_ser_CB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                Fill_Users_LB();
                if (Groups_CB.SelectedIndex > 0)
                {
                    Fill_Group_Privileges_DG();
                    Fill_Group_Privileges();
                }
            }
            catch
            {

            }
        }

        private void Admin_EP_Add(object sender, System.EventArgs e)
        {
            try
            {
                Add_User user = new Add_User(Edit_Mode.Add);
                user.ShowDialog();
                Fill_Users_LB();
            }
            catch
            {

            }
        }

        private void Admin_EP_Edit(object sender, System.EventArgs e)
        {
            try
            {
                Add_User user = new Add_User(Edit_Mode.Edit, LB.SelectedValue);
                user.ShowDialog();
                Fill_Users_LB();
            }
            catch
            {

            }
        }

        private void Admin_EP_Delete(object sender, System.EventArgs e)
        {
            try
            {
                if (Message.Show("هل تريد حذف هذا المستخدم", MessageBoxButton.YesNoCancel, 6) == MessageBoxResult.Yes)
                {
                    DB db = new DB("users");
                    db.AddCondition("user_id", LB.SelectedValue);
                    if (db.Delete())
                    {
                        var log = new Log();
                        log.Columns.Add(new Column("الإسم", user_name_ser_TB.Text));
                        log.Columns.Add(new Column("المجموعة", Groups_CB.Text));
                        log.CreateLog("المستخدمين");
                        Fill_Users_LB();
                    } 
                }
            }
            catch
            {

            }
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                CheckBox c = sender as CheckBox;
                if (Groups_CB.SelectedIndex < 1)
                {
                    c.IsChecked = !c.IsChecked;
                    Message.Show("من فضلك أختر النوع أولاً", MessageBoxButton.OK);
                }
                else
                {
                    DB d = new DB("privilages");
                    if ((bool)c.IsChecked)
                    {
                        d.AddColumn("prv_page", c.Tag);
                        d.AddColumn("prv_grp_id", Groups_CB.SelectedValue);
                        d.Insert();
                    }
                    else
                    {
                        d.AddCondition("prv_page", c.Tag);
                        d.AddCondition("prv_grp_id", Groups_CB.SelectedValue);
                        d.Delete();
                    }
                }
            }
            catch
            {

            }
        }

        private void Groups_BTN_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Groups g = new Groups();
                g.ShowDialog();
                Groups.Get_All_Groups(Groups_CB, "الكل");
            }
            catch
            {

            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {

               


            }
            catch
            {


            }
        }


        internal static void GetAllUsers(ComboBox Users_CB, string all ="")
        {
            try
            {
                DB db = new DB();
                db.Fill(Users_CB, "user_id", "user_name", "select * from users", all);
            }
            catch 
            {
                
                
            }
        }
    }
}
