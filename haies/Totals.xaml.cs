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
    /// Interaction logic for Totals.xaml
    /// </summary>
    public partial class Totals : Window
    {
        public Totals()
        {
            InitializeComponent();
        }
        #region Balance
        private void Get_Balance()
        {
            decimal Total_Sales = 0, Last_Balance = 0, Total_Discount = 0, Cash_Sales = 0;

            try
            {

                DB db = new DB();
                db.AddCondition("sin_date", From_DTP.Value.Value.Date, false, ">=", "SD");
                db.AddCondition("sin_date", To_DTP.Value.Value.Date, false, "<=", "ED");
                DataSet ds = db.SelectSet(@"select sum(COALESCE((rec_sell_price*rec_amount),0)+(COALESCE(trs_sell_price,0)*COALESCE(rec_amount,1))) total_price,
                                            COALESCE(sum(trs_paid),0) ,COALESCE(sum(trs_discount),0)
                                            from transportation left join receipt on trs_rec_id=rec_id where trs_date>=@SD and trs_date<=@ED;                                            
                                            select COALESCE(sum(oto_value),0) from outcome_office where oto_date>=@SD and oto_date<=@ED;                                          
                                            select COALESCE(sum(bnko_value),0) from bank_office where bnko_date>=@SD and bnko_date<=@ED;
                                            select COALESCE(sum(cstl_value),0) + (select COALESCE(sum(cstl_value),0) from customer_loans join customer on cust_id=cstl_cust_id 
                                            and cust_type=0 where cstl_date>=@SD and cstl_date<=@ED) from Client_loans where cstl_date>=@SD and cstl_date<=@ED;");

                Last_Balance = Get_Last_Balance();
                Total_Sales = decimal.Parse(ds.Tables[0].Rows[0][0].ToString());
                Cash_Sales = decimal.Parse(ds.Tables[0].Rows[0][1].ToString());
                Total_Discount = decimal.Parse(ds.Tables[0].Rows[0][2].ToString());
                Total_Sales -= Total_Discount;               
                Total_Sales_TK.Text = Total_Sales.ToString("0.00");
                cash_Sales_TK.Text = Cash_Sales.ToString("0.00");
                Futures_Sales_TK.Text = (Total_Sales - Cash_Sales).ToString("0.00");
                Total_Outcome_TK.Text = decimal.Parse(ds.Tables[1].Rows[0][0].ToString()).ToString("0.00");              
                Payments_TK.Text = decimal.Parse(ds.Tables[3].Rows[0][0].ToString()).ToString("0.00");
                Total_Income_TK.Text = (Cash_Sales + decimal.Parse(ds.Tables[3].Rows[0][0].ToString())
                                                  - decimal.Parse(ds.Tables[1].Rows[0][0].ToString())).ToString("0.00");
            }

            catch
            {

            }
        }
        private decimal Get_Last_Balance()
        {
            decimal Balance = 0;
            try
            {
                DB db = new DB();
                db.AddCondition("pmr_date", From_DTP.Value.Value.Date, false, "<", "Today");
                decimal.TryParse(db.Select(@"select sum(trs_paid)  
                                         + (select COALESCE(sum(cstl_value),0) from Client_loans join drivers on dri_id=cstl_dri_id  where cstl_date<@Today)
                                         + (select COALESCE(sum(cstl_value),0) from customer_loans join customer on cust_id=cstl_cust_id and cust_type=0 where cstl_date<@Today)                                            
                                         - (select COALESCE(sum(oto_value),0) from outcome_office where oto_date<@Today)                                                                                                                       
                                         - (select COALESCE(sum(bnko_value),0) from bank_office where bnko_date<@Today)                                          
                                            from transportation where trs_date<@Today;").ToString(), out Balance);
            }
            catch
            {

            }
            return Balance;
        }

        #endregion

        private void From_DTP_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            try
            {
                Get_Balance();
            }
            catch
            {

            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            try
            {
                CPrinting.CPrinting print = new CPrinting.CPrinting();
                print.header.Add(string.Format(" كشف حساب مكتب الأسمنت عن الفترة من {0} إلى {1}",
                From_DTP.Value.Value.Date.ToString("yyyy/MM/dd"), To_DTP.Value.Value.Date.ToString("yyyy/MM/dd")));
                print.printedDataTable.Add(new DataTable());               
                print.FooterTable.Add("إجمالــي المبيعــــات :", Total_Sales_TK.Text);
                print.FooterTable.Add("إجمالي المدفوعــات :", Payments_TK.Text);
                print.FooterTable.Add("المبيعــــات الآجلـــة :", Futures_Sales_TK.Text);
                print.FooterTable.Add("المبيـعــات النقـديـــة :", cash_Sales_TK.Text);
                print.FooterTable.Add("إجمالي المصروفات :", Total_Outcome_TK.Text);
                print.FooterTable.Add("صافي الدخل :", Total_Income_TK.Text);   
                print.print();
            }
            catch
            {

            }
        }
    }
}
