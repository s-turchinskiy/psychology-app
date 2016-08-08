using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
    /// Interaction logic for FocusModificationWindow.xaml
    /// </summary>
    public partial class FocusModificationWindow : Page
    {
        int amountOfAttemps = 0;

        public FocusModificationWindow()
        {
            InitializeComponent();
        }

        private void Window_Initialized(object sender, EventArgs e)
        {



        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Image1.Source = new BitmapImage(new Uri(@"Resources\Image1.jpg", UriKind.Relative)) { CreateOptions = BitmapCreateOptions.IgnoreImageCache };
            Image2.Source = new BitmapImage(new Uri("/Resources/Image2.jpg", UriKind.Relative)) { CreateOptions = BitmapCreateOptions.IgnoreImageCache };
            Image3.Source = new BitmapImage(new Uri(@"Resources\Image3.jpg", UriKind.Relative)) { CreateOptions = BitmapCreateOptions.IgnoreImageCache };
            Image4.Source = new BitmapImage(new Uri("Resources/Image4.jpg", UriKind.Relative)) { CreateOptions = BitmapCreateOptions.IgnoreImageCache };
            Image5.Source = new BitmapImage(new Uri("Resources/Image5.jpg", UriKind.Relative)) { CreateOptions = BitmapCreateOptions.IgnoreImageCache };
            Image6.Source = new BitmapImage(new Uri("/Resources/Image6.jpg", UriKind.Relative)) { CreateOptions = BitmapCreateOptions.IgnoreImageCache };
            Image7.Source = new BitmapImage(new Uri("/Resources/Image7.jpg", UriKind.Relative)) { CreateOptions = BitmapCreateOptions.IgnoreImageCache };
            Image8.Source = new BitmapImage(new Uri("/Resources/Image8.jpg", UriKind.Relative)) { CreateOptions = BitmapCreateOptions.IgnoreImageCache };
            Image9.Source = new BitmapImage(new Uri("/Resources/Image9.jpg", UriKind.Relative)) { CreateOptions = BitmapCreateOptions.IgnoreImageCache };

        }

        private void ButtonImage_Click(object sender, RoutedEventArgs e)
        {

            amountOfAttemps = amountOfAttemps + 1;

            if (sender != ButtonImage8)
            {
                return;
            }

            PreliminaryResultWindow w1 = new PreliminaryResultWindow();
            w1.Percent = 100 / amountOfAttemps;
            w1.NextWindow = "WordsSearchDescriptionWindow";
            MainWindow _mainWindow = (MainWindow)Window.GetWindow(this);
            _mainWindow.Frame.Navigate(w1);
        }
    }
}
