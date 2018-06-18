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

namespace haies
{
    /// <summary>
    /// Interaction logic for Station.xaml
    /// </summary>
    public partial class Station : Window
    {
        public static Operations Mode = Operations.Add;
        List<Page> Pages;       
        PumpRead Pump_read_Page;
        Station_Income Station_Income_Page;
        Station_Outcome Station_Outcome_Page;
        Station_Purchases Station_Purchases_Page;
        Station_Accounts Station_Accounts_Page;
        Customer_Pay Customer_Pay_Page;
        Bank Bank_Page;
        public Station(Page Start_Page = null, Operations mode = Operations.Add)
        {
            InitializeComponent();
            Mode = mode;
            if (mode == Operations.Add)
            {
                
                Pump_read_Page = new PumpRead();
                Station_Income_Page = new Station_Income();
                Station_Outcome_Page = new Station_Outcome();
                Station_Purchases_Page = new Station_Purchases();
                Station_Accounts_Page = new Station_Accounts();
                Customer_Pay_Page = new Customer_Pay();
                Bank_Page = new Bank();
                Pages = new List<Page>();
                Pages.Add(Pump_read_Page); Pages.Add(Station_Income_Page); Pages.Add(Station_Purchases_Page); Pages.Add(Customer_Pay_Page);Pages.Add(Station_Outcome_Page);
                Pages.Add(Station_Accounts_Page); Pages.Add(Bank_Page); 
                frame.Navigate(Pump_read_Page);
                this.Title = "Pump read - قراءة العدادات";
            }
            else
            {
                Main_GD.RowDefinitions[0].Height = new GridLength(0);
                frame.Navigate(Start_Page);
            }

        }
        private void Next_BTN_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                frame.Navigate(Pages[Pages.IndexOf((Page)frame.Content) + 1]);               
            }
            catch
            {

            }
        }
        private void Pervious_BTN_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                frame.Navigate(Pages[Pages.IndexOf((Page)frame.Content) - 1]);     

            }
            catch
            {

            }
        }
        private void frame_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            try
            {
                this.Title = ((Page)frame.Content).Title;
            }
            catch
            {

            }
        }
        private void Window_Closed(object sender, EventArgs e)
        {
            //Application.Current.Shutdown();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                frame.Navigate(Pump_read_Page);
            }
            catch
            {

            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            try
            {
                frame.Navigate(new Customer_En(Customer_type.محطة));
            }
            catch
            {

            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            try
            {
                frame.Navigate(new Gas_Page_EN());
            }
            catch
            {

            }
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            try
            {
                Application.Current.Shutdown();
            }
            catch
            {

            }
        }
    }
}
