using System;
using System.Windows;
using System.Windows.Controls;
using Source;
using System.Data;


namespace haies
{
    /// <summary>
    /// Interaction logic for Suppliers.xaml
    /// </summary>
    public partial class Suppliers : Window
    {

        public Suppliers()
        {
            InitializeComponent();
            Fill_Suppliers_LB();
        }

        private void Fill_Suppliers_LB()
        {
            try
            {
                if (LB.IsEnabled == true)
                {
                    DB db = new DB("Suppliers");
                    db.AddCondition("sup_name", Factory_TB.Text.Trim(), false, " like ");
                    db.SelectedColumns.Add("*");
                    db.Fill(LB, "sup_id", "sup_name");
                }
            }
            catch
            {

            }
        }

        public static void Get_All_Suppliers(ComboBox CB, string All = "")
        {
            try
            {
                DB db = new DB("Suppliers");
                db.SelectedColumns.Add("*");
                db.Fill(CB, "sup_id", "sup_name", "", All);
            }
            catch
            {

            }
        }

        private bool Add_Update()
        {
            try
            {

                DB DataBase = new DB("Suppliers");

                DataBase.AddColumn("sup_name", Factory_TB.Text.Trim());

                if (LB.SelectedIndex == -1)
                {
                    if (DataBase.IsNotExist("sup_id", "sup_name"))
                    {
                        return Confirm.Check(DataBase.Insert());
                    }
                    else
                    {
                        Message.Show("لقد تم تسجيل هذه المورد من قبل", MessageBoxButton.OK, 5);
                        return false;
                    }


                }
                else
                {
                    DataBase.AddCondition("sup_id", LB.SelectedValue);
                    return Confirm.Check(DataBase.Update());
                }
            }
            catch
            {
                //MessageBox.Show("kiki_method");
                return false;
            }
        }

        private void Save_Save(object sender, EventArgs e)
        {
            try
            {

                if (Add_Update())
                {
                    var log = new Log();
                    log.Columns.Add(new Column("الإسم", Factory_TB.Text));
                    log.CreateLog("الموردين", LB.SelectedIndex == -1);

                    if (!(bool)New.IsChecked)
                    {
                        Main_GD.RowDefinitions[1].Height = new GridLength(0);
                    } 
                     LB.IsEnabled = true;
                    Fill_Suppliers_LB();
                    Factory_TB.Text = "";
                   
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
                Main_GD.RowDefinitions[1].Height = new GridLength(0);
                Fill_Suppliers_LB();
                Factory_TB.Text = "";
                LB.IsEnabled = true;
            }
            catch
            {

            }
        }

        private void EditPanel_Add(object sender, EventArgs e)
        {
            try
            {
                LB.SelectedIndex = -1;
                LB.IsEnabled = false;
                Main_GD.RowDefinitions[1].Height = new GridLength(35);
            }
            catch
            {

            }
        }

        private void EditPanel_Edit(object sender, EventArgs e)
        {
            try
            {
                LB.IsEnabled = false;
                Factory_TB.Text = ((DataRowView)LB.SelectedItem)[1].ToString();
                Main_GD.RowDefinitions[1].Height = new GridLength(35);
            }
            catch
            {

            }
        }

        private void EditPanel_Delete(object sender, EventArgs e)
        {

            try
            {
                if (Message.Show("هل تريد حذف هذه المورد", MessageBoxButton.YesNoCancel, 10) == MessageBoxResult.Yes)
                {
                    DB db = new DB("Suppliers");
                    db.AddCondition("sup_id", LB.SelectedValue);
                    if (db.Delete())
                    {
                        var log = new Log();
                        log.Columns.Add(new Column("الإسم", Factory_TB.Text));
                        log.CreateLog("الموردين");
                        Fill_Suppliers_LB();
                    }
                }
            }
            catch
            {

            }
        }

        private void Category_TB_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                Fill_Suppliers_LB();
            }
            catch
            {

            }
        }

    }
}
