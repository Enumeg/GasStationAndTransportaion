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
    /// Interaction logic for Future_Sales.xaml
    /// </summary>
    public partial class Future_Sales : Window
    {
        Future Type;
        Customer_type CustType;
        public Future_Sales(Future type, Customer_type cust)
        {
            InitializeComponent();
            Type = type;
            CustType = cust;
            Get_Balance();
        }
        private void Get_Balance()
        {
            decimal Balance = 0, total = 0;

            try
            {
                DataTable Dt1 = new DataTable();
                Dt1.Columns.Add("Name"); Dt1.Columns.Add("Money"); Dt1.Columns.Add("Mobile");


                DB db2 = new DB();
                db2.AddCondition("cust_type", CustType);
                DataSet ds = db2.SelectSet(@"select dri_id ,p1.per_name,p1.per_mobile from drivers join persons p1 on p1.per_id= dri_per_id  left join persons p2 on p2.per_id=spr_per_id
                                             left join cars c on car_dri_id = dri_id where (p2.per_id<>1 OR p2.per_id is null) order by p1.per_name;                                                                                                                                                                                   
                                             select cust_Id, per_name ,per_mobile,per_id from persons join customer on per_id=cust_per_id and cust_type=@cust_type and per_status =1 order by per_name  ");

                switch (Type)
                {
                    case Future.All:

                        foreach (DataRow Row in ds.Tables[1].Rows)
                        {
                            Balance = CustomerAccounts.Get_Balance(CustType, Row[0], Row[3], Date.Value.Value.Date.AddDays(1));
                            Dt1.Rows.Add(Row[1], Balance.ToString("0.00"), Row[2]);
                            total += Balance;
                        }
                        foreach (DataRow Row in ds.Tables[0].Rows)
                        {
                            DB db = new DB();
                            db.AddCondition("cstl_date", Date.Value.Value.Date, false, "<", "Date");
                            db.AddCondition("cstl_cust_id", Row[0], false, "=", "cst_id");

                            decimal.TryParse(db.Select(@"select COALESCE(sum(cstl_value),0) -(select COALESCE(sum((rec_sell_price*rec_amount)+(COALESCE(trs_sell_price,0)*COALESCE(rec_amount,1))
                                                -trs_discount),0)- COALESCE(sum(trs_paid),0) from transportation left join receipt on rec_id= trs_rec_id where trs_date<=@Date and trs_dri_id=@cst_id)
                                                 from client_loans where cstl_date<=@Date and cstl_dri_id=@cst_id; ").ToString(), out Balance);
                            Balance = Balance * -1;
                            Dt1.Rows.Add(Row[1], Balance.ToString("0.00"), Row[2]);
                            total += Balance;
                        }
                        break;
                    case Future.Clients:
                        foreach (DataRow Row in ds.Tables[0].Rows)
                        {
                            DB db = new DB();
                            db.AddCondition("cstl_date", Date.Value.Value.Date, false, "<=", "Date");
                            db.AddCondition("cstl_cust_id", Row[0], false, "=", "cst_id");

                            decimal.TryParse(db.Select(@"select COALESCE(sum(cstl_value),0) -(select COALESCE(sum((rec_sell_price*rec_amount)+(COALESCE(trs_sell_price,0)*COALESCE(rec_amount,1))
                                                -trs_discount),0)- COALESCE(sum(trs_paid),0) from transportation left join receipt on rec_id= trs_rec_id where trs_date<=@Date and trs_dri_id=@cst_id)
                                                 from client_loans where cstl_date<=@Date and cstl_dri_id=@cst_id; ").ToString(), out Balance);
                            Balance = Balance * -1;
                            Dt1.Rows.Add(Row[1], Balance.ToString("0.00"), Row[2]);
                            total += Balance;
                        }
                        break;
                    case Future.Customers:
                        foreach (DataRow Row in ds.Tables[1].Rows)
                        {
                            Balance = CustomerAccounts.Get_Balance(CustType, Row[0], Row[3], Date.Value.Value.Date.AddDays(1));
                            Dt1.Rows.Add(Row[1], Balance.ToString("0.00"), Row[2]);
                            total += Balance;
                        }

                        break;

                }

                Dt1.Rows.Add("الإجمالي", total.ToString("0.00"));
                Balance_DG.ItemsSource = Dt1.DefaultView;
            }
            catch
            {

            }

        }
        private void Date_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            try
            {
                Get_Balance();
            }
            catch
            {

            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CPrinting.CPrinting print = new CPrinting.CPrinting();
                App.Get_Printed_Table(print, Balance_DG);
                print.print();
            }
            catch
            {

            }
        }

    }
}
