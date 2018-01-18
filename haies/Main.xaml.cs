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
    /// Interaction logic for Main.xaml
    /// </summary>
    public partial class Main : Window
    {
        public Main()
        {
            InitializeComponent();
        }

        private void Cement_BTN_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //Cement_Office c = new Cement_Office();
                //c.ShowDialog();
                Open_window(Pages.Cement);
            }
            catch
            {

            }
        }

        private void Transp_BTN_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Open_window(Pages.Transportations);
            }
            catch
            {

            }
        }

        private void Statio_BTN_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Open_window(Pages.Station);
            }
            catch
            {

            }
        }

        private void Employ_BTN_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Open_window(Pages.Employees);
            }
            catch
            {

            }
        }

        private void Userss_BTN_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Open_window(Pages.Users);
            }
            catch
            {

            }
        }

        private void Open_window(Pages page)
        {
            try
            {
                MainWindow m = new MainWindow(page);
                this.Hide();
                m.ShowDialog();
                this.ShowDialog();
            }
            catch
            {

            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
          //  Application.Current.Shutdown();
        }

        private void Sta_en_BTN_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Station s = new Station();
                this.Hide();
                s.ShowDialog();
                this.ShowDialog();
            }
            catch
            {

            }
        }

        private void Cem_en_BTN_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Cement_Office c = new Cement_Office();
                this.Hide();
                c.ShowDialog();
                this.ShowDialog();
            }
            catch
            {

            }
        }

        private void Exit_l_BTN_Click(object sender, RoutedEventArgs e)
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
