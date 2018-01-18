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
    /// Interaction logic for places.xaml
    /// </summary>
    public partial class Groups : Window
    {

        object GroupId = null;

        public Groups()
        {
            InitializeComponent();
            fill_types_listbox();
        }
        private void fill_types_listbox()
        {

            DB db2 = new DB("groups");

            // search by name
            db2.AddCondition("grp_name", "%" + Group_TB.Text + "%", false, " like ");

            db2.Fill(LB, "grp_id", "grp_name", "select * from groups");


        }
        public static void Get_All_Groups(ComboBox CB, string All = "")
        {
            try
            {
                DB db = new DB("groups");
                db.SelectedColumns.Add("*");
                db.Fill(CB, "grp_id", "grp_name", "", All);
            }
            catch
            {

            }
        }

        private void EP_Add(object sender, EventArgs e)
        {
            try
            {
                GroupId = null;
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
                    GroupId = LB.SelectedValue;
                    Group_TB.Text = ((DataRowView)LB.SelectedItem)[1].ToString();
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
                    if (Message.Show("هل تريد حذف هذه المجموعه", MessageBoxButton.YesNoCancel, 10) == MessageBoxResult.Yes)
                    {
                        GroupId = LB.SelectedValue;
                        DB db = new DB("groups");
                        db.AddCondition("grp_id", GroupId);
                        if (db.Delete())
                        {
                            var log = new Log();
                            log.Columns.Add(new Column("الإسم", Group_TB.Text));
                            log.CreateLog("المجموعات");
                            fill_types_listbox();

                        }
                        fill_types_listbox();
                    }
                }
            }
            catch
            {

            }
        }   
       
        private void add_update_outcome_Click(object sender, RoutedEventArgs e)
        {
            try
            {


                if (Notify.validate("من فضلك ادخل اسم المجموعه", Group_TB.Text, this))
                {
                    return;
                }

                
                if (Add_Update())
                {
                    var log = new Log();
                    log.Columns.Add(new Column("الإسم", Group_TB.Text));
                    log.CreateLog("المجموعات", GroupId == null);
                    GroupId = null;
                    fill_types_listbox();
                    Main_GD.RowDefinitions[1].Height = new GridLength(0);
                    
                    // yesafar
                    Group_TB.Text = "";
               }
            }
            catch
            {                
                return;
            }
        }
        public bool Add_Update()
        {
            try
            {

                DB DataBase = new DB("groups");

                DataBase.AddColumn("grp_name", Group_TB.Text);

                if (this.GroupId == null)
                {
                    if (DataBase.IsNotExist("grp_id", "grp_name"))
                    {
                      return Confirm.Check(  DataBase.Insert());                      
                    }
                    else
                    {
                        Message.Show("لقد تم تسجيل هذه المجموعه من قبل", MessageBoxButton.OK, 5);
                        return false;
                    }


                }
                else
                {
                    DataBase.AddCondition("grp_id", this.GroupId);                  
                    return Confirm.Check(DataBase.Update());
                }
            }
            catch
            {
                //MessageBox.Show("kiki_method");
                return false;
            }
        }
        private void LB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            try
            {

            }
            catch
            {

            }

        }
        private void Place_TB_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                fill_types_listbox();
            }
            catch
            {

            }
        }
    }
}
