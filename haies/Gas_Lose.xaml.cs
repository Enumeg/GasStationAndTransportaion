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
    /// Interaction logic for outcome.xaml
    /// </summary>
    public partial class Gas_Lose : Window
    {
        object GLS_Id;

        public Gas_Lose(object gls_id = null)
        {
            InitializeComponent();

            Gas.Get_All_Gas(Gas_CB);

            GLS_Id = gls_id;

            Get_GLS_ID();
            Fill_DG();

        }

        private void Get_GLS_ID()
        {
            try
            {
                DB db2 = new DB("gas_lose");

                db2.SelectedColumns.Add("*");

                db2.AddCondition("gls_id", GLS_Id);

                DataRow DR = db2.SelectRow();

                Date_TB.Value = DateTime.Parse(DR["gls_date"].ToString());
                Amount_TB.Text = DR["gls_amount"].ToString();
                Gas_CB.SelectedValue = DR["gls_gas_id"];

            }
            catch
            {


            }
        }

        private void add_update_outcome_Click(object sender, RoutedEventArgs e)
        {
            try
            {


                if (Notify.validate("من فضلك ادخل التاريخ", Date_TB.Text, this))
                {
                    return;
                }

                if (Notify.validate("من فضلك اختر المحروق", Gas_CB.SelectedIndex, this))
                {
                    return;
                }

                if (Notify.validate("من فضلك ادخل الكميه المفقوده", Amount_TB.Text, this))
                {
                    return;
                }


                if (Add_Update())
                {
                    var log = new Log();
                    log.Columns.Add(new Column("التاريـخ", Date_TB.Value.Value.ToShortDateString()));
                    log.Columns.Add(new Column("القيـمــة", Amount_TB.Text));
                    log.Columns.Add(new Column("المحروق", Gas_CB.Text));
                    log.CreateLog("فاقد المحطة", this.GLS_Id == null);
                    Amount_TB.Text = "";
                    Gas_CB.SelectedIndex = -1;
                    Fill_DG();
                    GLS_Id = null;
                }


            }
            catch
            {

            }
        }

        public bool Add_Update()
        {
            try
            {

                DB DataBase = new DB("gas_lose");

                DataBase.AddColumn("gls_date", Date_TB.Text);
                DataBase.AddColumn("gls_amount", Amount_TB.Text);
                DataBase.AddColumn("gls_gas_id", Gas_CB.SelectedValue);


                if (this.GLS_Id == null)
                {
                    if (DataBase.IsNotExist("gls_id", "gls_amount", "gls_date", "gls_gas_id"))
                    {
                        return Confirm.Check(DataBase.Insert());
                    }
                    else
                    {
                        // ye3ny hwa mawgood asln mesh ha3ml 7aga 
                        Message.Show("هذا المستند موجود من قبل", MessageBoxButton.OK, 5);
                        return false;
                        //DataBase.AddCondition("pl_id", this.placeId);
                        //DataBase.Update();

                    }


                }

// hena ye3ny hwa mawgod ba3mel edit
                else
                {
                    DataBase.AddCondition("gls_id", this.GLS_Id);
                    return Confirm.Check(DataBase.Update());
                }
            }
            catch
            {
                return false;
            }
        }

        private void Gas_CB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Fill_DG();
        }

        private void Fill_DG()
        {
            DB db = new DB("gas_lose");
            try
            {
                db.AddCondition("gls_date", Date_TB.Value.Value.Date, Date_TB.Text == "", "<=", "SD");

                DataTable ds = db.SelectTable(@"select gls.*,g.gas_name gas from gas_lose gls join gas g on g.gas_id=gls.gls_gas_id where gls_date<=@SD ;");

                Gas_Lose_DG.ItemsSource = ds.DefaultView;
            }
            catch
            {

            }
        }

        private void EP_Add(object sender, EventArgs e)
        {
            try
            {
                Amount_TB.Text = "";
                Gas_CB.SelectedIndex = -1;
                Date_TB.Value = DateTime.Now;
            }
            catch
            {

            }
        }

        private void EP_Edit(object sender, EventArgs e)
        {
            try
            {
                DataRowView dr = Gas_Lose_DG.SelectedItem as DataRowView;
                Date_TB.Value = DateTime.Parse(dr["gls_date"].ToString());
                Amount_TB.Text = dr["gls_amount"].ToString();
                Gas_CB.SelectedValue = dr["gls_gas_id"];
                GLS_Id = dr["gls_id"];
            }
            catch
            {

            }

        }

        private void EP_Delete(object sender, EventArgs e)
        {
            try
            {
                if (Gas_Lose_DG.SelectedIndex != -1)
                {
                    if (Message.Show("هل تريد الحذف", MessageBoxButton.YesNoCancel, 5) == MessageBoxResult.Yes)
                    {
                        var row = ((DataRowView)Gas_Lose_DG.SelectedItem);
                        DB db = new DB("gas_lose");
                        db.AddCondition("gls_id", row["gls_id"]);
                        if (db.Delete())
                        {
                            var log = new Log();
                            log.Columns.Add(new Column("التاريـخ", row["gls_date"]));
                            log.Columns.Add(new Column("القيـمــة", row["gls_amount"]));
                            log.Columns.Add(new Column("المحروق", row["gas"]));
                            log.CreateLog("فاقد المحطة");
                            Fill_DG();
                        }
                    }
                }


            }
            catch
            {

            }
        }

        //close class
    }
}
