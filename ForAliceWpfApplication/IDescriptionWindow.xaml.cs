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
    /// Interaction logic for IDescriptionWindow.xaml
    /// </summary>
    public partial class IDescriptionWindow : Page
    {
        public IDescriptionWindow()
        {
            InitializeComponent();
        }

        private void ButtonOnward_Click(object sender, RoutedEventArgs e)
        {
            MainWindow _mainWindow = (MainWindow)Window.GetWindow(this);
            _mainWindow.Frame.Navigate(new IDataFillingWindow());
        } 
    }
}
