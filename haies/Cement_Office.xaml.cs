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
    /// Interaction logic for Cement_Office.xaml
    /// </summary>
    public partial class Cement_Office : Window
    {
        Receipt Receipt_Page = new Receipt();
        Transprtation Transprtation_Page = new Transprtation();
        Office_Account Office_Account_Page = new Office_Account();
        Add_Transportation Add_Transportation_Page = new Add_Transportation();
        Clients Clients_Page = new Clients(); 
        public Cement_Office(Page page = null)
        {
            InitializeComponent();
            if (page == null)
            {
                frame.Navigate(Receipt_Page);
            }
            else
            {
                frame.Navigate(page);
                Main_GD.RowDefinitions[0].Height = new GridLength(0);
            }
            btn.Focus();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                frame.Navigate(Receipt_Page);
            }
            catch
            {

            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            try
            {
                frame.Navigate(Transprtation_Page);
            }
            catch
            {

            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            try
            {
                frame.Navigate(Office_Account_Page);
            }
            catch
            {

            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
           // Application.Current.Shutdown();
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            try
            {
                frame.Navigate(Add_Transportation_Page);
            }
            catch
            {

            }
        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            try
            {
                frame.Navigate(Clients_Page);
            }
            catch
            {

            }
        }
    }
}
