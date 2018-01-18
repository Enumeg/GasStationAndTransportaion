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

namespace haies
{
    /// <summary>
    /// Interaction logic for Product.xaml
    /// </summary>
    public partial class Gas_Price : Page
    {
        public Gas_Price()
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
                if (Cement_Prices_DG.Columns.IndexOf(e.Column) == 2)
                {
                    d.AddColumn("gsp_buyCost", ((TextBox)e.Column.GetCellContent(e.Row)).Text);
                }
                else
                {
                    d.AddColumn("gsp_sellCost", ((TextBox)e.Column.GetCellContent(e.Row)).Text);

                }
                if (d.Update())
                {

                    var log = new Log();
                    log.Columns.Add(new Column("المحروق", drv[1]));
                    log.Columns.Add(new Column("سعر الشراء", d.Columns_Values[1].Name == "gsp_buyCost" ? d.Columns_Values[1].Value : drv[2]));
                    log.Columns.Add(new Column("سعر البيع", d.Columns_Values[1].Name == "gsp_sellCost" ? d.Columns_Values[1].Value : drv[3]));
                    log.CreateLog("أسعار المحروقات", false);
                    Fill_DG();
                }
            }
            catch
            {

            }
        }
    }
}
