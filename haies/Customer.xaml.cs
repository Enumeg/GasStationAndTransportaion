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
using System.Data;

namespace haies
{
    /// <summary>
    /// Interaction logic for Product.xaml
    /// </summary>
    public partial class Customer : Page
    {
        Customer_type Customer_Type;
        public Customer(Customer_type customer_Type)
        {
            InitializeComponent();
            Customer_Type = customer_Type;
            if (Customer_Type == Customer_type.محطة)
            {
                Footer_GD.ColumnDefinitions[1].Width = new GridLength(0);
                Customers_Out_DG.Visibility = System.Windows.Visibility.Collapsed;
                Customer_Out_DG.Visibility = System.Windows.Visibility.Visible;
            }
            Payment_CB.ItemsSource = Enum.GetValues(typeof(Customer_Payment));
            Status_CB.ItemsSource = Enum.GetValues(typeof(Status));
            Status_CB.SelectedIndex = 1;
            Fill_Customers_LB();
        }

        private void Fill_Customers_LB()
        {

            DB db2 = new DB("person");

            // filter by name
            db2.AddCondition("cust_type", Customer_Type);

            // search by name
            db2.AddCondition("per_name", "%" + Name_Search_TB.Text + "%", false, " like ");

            // search by mobile
            db2.AddCondition("per_mobile", "%" + Mobile_Search_TB.Text + "%", false, " like ");
            db2.AddCondition("per_status", Status_CB.SelectedIndex);

            db2.Fill(LB, "cust_id", "per_name", "select p.*,caro.* from persons p join customer caro on per_id=cust_per_id order by per_name");


        }

        private void Get_Customers_Accounts()
        {

            try
            {
                decimal[] Totals;
                if (Customer_Type == Customer_type.مصنع)
                    Totals = CustomerAccounts.Get_Customers_Accounts(Customer_Type, LB.SelectedValue, ((DataRowView)LB.SelectedItem)["per_id"], From_DTP.Value.Value.Date, To_DTP.Value.Value.Date, Customers_Out_DG, Customers_In_DG);
                else
                    Totals = CustomerAccounts.Get_Customers_Accounts(Customer_Type, LB.SelectedValue, ((DataRowView)LB.SelectedItem)["per_id"], From_DTP.Value.Value.Date, To_DTP.Value.Value.Date, Customer_Out_DG, Customers_In_DG);

                Balance_Before_TB.Text = Totals[0].ToString("0.00");
                Total_TB.Text = Totals[1].ToString("0.00");
                Paid_TB.Text = Totals[2].ToString("0.00");
                Rest_TB.Text = Totals[3].ToString("0.00");
                Balance_After_TB.Text = Totals[4].ToString("0.00");
            }
            catch
            {

            }
        }

        public static void Get_All_Customers(ComboBox CB, Customer_type cust_typ, string all = "")
        {

            try
            {
                DB db2 = new DB("persons");

                db2.AddCondition("cust_type", cust_typ);
                db2.AddCondition("per_status", Status.فعال);

                db2.Fill(CB, "cust_id", "per_name", "select p.*,c.* from persons p join customer c on c.cust_per_id=p.per_id order by per_name", all);

            }

            catch
            {

            }
        }

        public static void Get_All_Customers(ComboBox CB, string all = "")
        {

            try
            {
                DB db2 = new DB("persons");
                db2.AddCondition("per_status", Status.فعال);


                db2.Fill(CB, "cust_id", "per_name", "select p.*,c.* from persons p join customer c on c.cust_per_id=p.per_id order by per_name", all);

            }

            catch
            {

            }
        }


        private bool Add_Update()
        {


            try
            {

                DB DataBase = new DB("persons");

                DataBase.AddColumn("per_name", Name_TB.Text.Trim());
                DataBase.AddColumn("per_address", Address_TB.Text.Trim());
                DataBase.AddColumn("per_mobile", Mobile_TB.Text.Trim());
                DataBase.AddColumn("per_email", Email_TB.Text.Trim());

                if (LB.SelectedIndex == -1)
                {
                    if (DataBase.IsNotExist("per_id", "per_name", "per_mobile"))
                    {

                        if (DataBase.Insert())
                        {
                            return (Add_Update_Customer(DataBase.Last_Inserted));
                        }
                        else
                        {
                            return false;
                        }

                    }
                    else
                    {
                        Message.Show("هذا الاسم مسجل من فضلك اختار اسم اخر ", MessageBoxButton.OK, 10);
                        return false;
                    }

                }

// hena ye3ny hwa mawgod ba3mel edit
                else
                {
                    DataBase.AddCondition("per_id", ((DataRowView)LB.SelectedItem)["per_id"]);
                    if (DataBase.Update())
                    {
                        return (Add_Update_Customer(((DataRowView)LB.SelectedItem)["per_id"]));
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        private bool Add_Update_Customer(object person_idd)
        {

            try
            {
                DB DB = new DB("customer");
                DB.AddColumn("cust_per_id", person_idd);
                DB.AddColumn("cust_payment", Payment_CB.SelectedValue);
                DB.AddColumn("cust_type", Customer_Type);



                if (DB.IsNotExist("cust_id", "cust_per_id"))
                {
                    return Confirm.Check(DB.Insert());
                }
                else
                {
                    DB.AddCondition("cust_per_id", person_idd);
                    return Confirm.Check(DB.Update());
                }

            }
            catch
            {
                //MessageBox.Show("hena el error");
                return false;
            }
        }

        private void EditPanel_Add(object sender, EventArgs e)
        {
            try
            {
                App.Set_Style(Main_Grid, Operations.Add);
                Save_Panel.Visibility = System.Windows.Visibility.Visible;
                LB.IsEnabled = false;
                LB.SelectedIndex = -1;
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
                    App.Set_Style(Main_Grid, Operations.Edit);
                    Save_Panel.Visibility = System.Windows.Visibility.Visible;
                    LB.IsEnabled = false;
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
                    if (Message.Show("هل تريد حذف هذا الشخص", MessageBoxButton.YesNoCancel, 5) == MessageBoxResult.Yes)
                    {
                        DB db = new DB("persons");
                        db.AddCondition("per_id", ((DataRowView)LB.SelectedItem)["per_id"]);
                        if (db.Delete())
                        {
                            var log = new Log();
                            log.Columns.Add(new Column("الإسم", Name_TB.Text));
                            log.Columns.Add(new Column("الجوال", Mobile_TB.Text));
                            log.CreateLog("العملاء");

                            Fill_Customers_LB();
                        }
                    }
                }
            }
            catch
            {

            }
        }

        private void Name_Search_TB_TextChanged(object sender, TextChangedEventArgs e)
        {
            Fill_Customers_LB();
        }

        private void Save_Panel_Save(object sender, EventArgs e)
        {
            try
            {
                if (Notify.validate("من فضلك ادخل الاسم", Name_TB.Text, MainWindow.GetWindow(this)))
                {
                    return;
                }

                if (Notify.validate("من فضلك ادخل التعامل", Payment_CB.SelectedIndex, MainWindow.GetWindow(this)))
                {
                    return;
                }
                if (Add_Update())
                {
                    var log = new Log();
                    log.Columns.Add(new Column("الإسم", Name_TB.Text));
                    log.Columns.Add(new Column("الجوال", Mobile_TB.Text));
                    log.CreateLog("العملاء", LB.SelectedIndex == -1);

                    App.Set_Style(Main_Grid, Operations.View);

                    Save_Panel.Visibility = System.Windows.Visibility.Collapsed;
                    LB.IsEnabled = true;


                    int i = LB.SelectedIndex;
                    Fill_Customers_LB();
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
                App.Set_Style(Main_Grid, Operations.View);
                Save_Panel.Visibility = System.Windows.Visibility.Collapsed;
                LB.IsEnabled = true;
            }
            catch
            {

            }
        }

        private void From_DTP_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            try
            {
                if (this.IsLoaded)
                    Get_Customers_Accounts();
            }
            catch
            {

            }
        }

        private void EP_Edit(object sender, EventArgs e)
        {
            try
            {
                if ((LB.SelectedIndex != -1) && (Customers_In_DG.SelectedIndex != -1))
                {
                    Customer_Loans c = new Customer_Loans(LB.SelectedValue, ((DataRowView)Customers_In_DG.SelectedItem)["cstl_id"]);
                    c.ShowDialog();
                    Get_Customers_Accounts();
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
                if (Customers_In_DG.SelectedIndex != -1)
                {
                    if (Message.Show("هل تريد الحذف", MessageBoxButton.YesNoCancel, 10) == MessageBoxResult.Yes)
                    {
                        DB db = new DB("customer_loans");
                        var row = ((DataRowView)Customers_In_DG.SelectedItem);
                        db.AddCondition("cstl_id", row["cstl_id"]);
                        if (db.Delete())
                        {
                            var log = new Log();
                            log.Columns.Add(new Column("التاريـخ", row["cstl_date"]));
                            log.Columns.Add(new Column("القيـمــة", row["cstl_value"]));
                            log.Columns.Add(new Column("البيـــان", row["cstl_description"]));
                            log.Columns.Add(new Column("إسم العميل", ((DataRowView)LB.SelectedItem)["per_name"]));
                            log.CreateLog("دفعات العملاء");

                            Get_Customers_Accounts();
                        }
                    }
                }
            }
            catch
            {

            }
        }

        private void EP_Add(object sender, EventArgs e)
        {
            try
            {
                if (LB.SelectedIndex != -1)
                {
                    Customer_Loans c = new Customer_Loans(LB.SelectedValue);
                    c.ShowDialog();
                    Get_Customers_Accounts();
                }
            }
            catch
            {

            }
        }


        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                if (LB.SelectedIndex != -1)
                {
                    Places_prices pp = new Places_prices(((DataRowView)LB.SelectedItem)["per_id"], "Customers");
                    pp.ShowDialog();
                }
            }
            catch
            {

            }
        }

        private void LB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                Get_Customers_Accounts();
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
                print.header.Add(string.Format(" كشف حساب  {0} \r\n عن الفترة من {1} إلى {2}", ((DataRowView)LB.SelectedItem)["per_name"],
                    From_DTP.Value.Value.Date.ToString("yyyy/MM/dd"), To_DTP.Value.Value.Date.ToString("yyyy/MM/dd")));
                if (Customer_Type == Customer_type.مصنع)
                {
                    print.PrintDocument.DefaultPageSettings.Landscape = true;
                    App.Get_Printed_Table(print, Customers_Out_DG, Customers_In_DG, new string[] { "عدد الردود", "المدفوعات" });
                    print.ColumnsLine.Add("trs_paid");
                }
                else
                {
                    App.Get_Printed_Table(print, Customer_Out_DG, Customers_In_DG, new string[] { "المسحوبات", "المدفوعات" });
                    print.ColumnsLine.Add("sin_cost");
                }

                print.FooterTable.Add("المستحق سـابقاً : ", Balance_Before_TB.Text);
                print.FooterTable.Add("الإجمالــــــــــي : ", Total_TB.Text);
                print.FooterTable.Add("المــدفــــــــــوع : ", Paid_TB.Text);
                print.FooterTable.Add("البــــــــــــــاقـي : ", Rest_TB.Text);
                print.FooterTable.Add("المستحق حاليـاً : ", Balance_After_TB.Text);

                print.print();
            }
            catch
            {

            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            try
            {
                Future_Sales f = new Future_Sales(Future.Customers, Customer_Type);
                f.ShowDialog();
            }
            catch
            {

            }
        }

        private void Archive_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ChangeStatus_BTN.Content = Status_CB.SelectedIndex == 1 ? "أرشيف" : "تفعيل";
            ChangeStatus_BTN.Tag = Status_CB.SelectedIndex == 0 ? "/haies;component/Images/Activate.ico" : "/haies;component/Images/Archive.ico";

            Fill_Customers_LB();
        }

        private void ChangeStatus_BTN_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var status = Status_CB.SelectedIndex == 0 ? 1 : 0;
                DB db = new DB("persons");
                db.AddColumn("per_status", status);
                db.AddCondition("per_id", ((DataRowView)LB.SelectedItem)["per_id"]);
                Confirm.Check(db.Update());
                Fill_Customers_LB();
            }
            catch
            {


            }
        }

        private void Claims_Click(object sender, RoutedEventArgs e)
        {
            if (LB.SelectedIndex > -1)
            {
                var person = ((DataRowView)LB.SelectedItem);
                Claims clm = new Claims(new Claim() { Id = (int)LB.SelectedValue, Name = person["per_name"].ToString(), Type = Customer_Type, Email = person["per_Email"].ToString(), PersonId = person["per_id"] });
                clm.ShowDialog();
            }
            else
            {
                Message.Show("من فضلك أختار العميل أولا", MessageBoxButton.OK);
            }
        }

    }
}
