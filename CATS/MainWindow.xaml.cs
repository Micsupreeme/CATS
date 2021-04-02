using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Media;

namespace CATS
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string DEFAULT_SAVE_FOLDER = "BU CW Assignments";
        private const string BUA_FILE_FILTER = 
            "BU Assessments (*.bua)|*.bua|" + 
            "All Files (*.*)|*.*";

        private const string DEFAULT_DRAG_AND_DROP_STRING = "...or drag your .bua file here";
        private const string MULTIPLE_DRAG_AND_DROP_STRING = "Cannot open multiple files at once!";
        private const string NONBUA_DRAG_AND_DROP_STRING = "Can only open \".bua\" files!";

        private bool hasSuccessfullyAuthenticated = false;

        public MainWindow()
        {           
            InitializeComponent();
        }

        /// <summary>
        /// Brings up the new brief dialog for selecting a new filepath
        /// </summary>
        private void createNewBrief()
        {
            NewBriefDialog newbriefdialog = new NewBriefDialog(this, hasSuccessfullyAuthenticated);
            newbriefdialog.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Brings up an openfiledialog for directly selecting an existing file to edit
        /// </summary>
        private void browseExistingBrief()
        {
            OpenFileDialog ofd = new OpenFileDialog()
            {
                Filter = BUA_FILE_FILTER,
                InitialDirectory = getDefaultDocsPath()
            };
            if (ofd.ShowDialog() == true)
            {
                openExistingBrief(ofd.FileName);
            }
        }

        /// <summary>
        /// Opens the specified .bua file for viewing/editing
        /// </summary>
        /// <param name="buaFilePath">The full filepath to the .bua file to open for viewing/editing</param>
        private void openExistingBrief(string buaFilePath)
        {
            BUAssessment openedBua = new BUAssessment();
            openedBua.loadFromJson(buaFilePath);

            PagedWindow pw = new PagedWindow(this, openedBua, buaFilePath, hasSuccessfullyAuthenticated);
            pw.Visibility = Visibility.Visible;
            this.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Determines whether or not a specified filepath ends with a ".bua" extension
        /// </summary>
        /// <param name="buaFilePath">The full filepath to the file to check its extension</param>
        /// <returns>True if the extension is ".bua", false otherwise</returns>
        private bool hasBuaExtension(string filePath)
        {
            if (filePath.Substring(filePath.Length - 4).Equals(".bua")) {
                return true;
            } else {
                return false;
            }
        }

        /// <summary>
        /// Creates the default save folder if it doesn't already exist, and returns its name
        /// </summary>
        /// <returns>The name of the app's default save folder</returns>
        private string getDefaultDocsPath()
        {
            if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\" + DEFAULT_SAVE_FOLDER))
            {
                //Directory doesn't exist, create it before returning
                Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\" + DEFAULT_SAVE_FOLDER);
            }
            return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\" + DEFAULT_SAVE_FOLDER;
        }

        /// <summary>
        /// Activates elevated user mode if indicated by the boolean input
        /// </summary>
        /// <param name="doElevate">True to elevate the user, false otherwise</param>
        public void toggleAuthenticated(bool doElevate)
        {
            if(doElevate) {
                hasSuccessfullyAuthenticated = true;
                Console.WriteLine("Elevated!");
            }
        }

        #region Drag-and-Drop event handlers
        /// <summary>
        /// When the mouse enters the control while dragging an item
        /// </summary>
        private void openDragAndDrop_DragEnter(object sender, DragEventArgs e)
        {
            openDragAndDropTb.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));
            openDragAndDropTb.Text = DEFAULT_DRAG_AND_DROP_STRING;
            openDragAndDropRect.Opacity = 1;
            openDragAndDropTb.Opacity = 1;
        }

        /// <summary>
        /// When the mouse leaves the control while dragging an item
        /// </summary>
        private void openDragAndDrop_DragLeave(object sender, DragEventArgs e)
        {
            if(openDragAndDropTb.Text.Equals(DEFAULT_DRAG_AND_DROP_STRING)) {       //Error text is not dimmed, unlike the default text
                openDragAndDropRect.Opacity = 0.5;
                openDragAndDropTb.Opacity = 0.5;
            }
        }

        /// <summary>
        /// When a dragged item is dropped into the control
        /// </summary>
        private void openDragAndDrop_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) {                      //If the item is a file (or multiple files)...
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                if(files.Length == 1) {                                             //If there is only 1 file... 
                    if(hasBuaExtension(files[0])) {                                 //If it's a ".bua" file...
                        openExistingBrief(files[0]);
                    } else {
                        Console.WriteLine("WARN: attempted to drop a non-bua file");
                        openDragAndDropTb.Foreground = new SolidColorBrush(Color.FromRgb(255, 0, 0));
                        openDragAndDropTb.Text = NONBUA_DRAG_AND_DROP_STRING;
                    }                  
                } else {
                    Console.WriteLine("WARN: attempted to drop multiple files");
                    openDragAndDropTb.Foreground = new SolidColorBrush(Color.FromRgb(255, 0, 0));
                    openDragAndDropTb.Text = MULTIPLE_DRAG_AND_DROP_STRING;
                }
            }

            if (openDragAndDropTb.Text.Equals(DEFAULT_DRAG_AND_DROP_STRING)) {      //Error text is not dimmed, unlike the default text
                openDragAndDropRect.Opacity = 0.5;
                openDragAndDropTb.Opacity = 0.5;
            }
        }
        #endregion

        #region Event handlers
        /// <summary>
        /// When the main create button is clicked
        /// </summary>
        private void createBtn_Click(object sender, RoutedEventArgs e)
        {
            createNewBrief();
        }

        /// <summary>
        /// When the File > New button is clicked
        /// </summary>
        private void FileNewMi_Click(object sender, RoutedEventArgs e)
        {
            createNewBrief();
        }

        /// <summary>
        /// When the main open button is clicked
        /// </summary>
        private void openBtn_Click(object sender, RoutedEventArgs e)
        {
            browseExistingBrief();
        }

        /// <summary>
        /// When the File > Open button is clicked
        /// </summary>
        private void FileOpenMi_Click(object sender, RoutedEventArgs e)
        {
            browseExistingBrief();
        }

        /// <summary>
        /// When the File > Exit button is clicked
        /// </summary>
        private void FileExitMi_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        
        }

        /// <summary>
        /// When the Options > Authenticate button is clicked
        /// </summary>
        private void OptionsAuthMi_Click(object sender, RoutedEventArgs e)
        {
            if(!hasSuccessfullyAuthenticated) {
                AuthenticateDialog authenticatedialog = new AuthenticateDialog(this);
                authenticatedialog.Visibility = Visibility.Visible;
            }
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
        /// Do a complete shutdown (prevents processes from remaining open when they should have stopped)
        /// </summary>
        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Application.Current.Shutdown();
        }
        #endregion
    }
}
