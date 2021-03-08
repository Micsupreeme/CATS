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

namespace CATS
{
    /// <summary>
    /// Interaction logic for PagedWindow.xaml
    /// </summary>
    public partial class PagedWindow : Window
    {
        int currentPageNumber = 0; //Page 0 is the first page

        public PagedWindow()
        {
            InitializeComponent();
            navigatePage(currentPageNumber); //Initialise the frame by setting it to show the first page
        }

        /*
         * Navigates to the specified page index
         * and adjusts the UI to match the specified page
         */
        private void navigatePage(int pageNumber)
        {
            switch (pageNumber)
            {
                case 0:
                    mainFrame.Content = new TitleLevelPage();
                    prevBtn.Visibility = Visibility.Hidden; //Cannot go back from the first page
                    nextBtn.Visibility = Visibility.Visible;
                    currentPageNumber = 0;
                    break;
                case 1:
                    mainFrame.Content = new WeightDatePage();
                    prevBtn.Visibility = Visibility.Visible;
                    nextBtn.Visibility = Visibility.Visible;
                    currentPageNumber = 1;
                    break;
                case 2:
                    mainFrame.Content = new StaffSubPage();
                    prevBtn.Visibility = Visibility.Visible;
                    nextBtn.Visibility = Visibility.Hidden; //Cannot go forward from the last page
                    currentPageNumber = 2;
                    break;
                default:
                    Console.Error.WriteLine("ERROR: Invalid page specified");
                    break;
            }
        }

        private void prevBtn_Click(object sender, RoutedEventArgs e)
        {
            navigatePage(--currentPageNumber); //Go to the previous page
        }

        private void nextBtn_Click(object sender, RoutedEventArgs e)
        {
            navigatePage(++currentPageNumber); //Go to the next page
        }
    }
}
