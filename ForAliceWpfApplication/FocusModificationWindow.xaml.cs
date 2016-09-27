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
        int amountWindow = 0;
        int positiveImageNumberForm;
        Button positiveButton;

        public FocusModificationWindow()
        {
            InitializeComponent();
        }


        private int GetNumber(int maxSize)
        {
            Random rnd = new Random();
            return rnd.Next(1, maxSize);
        }

        private int[] GetNegativeNumbersImages() 
        {
            Random rnd = new Random();
            int[] negativeNumbersImages = new int[8];
            int currentNumber = 0;
            int nextValue;
            while (true)
            {
                nextValue = rnd.Next(1, 18);
                while (true)
                {
                    if (negativeNumbersImages.Where(a => a == nextValue).Count() == 0)
                    {
                        break;
                    }
                    else
                    {
                        nextValue = nextValue + 1;
                    }
                }

                negativeNumbersImages[currentNumber] = nextValue;
                currentNumber++;
                if (currentNumber == 8)
                {
                    break;
                }
            }

            return negativeNumbersImages;
        }

        private void SetSourseImage(bool positivePicture, int imageNumberForm, int imageNumberFolder)
        {
            string formPath = "Image" + imageNumberForm.ToString();
            string folderPath = "Resources" + ((positivePicture) ? "/Positive pictures/" : "/Negative pictures/") + "Image" + imageNumberFolder.ToString() + ".jpg";

            ((Image)(Form.FindName(formPath))).Source = new BitmapImage(new Uri(folderPath, UriKind.Relative)) 
            { CreateOptions = BitmapCreateOptions.IgnoreImageCache };

            //((Image)(Form.FindName("Image1"))).Source = new BitmapImage(new Uri(@"Resources\Image1.jpg", UriKind.Relative)) { CreateOptions = BitmapCreateOptions.IgnoreImageCache };
            
        }

        private void SetImage()
        {
            positiveImageNumberForm = GetNumber(9);
            int positiveImageNumberFolder = GetNumber(15);
            SetSourseImage(true, positiveImageNumberForm, positiveImageNumberFolder);
            int[] negativeNumbersImages = GetNegativeNumbersImages();

            int currentNumber = 0;
            for (int i = 1; i <= 9; i++)
            {
                if (i == positiveImageNumberForm)
                {
                    continue;
                }

                SetSourseImage(false, i, negativeNumbersImages[currentNumber]);
                currentNumber++;
            }

            string formPath = "ButtonImage" + positiveImageNumberForm.ToString();
            positiveButton = ((Button)(Form.FindName(formPath)));
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SetImage();
            amountWindow++;
        }

        private void ButtonImage_Click(object sender, RoutedEventArgs e)
        {

            amountOfAttemps = amountOfAttemps + 1;

            if (sender != positiveButton)
            {
                return;
            }

            amountWindow++;
            SetImage();

            if (amountWindow!=11)
            {
                return;
            }

            PreliminaryResultWindow w1 = new PreliminaryResultWindow();
            w1.Percent = 100*10 / amountOfAttemps;
            w1.NextWindow = "WordsSearchDescriptionWindow";
            MainWindow _mainWindow = (MainWindow)Window.GetWindow(this);
            _mainWindow.Frame.Navigate(w1);
        }
    }
}
