using Source;
using System;
using System.Collections.Generic;
using System.Data;
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

namespace haies
{
    /// <summary>
    /// Interaction logic for Claims.xaml
    /// </summary>
    public partial class Claims : Window
    {
        Claim Claim;
        DataRowView current;
        public Claims(Claim claim)
        {
            InitializeComponent();
            Claim = claim;
        }

        private void Get_Claims()
        {
            var db = new DB();


            db.AddCondition("CLM_from", From_DTP.Value.Value.Date, false, ">=", "SD");
            db.AddCondition("CLM_to", To_DTP.Value.Value.Date, false, "<=", "ED");
            db.AddCondition("CLM_cust_id", Claim.Id);
            CLaims_DG.ItemsSource = db.SelectTableView("select * from claims");
        }
        private bool Add_Update()
        {
            try
            {
                //create new DB giving table name
                DB DataBase = new DB("claims");
                //add columns names and inserted or update values
                DataBase.AddColumn("clm_date", Date_DTP.Value.Value.Date);
                DataBase.AddColumn("clm_from", From_DTP.Value.Value.Date);
                DataBase.AddColumn("clm_to", To_DTP.Value.Value.Date);

                //check if item is selected
                if (current == null)
                {
                    DataBase.AddColumn("clm_cust_id", Claim.Id);
                    //check if this item has inserted before                                         
                    return DataBase.Insert();
                }
                else
                {
                    // update row giving the id                  
                    DataBase.AddCondition("clm_id", current["clm_id"]);
                    return DataBase.Update();
                }
            }
            catch
            {
                return false;
            }
        }
        private void From_DTP_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (IsLoaded)
                if (CLaims_DG.Visibility == System.Windows.Visibility.Visible)
                {
                    Get_Claims();
                }
        }
        private void SP_Save(object sender, EventArgs e)
        {
            if (Confirm.Check(Add_Update()))
            {
                var log = new Log();
                log.Columns.Add(new Column("الإسم", Claim.Name));
                log.Columns.Add(new Column("التاريخ", Date_DTP.Value.Value.Date.ToShortDateString()));
                log.Columns.Add(new Column("من", From_DTP.Value.Value.Date.ToShortDateString()));
                log.Columns.Add(new Column("إلى", To_DTP.Value.Value.Date.ToShortDateString()));
                log.CreateLog("المطالبات", current == null);

                CLaims_DG.Visibility = System.Windows.Visibility.Visible;
                EP.Visibility = System.Windows.Visibility.Visible;
                SP.Visibility = System.Windows.Visibility.Collapsed;
                current = null;
            }
        }

        private void SP_Cancel(object sender, EventArgs e)
        {
            CLaims_DG.Visibility = System.Windows.Visibility.Visible;
            EP.Visibility = System.Windows.Visibility.Visible;
            SP.Visibility = System.Windows.Visibility.Collapsed;
            current = null;
        }

        private void EP_Add(object sender, EventArgs e)
        {
            CLaims_DG.Visibility = System.Windows.Visibility.Collapsed;
            EP.Visibility = System.Windows.Visibility.Collapsed;
            SP.Visibility = System.Windows.Visibility.Visible;
        }

        private void EP_Edit(object sender, EventArgs e)
        {
            if (CLaims_DG.SelectedIndex != -1)
            {
                current = CLaims_DG.SelectedItem as DataRowView;
                Date_DTP.Value = DateTime.Parse(current["clm_date"].ToString());
                From_DTP.Value = DateTime.Parse(current["clm_from"].ToString());
                To_DTP.Value = DateTime.Parse(current["clm_to"].ToString());
                CLaims_DG.Visibility = System.Windows.Visibility.Collapsed;
                EP.Visibility = System.Windows.Visibility.Collapsed;
                SP.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                Message.Show("من فضلك إختار مطالبة", MessageBoxButton.OK, 5);
            }
        }

        private void EP_Delete(object sender, EventArgs e)
        {
            if (Message.Show("هل تريد حذف هذه المطالبة", MessageBoxButton.YesNoCancel, 5) == MessageBoxResult.Yes)
            {
                DB db = new DB("claims");
                db.AddCondition("clm_id", ((DataRowView)CLaims_DG.SelectedValue)["clm_id"]);
                if (db.Delete())
                {
                    var cr = CLaims_DG.SelectedItem as DataRowView;
                    var log = new Log();
                    log.Columns.Add(new Column("الإسم", Claim.Name));
                    log.Columns.Add(new Column("التاريخ", cr["clm_date"].ToString()));
                    log.Columns.Add(new Column("من", cr["clm_from"].ToString()));
                    log.Columns.Add(new Column("إلى", cr["clm_to"].ToString()));
                    log.CreateLog("المطالبات");

                }
                Get_Claims();
            }
        }

        private void View_Click(object sender, RoutedEventArgs e)
        {
            current = CLaims_DG.SelectedItem as DataRowView;
            Claim.From = DateTime.Parse(current["clm_from"].ToString());
            Claim.To = DateTime.Parse(current["clm_to"].ToString());

            View_Claims view = new View_Claims(Claim);
            this.Hide();
            view.ShowDialog();
            current = null;
            this.ShowDialog();
        }
    }
}
