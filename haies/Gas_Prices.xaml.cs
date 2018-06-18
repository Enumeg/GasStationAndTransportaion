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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Source;
using System.IO;
using Microsoft.Win32;
using System.Data;
using System.Web.UI;
using Page = System.Windows.Controls.Page;

namespace haies
{
    /// <summary>
    /// Interaction logic for Product.xaml
    /// </summary>
    public partial class Gas_Prices : Page
    {
        public Gas_Prices()
        {
            InitializeComponent();
            Fill_DG();

        }
        private void Fill_DG()
        {
            try
            {
                DB db2 = new DB("gas_price");

                Cement_Prices_DG.ItemsSource = db2.SelectTableView("select g.gas_name,gsp.* from gas_price gsp join gas g on gas_id=gsp_gas_id ");
            }
            catch
            {

            }
        }
        private void Cement_Prices_DG_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            try
            {

                DataRowView drv = Cement_Prices_DG.SelectedItem as DataRowView;
                DB d = new DB("gas_price");
                d.AddCondition("gsp_id", drv["gsp_id"]);            
                d.AddColumn(((Binding)((DataGridTextColumn)e.Column).Binding).Path.Path, ((TextBox)e.Column.GetCellContent(e.Row)).Text);            
                if (d.Update())
                {

                    var log = new Log();
                    log.Columns.Add(new Column("المحروق", drv[1]));
                    log.Columns.Add(new Column("سعر الشراء", d.Columns_Values[1].Name == "gsp_buyCost" ? d.Columns_Values[1].Value : drv[2]));
                    log.Columns.Add(new Column("سعر البيع", d.Columns_Values[1].Name == "gsp_sellCost" ? d.Columns_Values[1].Value : drv[3]));
                    log.Columns.Add(new Column("ضريبة الشراء", d.Columns_Values[1].Name == "gsp_buy_tax" ? d.Columns_Values[1].Value : drv[4]));
                    log.Columns.Add(new Column("ضريبة البيع", d.Columns_Values[1].Name == "gsp_sell_tax" ? d.Columns_Values[1].Value : drv[5]));
                    log.CreateLog("أسعار المحروقات", false);
                    Fill_DG();
                }
            }
            catch
            {

            }
        }

        private void Gas_EP_Add(object sender, EventArgs e)
        {
            var prices = new Gas_Price();
            prices.ShowDialog();
            Fill_DG();
        }

        private void Gas_EP_Edit(object sender, EventArgs e)
        {
            var prices = new Gas_Price(((DataRowView)Cement_Prices_DG.SelectedItem)[1]);
            prices.ShowDialog();
            Fill_DG();
        }

        private void Gas_EP_Delete(object sender, EventArgs e)
        {
            try
            {
                if (Cement_Prices_DG.SelectedIndex != -1)
                {
                    if (Message.Show("هل تريد حذف هذا السعر", MessageBoxButton.YesNoCancel, 5) == MessageBoxResult.Yes)
                    {
                        DB DataBase = new DB("gas_price");
                        DataBase.AddCondition("gsp_id", ((DataRowView)Cement_Prices_DG.SelectedItem)[1]);
                        if (DataBase.Delete())
                        {
                            var log = new Log();
                            log.Columns.Add(new Column("التاريـخ", ((DataRowView)Cement_Prices_DG.SelectedItem)[1]));
                            log.Columns.Add(new Column("المحروق", ((DataRowView)Cement_Prices_DG.SelectedItem)[2]));
                            log.CreateLog("أسعار المحروقات");
                        }
                        Fill_DG();
                    }
                }
            }
            catch
            {

            }
        }
    }
}
