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
using System.Diagnostics;

namespace CATS
{
    /// <summary>
    /// Interaction logic for PagedWindow.xaml
    /// </summary>
    public partial class PagedWindow : Window
    {
        BUAssessment currentBua;
        string currentFilePath;
        int currentPageNumber = 0; //Page 0 is the first page

        public PagedWindow(BUAssessment bua, string buaFilePath)
        {
            InitializeComponent();
            currentBua = bua;
            currentFilePath = buaFilePath;
            this.Title = currentFilePath + " - CATS";
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
                    mainFrame.Content = new TitleLevelPage(currentBua);
                    prevBtn.Visibility = Visibility.Hidden; //Cannot go back from the first page
                    nextBtn.Visibility = Visibility.Visible;

                    this.Width = 770;
                    this.Height = 500;
                    currentPageNumber = 0;
                    break;
                case 1:
                    mainFrame.Content = new WeightDatePage(currentBua);
                    prevBtn.Visibility = Visibility.Visible;
                    nextBtn.Visibility = Visibility.Visible;

                    this.Width = 770;
                    this.Height = 500;
                    currentPageNumber = 1;
                    break;
                case 2:
                    mainFrame.Content = new StaffSubPage(currentBua);
                    prevBtn.Visibility = Visibility.Visible;
                    nextBtn.Visibility = Visibility.Visible;

                    this.Width = 770;
                    this.Height = 500;
                    currentPageNumber = 2;
                    break;
                case 3:
                    mainFrame.Content = new HtmlPage(this, currentBua, "Assessment Task");
                    prevBtn.Visibility = Visibility.Visible;
                    nextBtn.Visibility = Visibility.Visible;

                    this.Width = 1150;
                    this.Height = 580;
                    currentPageNumber = 3;
                    break;
                case 4:
                    mainFrame.Content = new HtmlPage(this, currentBua, "Submission Format");
                    prevBtn.Visibility = Visibility.Visible;
                    nextBtn.Visibility = Visibility.Visible;

                    this.Width = 1150;
                    this.Height = 580;
                    currentPageNumber = 4;
                    break;
                case 5:
                    mainFrame.Content = new HtmlPage(this, currentBua, "Marking Criteria");
                    prevBtn.Visibility = Visibility.Visible;
                    nextBtn.Visibility = Visibility.Hidden; //Cannot go forward from the last page

                    this.Width = 1150;
                    this.Height = 580;
                    currentPageNumber = 5;
                    break;
                default:
                    Console.Error.WriteLine("ERROR: Invalid page specified");
                    break;
            }
        }

        private void prevBtn_Click(object sender, RoutedEventArgs e)
        {
            currentBua.saveAsJson(currentFilePath);
            navigatePage(--currentPageNumber); //Go to the previous page
        }

        private void nextBtn_Click(object sender, RoutedEventArgs e)
        {
            currentBua.saveAsJson(currentFilePath);
            navigatePage(++currentPageNumber); //Go to the next page
        }
    }
}
