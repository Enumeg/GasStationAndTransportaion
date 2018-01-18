using System.Windows;
using System.Windows.Input;
namespace haies.Styles
{
    partial class Window : ResourceDictionary
    {
        public Window()
        {
            InitializeComponent();
        }
        #region Header
        private void Button_Click(object sender, RoutedEventArgs e)
        
        {
            try
            {
                var f = (System.Windows.Window)((FrameworkElement)sender).TemplatedParent;
                f.Close();
            }
            catch
            {

            }

        }
        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var f = (System.Windows.Window)((FrameworkElement)sender).TemplatedParent;
                f.DragMove();
            }
            catch
            {

            }
        }
        #endregion

    }
}
