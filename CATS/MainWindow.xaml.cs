using Microsoft.Win32; //enables openfiledialog
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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

        public MainWindow()
        {           
            InitializeComponent();
        }

        private void createBtn_Click(object sender, RoutedEventArgs e)
        {
            createNewBrief();
        }

        private void openBtn_Click(object sender, RoutedEventArgs e)
        {
            openExistingBrief();
        }

        /// <summary>
        /// Brings up the new brief dialog for selecting a new filepath
        /// </summary>
        private void createNewBrief()
        {
            NewBriefDialog newbriefdialog = new NewBriefDialog(this);
            newbriefdialog.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Brings up an openfiledialog for directly selecting an existing file to edit
        /// </summary>
        private void openExistingBrief()
        {
            OpenFileDialog ofd = new OpenFileDialog()
            {
                Filter = BUA_FILE_FILTER,
                InitialDirectory = getDefaultDocsPath()
            };
            if (ofd.ShowDialog() == true)
            {
                BUAssessment openedBua = new BUAssessment();
                openedBua.loadFromJson(ofd.FileName);

                PagedWindow pw = new PagedWindow(openedBua, ofd.FileName);
                pw.Visibility = Visibility.Visible;
                this.Visibility = Visibility.Collapsed;
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
    }
}
