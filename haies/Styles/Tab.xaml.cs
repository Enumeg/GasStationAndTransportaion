using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
namespace haies.Styles
{
    partial class Tab : ResourceDictionary
    {
        public Tab()
        {
            InitializeComponent();
        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var f = (TabItem)((FrameworkElement)sender).TemplatedParent;
                ((TabControl)f.Parent).Items.Remove(f);
              
            }
            catch
            {

            }
        }

    }
}
