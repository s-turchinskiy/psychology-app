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
    /// Interaction logic for WordsSearchWindow.xaml
    /// </summary>
    public partial class WordsSearchWindow : Page
    {
        public WordsSearchWindow()
        {
            InitializeComponent();
        }

        private void ButtonOnward_Click(object sender, RoutedEventArgs e)
        {
            //FeelWords mm = new FeelWords();
            //mm.DoWork();

            PreliminaryResultWindow w1 = new PreliminaryResultWindow();
            w1.Percent = 100 / 10;
            w1.NextWindow = "IDescriptionWindow";
            MainWindow _mainWindow = (MainWindow)Window.GetWindow(this);
            _mainWindow.Frame.Navigate(w1);
        } 
    }
}
