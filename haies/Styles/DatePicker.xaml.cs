using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Windows.Controls;
namespace haies.Styles
{
    partial class DatePicker : ResourceDictionary
    {
        public DatePicker()
        {
            InitializeComponent();
        }
        private void TB_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                var f = (System.Windows.Controls.DatePicker)((FrameworkElement)sender).TemplatedParent;
                f.SelectedDate = System.DateTime.Parse(f.Text, new System.Globalization.CultureInfo("ar-eg"));
            }
            catch
            {

            }

        }
        private void TB_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    var f = (System.Windows.Controls.DatePicker)((FrameworkElement)sender).TemplatedParent;
                    f.SelectedDate = System.DateTime.Parse(f.Text, new System.Globalization.CultureInfo("ar-eg"));
                }
            }
            catch
            {

            }

        }
        private void Part_Calendar_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var f = (DateTimePicker)((FrameworkElement)sender).TemplatedParent;
                f.IsOpen = false;
            }
            catch
            {

            }
        }

    }
}
