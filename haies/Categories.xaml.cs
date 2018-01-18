using System;
using System.Windows;
using System.Windows.Controls;
using Source;
using System.Data;


namespace haies
{
    /// <summary>
    /// Interaction logic for Categories.xaml
    /// </summary>
    public partial class Categories : Window
    {
        object Category_Id;

        public Categories()
        {
            InitializeComponent();
            Fill_Categories_LB();
        }

        private void Fill_Categories_LB()
        {
            try
            {
                DB db = new DB("Categories");
                db.AddCondition("cat_name", Category_TB.Text.Trim(), false, " like ");
                db.SelectedColumns.Add("*");
                db.Fill(LB, "cat_id", "cat_name");
            }
            catch
            {

            }
        }

        public static void Get_All_Categories(ComboBox CB, string All = "")
        {
            try
            {
                DB db = new DB("Categories");
                db.SelectedColumns.Add("*");
                db.Fill(CB, "cat_id", "cat_name", "", All);
            }
            catch
            {

            }
        }

        private bool Add_Update()
        {
            try
            {

                DB DataBase = new DB("Categories");

                DataBase.AddColumn("cat_name", Category_TB.Text.Trim());

                if (Category_Id == null)
                {
                    if (DataBase.IsNotExist("cat_id", "cat_name"))
                    {
                        return Confirm.Check(DataBase.Insert());
                    }
                    else
                    {
                        Message.Show("لقد تم تسجيل هذه الفئة من قبل", MessageBoxButton.OK, 5);
                        return false;
                    }


                }
                else
                {
                    DataBase.AddCondition("cat_id", this.Category_Id);
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
                    log.Columns.Add(new Column("الإسم", Category_TB.Text));
                    log.CreateLog("الفئات", Category_Id == null);
                    if (!(bool)New.IsChecked)
                    {
                        Main_GD.RowDefinitions[1].Height = new GridLength(0);
                    }
                    Fill_Categories_LB();
                    Category_Id = null;
                    Category_TB.Text = "";
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
                Fill_Categories_LB();
                Category_Id = null;
                Category_TB.Text = "";
            }
            catch
            {

            }
        }

        private void EditPanel_Add(object sender, EventArgs e)
        {
            try
            {
                Category_Id = null;
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
                Category_Id = LB.SelectedValue;
                Category_TB.Text = ((DataRowView)LB.SelectedItem)[1].ToString();
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
                if (Message.Show("هل تريد حذف هذه المجموعة", MessageBoxButton.YesNoCancel, 10) == MessageBoxResult.Yes)
                {
                    Category_Id = LB.SelectedValue;
                    DB db = new DB("Categories");
                    db.AddCondition("cat_id", Category_Id);
                    if (db.Delete())
                    {
                        var log = new Log();
                        log.Columns.Add(new Column("إسم الفئة", ((DataRowView)LB.SelectedItem)["cat_name"]));
                        log.CreateLog("الفئات");
                    }
                    Fill_Categories_LB();
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
                Fill_Categories_LB();
            }
            catch
            {

            }
        }

    }
}
