﻿using System;
using System.IO;
using System.Windows;

namespace CATS
{
    /// <summary>
    /// Interaction logic for PagedWindow.xaml
    /// </summary>
    public partial class PagedWindow : Window
    {
        private bool currentElevation;
        public string currentFilePath;
        BUAssessment currentBua;
        MainWindow callingWindow;
        int currentPageNumber = 0; //Page 0 is the first page

        public PagedWindow(MainWindow caller, BUAssessment bua, string buaFilePath, bool elevate)
        {
            InitializeComponent();
            callingWindow = caller;
            currentBua = bua;
            currentFilePath = buaFilePath;
            currentElevation = elevate;
            this.Title = Path.GetFileName(currentFilePath) + " - CATS";
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

                    this.MinWidth = 770;
                    this.MinHeight = 500;
                    this.Width = 770;
                    this.Height = 500;
                    this.WindowState = WindowState.Normal;
                    recenterWindow();
                    currentPageNumber = 0;
                    break;
                case 1:
                    mainFrame.Content = new WeightDatePage(currentBua, currentElevation);
                    prevBtn.Visibility = Visibility.Visible;
                    nextBtn.Visibility = Visibility.Visible;

                    this.MinWidth = 770;
                    this.MinHeight = 500;
                    this.Width = 770;
                    this.Height = 500;
                    this.WindowState = WindowState.Normal;
                    recenterWindow();
                    currentPageNumber = 1;
                    break;
                case 2:
                    mainFrame.Content = new StaffSubPage(currentBua);
                    prevBtn.Visibility = Visibility.Visible;
                    nextBtn.Visibility = Visibility.Visible;

                    this.MinWidth = 770;
                    this.MinHeight = 550;
                    this.Width = 770;
                    this.Height = 550;
                    this.WindowState = WindowState.Normal;
                    recenterWindow();
                    currentPageNumber = 2;
                    break;
                case 3:
                    mainFrame.Content = new WYSIWYGPage(this, currentBua, "Assessment Task");
                    prevBtn.Visibility = Visibility.Visible;
                    nextBtn.Visibility = Visibility.Visible;

                    this.MinWidth = 1250;
                    this.MinHeight = 750;
                    this.Width = 1250;
                    this.Height = 750;
                    this.WindowState = WindowState.Maximized;
                    currentPageNumber = 3;
                    break;
                case 4:
                    mainFrame.Content = new WYSIWYGPage(this, currentBua, "Submission Format");
                    prevBtn.Visibility = Visibility.Visible;
                    nextBtn.Visibility = Visibility.Visible;

                    this.MinWidth = 1250;
                    this.MinHeight = 750;
                    this.Width = 1250;
                    this.Height = 750;
                    this.WindowState = WindowState.Maximized;
                    currentPageNumber = 4;
                    break;
                case 5:
                    mainFrame.Content = new WYSIWYGPage(this, currentBua, "Marking Criteria");
                    prevBtn.Visibility = Visibility.Visible;
                    nextBtn.Visibility = Visibility.Visible;

                    this.MinWidth = 1250;
                    this.MinHeight = 750;
                    this.Width = 1250;
                    this.Height = 750;
                    this.WindowState = WindowState.Maximized;
                    currentPageNumber = 5;
                    break;
                case 6:
                    mainFrame.Content = new ILOsPage(currentBua);
                    prevBtn.Visibility = Visibility.Visible;
                    nextBtn.Visibility = Visibility.Visible;

                    this.MinWidth = 770;
                    this.MinHeight = 550;
                    this.Width = 770;
                    this.Height = 550;
                    this.WindowState = WindowState.Normal;
                    recenterWindow();
                    currentPageNumber = 6;
                    break;
                case 7:
                    mainFrame.Content = new QuestionsSignaturePage(currentBua);
                    prevBtn.Visibility = Visibility.Visible;
                    nextBtn.Visibility = Visibility.Visible;

                    this.MinWidth = 770;
                    this.MinHeight = 500;
                    this.Width = 770;
                    this.Height = 500;
                    this.WindowState = WindowState.Normal;
                    recenterWindow();
                    currentPageNumber = 7;
                    break;
                case 8:
                    mainFrame.Content = new ExportPage(currentBua, currentFilePath, currentElevation);
                    prevBtn.Visibility = Visibility.Visible;
                    nextBtn.Visibility = Visibility.Hidden; //Cannot go forward from the last page

                    this.MinWidth = 770;
                    this.Width = 770;

                    //Evelated-exclusive controls require more height
                    if (currentElevation) {
                        this.MinHeight = 755;
                        this.Height = 755;
                    } else {
                        this.MinHeight = 690;
                        this.Height = 690;
                    }
                    this.WindowState = WindowState.Normal;
                    recenterWindow();
                    currentPageNumber = 8;
                    break;
                default:
                    Console.Error.WriteLine("ERROR: Invalid page specified");
                    break;
            }
        }

        /// <summary>
        /// Re-center the window by determining the ideal position from the current width and height
        /// NOTE: The window changes size a lot during the app, and when the height increases, the window is not recentered automatically
        /// </summary>
        private void recenterWindow()
        {
            this.Left = (SystemParameters.PrimaryScreenWidth / 2) - (this.Width / 2);
            this.Top = (SystemParameters.PrimaryScreenHeight / 2) - (this.Height / 2);
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
