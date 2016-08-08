using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Interaction logic for FocusModificationResultWindow.xaml
    /// </summary>
    public partial class PreliminaryResultWindow : Page
    {

        private int _percent;
        public int Percent
        {
            get { return _percent; }
            set { _percent = value; }
        }

        private String _nextWindow;
        public String NextWindow
        {
            get { return _nextWindow; }
            set { _nextWindow = value; }
        }


        public PreliminaryResultWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void ButtonOnward_Click(object sender, RoutedEventArgs e)
        {
            MainWindow _mainWindow = (MainWindow)Window.GetWindow(this);
            _mainWindow.Frame.NavigationService.Navigate(new Uri(NextWindow + ".xaml", UriKind.Relative));
        }
    }
}
