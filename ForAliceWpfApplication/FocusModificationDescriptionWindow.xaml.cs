using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ForAliceWpfApplication
{
    /// <summary>
    /// Interaction logic for FocusModificationDescriptionWindow.xaml
    /// </summary>
    public partial class FocusModificationDescriptionWindow : Window
    {
        public FocusModificationDescriptionWindow()
        {
            InitializeComponent();
        }

        private void ButtonOnward_Click(object sender, RoutedEventArgs e)
        {
            FocusModificationWindow w1 = new FocusModificationWindow();
            w1.WindowStartupLocation = System.Windows.WindowStartupLocation.Manual;
            w1.Top = this.Top;
            w1.Left = this.Left;
            w1.Height = this.Height;
            w1.Width = this.Width;
            w1.Show();
            this.Close();
        }
    }
}
