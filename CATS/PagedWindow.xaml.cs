using System;
using System.Windows;

namespace CATS
{
    /// <summary>
    /// Interaction logic for PagedWindow.xaml
    /// </summary>
    public partial class PagedWindow : Window
    {
        private bool isElevated;
        public string currentFilePath;
        BUAssessment currentBua;
        MainWindow callingWindow;
        int currentPageNumber = 0; //Page 0 is the first page

        public PagedWindow(MainWindow caller, BUAssessment bua, string buaFilePath, bool elevated)
        {
            InitializeComponent();
            callingWindow = caller;
            currentBua = bua;
            currentFilePath = buaFilePath;
            isElevated = elevated;
            this.Title = currentFilePath + " - CATS";
            navigatePage(currentPageNumber); //Initialise the frame by setting it to show the first page
        }

        /// <summary>
        /// Navigates to the specified page index and adjusts the UI to match the specified page
        /// </summary>
        /// <param name="pageNumber">The page number to navigate to</param>
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
                    this.WindowState = WindowState.Normal;
                    currentPageNumber = 0;
                    break;
                case 1:
                    mainFrame.Content = new WeightDatePage(currentBua);
                    prevBtn.Visibility = Visibility.Visible;
                    nextBtn.Visibility = Visibility.Visible;

                    this.Width = 770;
                    this.Height = 500;
                    this.WindowState = WindowState.Normal;
                    currentPageNumber = 1;
                    break;
                case 2:
                    mainFrame.Content = new StaffSubPage(currentBua);
                    prevBtn.Visibility = Visibility.Visible;
                    nextBtn.Visibility = Visibility.Visible;

                    this.Width = 770;
                    this.Height = 550;
                    this.WindowState = WindowState.Normal;
                    currentPageNumber = 2;
                    break;
                case 3:
                    mainFrame.Content = new WYSIWYGPage(this, currentBua, "Assessment Task");
                    prevBtn.Visibility = Visibility.Visible;
                    nextBtn.Visibility = Visibility.Visible;

                    this.Width = 1250;
                    this.Height = 700;
                    this.WindowState = WindowState.Maximized;
                    currentPageNumber = 3;
                    break;
                case 4:
                    mainFrame.Content = new WYSIWYGPage(this, currentBua, "Submission Format");
                    prevBtn.Visibility = Visibility.Visible;
                    nextBtn.Visibility = Visibility.Visible;

                    this.Width = 1250;
                    this.Height = 700;
                    this.WindowState = WindowState.Maximized;
                    currentPageNumber = 4;
                    break;
                case 5:
                    mainFrame.Content = new WYSIWYGPage(this, currentBua, "Marking Criteria");
                    prevBtn.Visibility = Visibility.Visible;
                    nextBtn.Visibility = Visibility.Visible;

                    this.Width = 1250;
                    this.Height = 700;
                    this.WindowState = WindowState.Maximized;
                    currentPageNumber = 5;
                    break;
                case 6:
                    mainFrame.Content = new ILOsPage(currentBua);
                    prevBtn.Visibility = Visibility.Visible;
                    nextBtn.Visibility = Visibility.Hidden; //Cannot go forward from the last page

                    this.Width = 770;
                    this.Height = 500;
                    this.WindowState = WindowState.Normal;
                    currentPageNumber = 6;
                    break;
                default:
                    Console.Error.WriteLine("ERROR: Invalid page specified");
                    break;
            }
        }

        #region Event handlers
        /// <summary>
        /// When the "previous" button is clicked
        /// Navigates to the previous page
        /// </summary>
        private void prevBtn_Click(object sender, RoutedEventArgs e)
        {
            currentBua.saveAsJson(currentFilePath);
            navigatePage(--currentPageNumber);
        }

        /// <summary>
        /// When the "next" button is clicked
        /// Navigates to the next page
        /// </summary>
        private void nextBtn_Click(object sender, RoutedEventArgs e)
        {
            currentBua.saveAsJson(currentFilePath);
            navigatePage(++currentPageNumber); //Go to the next page
        }

        /// <summary>
        /// When the File > Save button is clicked
        /// Saves changes
        /// </summary>
        private void FileSaveMi_Click(object sender, RoutedEventArgs e)
        {
            currentBua.saveAsJson(currentFilePath);
        }

        /// <summary>
        /// When the File > Close button is clicked
        /// Saves changes, then closes this window and returns to MainWindow
        /// </summary>
        private void FileCloseMi_Click(object sender, RoutedEventArgs e)
        {
            currentBua.saveAsJson(currentFilePath);
            callingWindow.Visibility = Visibility.Visible;
            this.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// When the File > Exit button is clicked
        /// Saves changes, then exits the application
        /// </summary>
        private void FileExitMi_Click(object sender, RoutedEventArgs e)
        {
            currentBua.saveAsJson(currentFilePath);
            Application.Current.Shutdown();
        }

        /// <summary>
        /// When the Options > About button is clicked
        /// </summary>
        private void OptionsAboutMi_Click(object sender, RoutedEventArgs e)
        {
            About about = new About();
            about.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// When the window is closed (e.g., "X" button)
        /// Do a save and complete shutdown (prevents processes from remaining open when they should have stopped)
        /// </summary>
        private void PagedWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            currentBua.saveAsJson(currentFilePath);
            Application.Current.Shutdown();
        }
        #endregion
    }
}
