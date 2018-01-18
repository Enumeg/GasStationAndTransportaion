using System;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using Source;

namespace haies
{
    /// <summary>
    /// Interaction logic for Product.xaml
    /// </summary>
    public partial class Archive : Page
    {

        string path = "", file = "";
        public Archive()
        {
            InitializeComponent();
            fill_Categories();
            Fill_LB();
        }
        private void Fill_LB()
        {

            try
            {
                DB db = new DB("archive");

                // search by text
                db.AddCondition("arc_title", Title_Search_TB.Text, false, " like ");

                // search by value
                db.AddCondition("arc_cat_id", Category_Search_CB.SelectedValue, Category_Search_CB.SelectedIndex < 1);


                db.Fill(LB, "arc_id", "arc_title", @"select * from archive");
            }
            catch
            {

            }

        }

        private bool Add_Update()
        {
            try
            {
                //create new DB giving table name
                DB DataBase = new DB("archive");
                //add columns names and inserted or update values
                DataBase.AddColumn("arc_title", Title_TB.Text);
                DataBase.AddColumn("arc_cat_id", Category_CB.SelectedValue);

                //check if item is selected
                if (LB.SelectedIndex == -1)
                {
                    //check if this item has inserted before 
                    if (DataBase.IsNotExist("arc_id", "arc_title", "arc_cat_id"))
                    {
                        var id = getFileId();
                        var targetPath = string.Format("{0}\\{1}.{2}", Properties.Settings.Default.ImagesPath, id, file.Split('.')[1]);
                        DataBase.AddColumn("arc_path", targetPath);
                        DataBase.AddColumn("arc_id", id);
                        //insert row
                        if (DataBase.Insert())
                        {
                            File.Copy(path, targetPath, true);
                            return true;
                        }
                        else
                            return false;
                    }
                    else
                    {
                        Message.Show("هذا المستند موجود من قبل", MessageBoxButton.OK, 5);
                        return false;
                    }
                }
                else
                {
                    // update row giving the id                  
                    var targetPath = file != "" ? string.Format("{0}\\{1}.{2}", Properties.Settings.Default.ImagesPath, LB.SelectedValue, file.Split('.')[1]) : path;
                    DataBase.AddColumn("arc_path", targetPath);
                    DataBase.AddCondition("arc_id", LB.SelectedValue);
                    if (DataBase.Update())
                    {
                        if (path != targetPath)
                            File.Copy(path, targetPath, true);

                        return true;
                    }
                    else
                        return false;
                }
            }
            catch
            {
                return false;
            }
        }

        private int getFileId()
        {
            try
            {
                DB db = new DB();
                var max = db.Select("select Max(arc_id) from archive");
                return max != null ? (int)max + 1 : 1;
            }
            catch
            {
                return 0;
            }
        }

        #region Image
        private void Img_MouseLeave(object sender, MouseEventArgs e)
        {
            try
            {
                if (!BTNImg.IsMouseOver && LB.IsEnabled == false)
                {
                    BTNImg.Visibility = System.Windows.Visibility.Hidden;
                }
            }
            catch
            {

            }

        }
        private void BTNImg_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog dlg = new OpenFileDialog();
                if ((bool)dlg.ShowDialog())
                {
                    BTNImg.Visibility = System.Windows.Visibility.Hidden;
                    if (!string.IsNullOrEmpty(dlg.FileName))
                    {
                        Img.Source = null;
                        Img.Source = new BitmapImage(new Uri(dlg.FileName));
                        path = dlg.FileName;
                        file = dlg.SafeFileName;
                    }
                }
            }
            catch
            {

            }


        }
        private void Img_MouseEnter(object sender, MouseEventArgs e)
        {
            try
            {
                if (!BTNImg.IsMouseOver && LB.IsEnabled == false)
                {
                    BTNImg.Visibility = System.Windows.Visibility.Visible;
                }
            }
            catch
            {

            }

        }
        #endregion

        private void EditPanel_Add(object sender, EventArgs e)
        {
            try
            {
                Main_Grid.RowDefinitions[2].Height = new GridLength(35);
                LB.IsEnabled = false;
                LB.SelectedIndex = -1;
                Form.Set_Style(Main_Grid, Source.Operations.Add);
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
                    Main_Grid.RowDefinitions[2].Height = new GridLength(35);
                    LB.IsEnabled = false;
                    Form.Set_Style(Main_Grid, Source.Operations.Edit);
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

                    if (Message.Show("هل تريد حذف هذا المستند", MessageBoxButton.YesNoCancel, 5) == MessageBoxResult.Yes)
                    {
                        DB db = new DB("archive");
                        db.AddCondition("arc_id", LB.SelectedValue);
                        db.Delete();
                        Fill_LB();

                    }
                }
            }
            catch
            {

            }
        }

        private void Save_Panel_Save(object sender, EventArgs e)
        {
            try
            {
                if (Confirm.Check(Add_Update()))
                {
                    var log = new Log();
                    log.Columns.Add(new Column("الفئة", Category_CB.Text));
                    log.Columns.Add(new Column("الإسم", Title_TB.Text));
                    log.CreateLog("الأرشيف", LB.SelectedIndex == -1);
                    Form.Set_Style(Main_Grid, Source.Operations.View);
                    Main_Grid.RowDefinitions[2].Height = new GridLength(0);
                    LB.IsEnabled = true;
                    file = path = "";
                    int i = LB.SelectedIndex;
                    Fill_LB();
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
                Main_Grid.RowDefinitions[2].Height = new GridLength(0);
                Form.Set_Style(Main_Grid, Source.Operations.View);
                file = path = "";
                LB.IsEnabled = true;
            }
            catch
            {

            }
        }

        private void LB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                path = ((DataRowView) LB.SelectedItem)["arc_path"].ToString();               
                Img.Source = new BitmapImage(new Uri(path));
                
            }
            catch
            {

            }
        }

        private void Fill_LB(object sender, EventArgs e)
        {
            Fill_LB();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Categories c = new Categories();
            c.ShowDialog();
            fill_Categories();
        }

        private void fill_Categories()
        {
            Categories.Get_All_Categories(Category_CB);
            Categories.Get_All_Categories(Category_Search_CB, "الكل");
        }

    }
}
