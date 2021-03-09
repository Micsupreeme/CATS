﻿using System;
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
using System.IO;
using System.Windows.Shapes;
using Microsoft.WindowsAPICodePack.Dialogs; //enables open-folder dialogs

namespace CATS
{
    /// <summary>
    /// Interaction logic for NewBriefDialog.xaml
    /// </summary>
    public partial class NewBriefDialog : Window
    {
        private const string DEFAULT_SAVE_FOLDER = "BU CW Assignments";

        MainWindow callingWindow; //Storing this enables the closing of the initial Window when a new BUA is created
        string selectedSaveFolder, selectedSaveFile;

        public NewBriefDialog(MainWindow caller)
        {
            InitializeComponent();
            callingWindow = caller;
            selectedSaveFolder = getDefaultDocsPath();
            saveFolderTxt.Text = selectedSaveFolder;
        }

        private void saveFolderBtn_Click(object sender, RoutedEventArgs e)
        {
            selectSaveFolder();
        }

        private void createBtn_Click(object sender, RoutedEventArgs e)
        {
            selectSaveFile();
        }

        private void cancelBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Displays a select-folder dialog and sets the save folder textbox to the selected folder
        /// </summary>
        private void selectSaveFolder()
        {
            CommonOpenFileDialog cofd = new CommonOpenFileDialog();
            cofd.InitialDirectory = getDefaultDocsPath();
            cofd.IsFolderPicker = true;
            if (cofd.ShowDialog() == CommonFileDialogResult.Ok)
            {
                selectedSaveFolder = cofd.FileName;
                saveFolderTxt.Text = selectedSaveFolder;
            }
            this.Activate(); //cofd sends the window backwards, bring it back to the front after cofd is dismissed
        }

        /// <summary>
        /// If the specified file name already exists, confirms that the user wants to replace it before continuing
        /// then calls for the file name to be fixed and opens the editor
        /// </summary>
        private void selectSaveFile()
        {
            selectedSaveFile = saveFileTxt.Text;
            string selectedFullPath = selectedSaveFolder + "\\" + selectedSaveFile + ".bua";

            if (File.Exists(selectedSaveFolder + "\\" + selectedSaveFile + ".bua")) {
                var msgRes = MessageBox.Show(selectedSaveFile + ".bua already exists.\nDo you want to replace it?", "Confirm Save As", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if(msgRes == MessageBoxResult.Yes) {
                    createSaveFile(selectedFullPath);
                }
            } else {
                createSaveFile(selectedFullPath);
            }
        }

        /// <summary>
        /// Creates a new BUAssessment instance
        /// Opens the new BUAssessment in the editor
        /// </summary>
        /// <param name="fullPath">The full folder+file+extension string path for the new assignment</param>
        private void createSaveFile(string fullPath)
        {
            BUAssessment newBua = new BUAssessment();
            newBua.saveAsJson(fullPath);

            PagedWindow pw = new PagedWindow(newBua, fullPath);
            pw.Visibility = Visibility.Visible;
            this.Visibility = Visibility.Collapsed;
            callingWindow.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Creates the default save folder if it doesn't already exist, and returns its name
        /// </summary>
        /// <returns>The name of the app's default save folder</returns>
        private string getDefaultDocsPath()
        {
            if(!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\" + DEFAULT_SAVE_FOLDER)) {
                //Directory doesn't exist, create it before returning
                Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\" + DEFAULT_SAVE_FOLDER);
            }
            return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\" + DEFAULT_SAVE_FOLDER;
        }
    }
}
