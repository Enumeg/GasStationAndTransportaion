using System.Windows;
using System;
using System.Windows.Controls;

namespace haies
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Pages
        Cement Cement_Page = new Cement();
        Factory_Page Factory_Page = new Factory_Page();
        Customer Factroy_Customer_Page = new Customer(Customer_type.مصنع);
        Receipts Receipts_Page = new Receipts();
        Car_Owners Car_Owner_Page = new Car_Owners();
        Drivers Drivers_Page = new Drivers();
        Cars Cars_Page = new Cars();
        Transportations Transportations_Page = new Transportations();
        Station_Accounts_View Station_Accounts_View = new Station_Accounts_View();
        Customer Customer_Station_Page = new Customer(Customer_type.محطة);
        Gas_Page Gas_Page = new Gas_Page();
        Pumps Pumps_Page = new Pumps();
        Gas_Prices Gas_Price_Page = new Gas_Prices();
        Clients Clients_Page = new Clients();
        Employees Employees_Page = new Employees();
        Salaries Salaries_Page = new Salaries();
        Archive Archive_Page = new Archive();
        Transactions Safe_Page = new Transactions();        
        UserLogs UserLogs_Page = new UserLogs();
        Users Users_Window = new Users();
        #endregion

        public MainWindow(Pages page)
        {
            InitializeComponent();
            try
            {

                switch (page)
                {
                    case Pages.Cement:
                        Main_GD.RowDefinitions[0].Height = new GridLength(1, GridUnitType.Star);
                        Receipts_BTN.Focus();
                        frame.Navigate(Receipts_Page);
                        break;
                    case Pages.Employees:
                        Main_GD.RowDefinitions[3].Height = new GridLength(1, GridUnitType.Star);
                        Salaries_BTN.Focus();
                        frame.Navigate(Salaries_Page);
                        break;
                    case Pages.Station:
                        Main_GD.RowDefinitions[2].Height = new GridLength(1, GridUnitType.Star);
                        Station_Accounts_BTN.Focus();
                        frame.Navigate(Station_Accounts_View);
                        break;
                    case Pages.Transportations:
                        Main_GD.RowDefinitions[1].Height = new GridLength(1, GridUnitType.Star);
                        Cars_BTN.Focus();
                        frame.Navigate(Cars_Page);
                        break;
                    case Pages.Users:
                         Main_GD.RowDefinitions[4].Height = new GridLength(1, GridUnitType.Star);
                        UserLogs_Page.Focus();
                        frame.Navigate(UserLogs_Page);
                        break;
                }
            }
            catch
            {

            }

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Main_GD.RowDefinitions[0].Height != new GridLength(0))
                {
                    decimal[] Values = new decimal[2];
                    Values[0] = Factory_Page.Get_Balance(DateTime.Now);
                    Values[1] = Factory_Page.Get_Least();
                    if (Values[0] == Values[1])
                    {
                        Source.Message.Show("لقد بلغ رصيدك بالمصنع الحد الأدنى", MessageBoxButton.OK);
                    }
                    else if (Values[0] < Values[1])
                    {
                        Source.Message.Show("لقد تجاوزت الحد الأدنى لرصيدك بالمصنع", MessageBoxButton.OK);
                    }
                }
            }
            catch
            {

            }

        }

        private void Cement_BTN_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                frame.Navigate(Cement_Page);
            }
            catch
            {

            }
        }

        private void Factory_BTN_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                frame.Navigate(Factory_Page);
            }
            catch
            {

            }
        }

        private void Customers_BTN_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                frame.Navigate(Factroy_Customer_Page);
            }
            catch
            {

            }
        }

        private void Receipts_BTN_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                frame.Navigate(Receipts_Page);
            }
            catch
            {

            }
        }

        private void Owners_BTN_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                frame.Navigate(Car_Owner_Page);
            }
            catch
            {

            }
        }

        private void Drivers_BTN_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                frame.Navigate(Drivers_Page);
            }
            catch
            {

            }
        }

        private void Cars_BTN_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                frame.Navigate(Cars_Page);
            }
            catch
            {

            }
        }

        private void Refresh_BTN_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                #region Pages
                Cement_Page = new Cement();
                Factory_Page = new Factory_Page();
                Factroy_Customer_Page = new Customer(Customer_type.مصنع);
                Receipts_Page = new Receipts();
                Car_Owner_Page = new Car_Owners();
                Drivers_Page = new Drivers();
                Cars_Page = new Cars();
                Transportations_Page = new Transportations();
                Station_Accounts_View = new Station_Accounts_View();
                Customer_Station_Page = new Customer(Customer_type.محطة);
                Gas_Page = new Gas_Page();
                Pumps_Page = new Pumps();
                #endregion

            }
            catch
            {

            }
        }

        private void Pursh_BTN_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                frame.Navigate(Station_Accounts_View);
            }
            catch
            {

            }
        }

        private void Gas_BTN_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                frame.Navigate(Gas_Page);
            }
            catch
            {

            }
        }
        private void Pumps_BTN_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                frame.Navigate(Pumps_Page);
            }
            catch
            {

            }
        }

        private void Station_Customers_BTN_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                frame.Navigate(Customer_Station_Page);
            }
            catch
            {

            }
        }

        private void Transpotations_BTN_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                frame.Navigate(Transportations_Page);
            }
            catch
            {

            }
        }

        private void Gas_Price_BTN_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                frame.Navigate(Gas_Price_Page);
            }
            catch
            {

            }
        }

        private void Clients_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                frame.Navigate(Clients_Page);
            }
            catch
            {

            }
        }

        private void Home_Pa_BTN_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Close();
            }
            catch
            {

            }
        }

        private void Exit_Al_BTN_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Application.Current.Shutdown();
            }
            catch
            {

            }
        }

        private void Employ_BTN_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                frame.Navigate(Employees_Page);
            }
            catch
            {

            }
        }

        private void Salary_BTN_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                frame.Navigate(Salaries_Page);
            }
            catch
            {

            }
        }

        private void Suplliers_BTN_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                frame.Navigate(null);
                new Suppliers().ShowDialog();
            }
            catch
            {

            }
        }

        private void Archive_BTN_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                frame.Navigate(Archive_Page);
            }
            catch
            {

            }
        }

        private void Safe_BTN_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                frame.Navigate(Safe_Page);
            }
            catch
            {
                
            }
        }

      

        private void Users_Log_BTN_Click(object sender, RoutedEventArgs e)
        {
            frame.Navigate(UserLogs_Page);
        }

        private void Users_BTN_Click(object sender, RoutedEventArgs e)
        {
            frame.Navigate(null);
            Users_Window.ShowDialog();
            
        }


       
    }
}
