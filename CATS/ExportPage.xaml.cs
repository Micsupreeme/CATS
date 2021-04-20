using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs; //enables open-folder dialogs

namespace CATS
{
    /// <summary>
    /// Interaction logic for ExportPage.xaml
    /// </summary>
    public partial class ExportPage : Page
    {
        private const int MAX_NUMBER_APPENDICES = 10;
        private const string APPENDIX_FILE_FILTER =
            "PDF Files (*.pdf)|*.pdf";
        private const int APPENDIX_ITEM_WIDTH = 155;
        private const int APPENDIX_ITEM_HEIGHT = 120;

        private BUAssessment currentBua;
        private string currentFilePath;
        private List<string> currentAppendicesList;
        BackgroundWorker exportWorker; //does export work in the background to prevent the UI from freezing
        exportProgressScreen exportProgressWindow; //where to report export progress to

        StackPanel appendicesFullStack;
        List<StackPanel> appendixItemStacks;

        public ExportPage(BUAssessment bua, string path, bool elevate)
        {
            InitializeComponent();
            currentBua = bua;
            currentFilePath = path;
            if (elevate) {
                enableElevatedOptions();
            }
            populateFields();
            createDynamicControls();
        }

        /// <summary>
        /// Create initial appendix controls to represent the current number of appendices in the BUA object
        /// </summary>
        private void createDynamicControls()
        {
            //Ensure canvas is reset before drawing
            appendicesCanvas.Children.Clear();
            appendicesCanvas.Height = APPENDIX_ITEM_HEIGHT;
            appendicesCanvas.Width = 0;

            appendixItemStacks = new List<StackPanel>();

            //Draw records
            for (int apdx = 0; apdx < currentBua.appendicesList.Count; apdx++)
            {
                appendicesCanvas.Width += (APPENDIX_ITEM_WIDTH + 5);

                var appendixImg = new Image();
                appendixImg.Height = 60;
                appendixImg.Width = 60;
                appendixImg.Margin = new Thickness(5);
                if(File.Exists(currentBua.appendicesList[apdx])) {
                    //White icon to indicate normal conditions - pointed-to appendix file exists
                    appendixImg.Source = new BitmapImage(new Uri("pack://application:,,,/img/icons/outline_description_white_48dp.png"));
                    appendixImg.ToolTip = currentBua.appendicesList[apdx];
                    appendixImg.Tag = 1; //A tag of 1 indicates this file exists
                } else {
                    //Red icon to indicate an error - pointed-to appendix file does not exist on the system (might have sent this BUA to someone else)
                    appendixImg.Source = new BitmapImage(new Uri("pack://application:,,,/img/icons/outline_description_red_48dp.png"));
                    appendixImg.ToolTip = currentBua.appendicesList[apdx] + "\nThis file does not exist. It must be changed or removed.";
                    appendixImg.Tag = 0; //A tag of 0 indicates this file doesn't exist
                }

                var appendixNameTxt = new TextBox();
                appendixNameTxt.FontFamily = new FontFamily("Arial");
                appendixNameTxt.FontSize = 13;
                appendixNameTxt.Text = Path.GetFileName(currentBua.appendicesList[apdx]); //limited space, don't show full path, just the file name
                appendixNameTxt.Tag = currentBua.appendicesList[apdx]; //we still need to know the full path later though, so store it as a tag
                appendixNameTxt.Height = 25;
                appendixNameTxt.Width = (APPENDIX_ITEM_WIDTH - 10);
                appendixNameTxt.Padding = new Thickness(2, 2, 2, 2);
                appendixNameTxt.Margin = new Thickness(5, 0, 5, 0);
                appendixNameTxt.IsReadOnly = true;
                appendixNameTxt.TextAlignment = TextAlignment.Center;
                appendixNameTxt.BorderThickness = new Thickness(0);
                appendixNameTxt.Background = new SolidColorBrush(Color.FromRgb(37, 37, 37));
                appendixNameTxt.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));

                var appendixChangeBtn = new Button();
                appendixChangeBtn.FontFamily = new FontFamily("Arial");
                appendixChangeBtn.FontSize = 13;
                appendixChangeBtn.Content = "Change";
                appendixChangeBtn.Height = 20;
                appendixChangeBtn.Width = (APPENDIX_ITEM_WIDTH - 10) / 2;
                appendixChangeBtn.Padding = new Thickness(5, 2, 5, 2);
                appendixChangeBtn.Margin = new Thickness(5, 0, 2.5, 5);
                appendixChangeBtn.Tag = apdx;
                appendixChangeBtn.AddHandler(Button.ClickEvent, new RoutedEventHandler(appendixChangeBtn_Click));

                var appendixDeleteBtn = new Button();
                appendixDeleteBtn.FontFamily = new FontFamily("Arial");
                appendixDeleteBtn.FontSize = 13;
                appendixDeleteBtn.Content = "Remove";
                appendixDeleteBtn.Height = 20;
                appendixDeleteBtn.Width = (APPENDIX_ITEM_WIDTH - 10) / 2;
                appendixDeleteBtn.Padding = new Thickness(5, 2, 5, 2);
                appendixDeleteBtn.Margin = new Thickness(2.5, 0, 5, 5);
                appendixDeleteBtn.Tag = apdx;
                appendixDeleteBtn.AddHandler(Button.ClickEvent, new RoutedEventHandler(appendixDeleteBtn_Click));

                var controlStack = new StackPanel();
                controlStack.Orientation = Orientation.Horizontal;
                controlStack.Children.Add(appendixChangeBtn);
                controlStack.Children.Add(appendixDeleteBtn);

                var appendixSingleItem = new StackPanel();
                appendixSingleItem.Orientation = Orientation.Vertical;
                appendixSingleItem.Children.Add(appendixImg);
                appendixSingleItem.Children.Add(appendixNameTxt);
                appendixSingleItem.Children.Add(controlStack);
                appendixItemStacks.Add(appendixSingleItem);
                appendixItemStacks[apdx].Name = "itemStk_" + apdx;
            }

            appendicesFullStack = new StackPanel();
            appendicesFullStack.Orientation = Orientation.Horizontal;
            foreach (StackPanel apdxItem in appendixItemStacks)
            {
                appendicesFullStack.Children.Add(apdxItem);
            }

            appendicesCanvas.Children.Add(appendicesFullStack);
            Canvas.SetLeft(appendicesFullStack, 0);
            Canvas.SetTop(appendicesFullStack, 0);

            updateNumberOfAppendices();
        }

        /// <summary>
        /// Update the appendices heading to match the current number of appendices
        /// </summary>
        private void updateNumberOfAppendices()
        {
            if(appendicesFullStack.Children.Count != 1) {
                appendicesTb.Text = appendicesFullStack.Children.Count + " appendices:";

                //Disable the add button if the maximum number of appendices has been reached
                if(appendicesFullStack.Children.Count == MAX_NUMBER_APPENDICES) {
                    appendixAddBtn.IsEnabled = false;
                } else {
                    appendixAddBtn.IsEnabled = true;
                }
            } else {
                appendicesTb.Text = appendicesFullStack.Children.Count + " appendix:";
                appendixAddBtn.IsEnabled = true;
            }
        }

        /// <summary>
        /// Update the BUA object appendices list with the contents of the dynamic appendix textboxes
        /// </summary>
        private void saveAppendixChanges()
        {
            List<string> tempAppendicesList = new List<string>();

            for (int apdx = 0; apdx < appendicesFullStack.Children.Count; apdx++) {
                StackPanel itemStack = (StackPanel)appendicesFullStack.Children[apdx];
                TextBox fileNameTxt = (TextBox)itemStack.Children[1]; //the second ("1") child of a single stack is always the file name textbox

                string fileNameTxtTag = (string)fileNameTxt.Tag; //the full file path is not stored in the text due to space limitations, it's in the tag
                if (fileNameTxtTag.Length > 0) {
                    tempAppendicesList.Add(fileNameTxtTag); //only save file names that aren't string empty
                }
            }
            currentBua.appendicesList = tempAppendicesList;
            currentBua.saveAsJson(currentFilePath);
        }

        /// <summary>
        /// Uses a messagebox to get confirmation before replace a file that already exists
        /// </summary>
        /// <param name="fileName">The file to ask to replace</param>
        /// <returns>True if the user agrees to replace the file, false otherwise</returns>
        private bool getOverwriteConfirmation(string fileName)
        {
            var result = MessageBox.Show(fileName + " already exists. Do you want to replace it?", "Confirm Export", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
            if(result == MessageBoxResult.Yes) {
                return true;
            } else {
                return false;
            }
        }

        /// <summary>
        /// Gets the number of appendices stored against this BUA that don't exist on the system
        /// If missing appendices are found, displays a messagebox to inform the user
        /// </summary>
        /// <returns>The number of appendices stored against this BUA that don't exist on the system</returns>
        private int getMissingAppendicesCount()
        {
            int missingAppendices = 0;
            List<string> missingAppendicesFiles = new List<string>();

            //Find missing appendices - when found, increment the count and add the problem file path to a list
            for (int apdx = 0; apdx < appendicesFullStack.Children.Count; apdx++) {
                StackPanel itemStack = (StackPanel)appendicesFullStack.Children[apdx];
                TextBox fileNameTxt = (TextBox)itemStack.Children[1]; //the second ("1") child of a single stack is always the file name textbox

                string fileNameTxtTag = (string)fileNameTxt.Tag; //the full file path is not stored in the text due to space limitations, it's in the tag
                if (fileNameTxtTag.Length > 0) {
                    if(!File.Exists(fileNameTxtTag)) {
                        missingAppendices++;
                        missingAppendicesFiles.Add(fileNameTxtTag);
                    }
                }
            }

            if(missingAppendices > 0) { //If problem appendices are found, inform the user of them
                //Create string list of the missing appendices
                string stringifiedAppendicesFiles = String.Empty;
                foreach (string missingApdx in missingAppendicesFiles) {
                    stringifiedAppendicesFiles += "- " + missingApdx + "\n";
                }

                MessageBox.Show("The export was cancelled because " + missingAppendices + " of the " + appendicesFullStack.Children.Count + " added appendices could not be found on the system. The following files need to be changed or removed:\n\n" + stringifiedAppendicesFiles, "Export Cancelled - Missing Appendices", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            return missingAppendices;
        }

        /// <summary>
        /// Upon page load, populate the fields with the values passed to this page
        /// </summary>
        private void populateFields()
        {
            exportFolderTxt.Text = Path.GetDirectoryName(currentFilePath);
            exportFileTxt.Text = Path.GetFileNameWithoutExtension(currentFilePath);
        }

        /// <summary>
        /// Displays a select-folder dialog and sets the save folder textbox to the selected folder
        /// </summary>
        private void selectExportFolder()
        {
            CommonOpenFileDialog cofd = new CommonOpenFileDialog();
            cofd.InitialDirectory = Path.GetDirectoryName(currentFilePath);
            cofd.IsFolderPicker = true;
            if (cofd.ShowDialog() == CommonFileDialogResult.Ok) {
                exportFolderTxt.Text = cofd.FileName;
            }
            this.Focus(); //cofd sends the window backwards, bring it back to the front after cofd is dismissed
        }

        /// <summary>
        /// Enables elevated user exclusive options
        /// </summary>
        private void enableElevatedOptions()
        {
            exportFinalPdfBtn.Visibility = Visibility.Visible;
            exportFinalHtmlBtn.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Sets the enabled state of the 4 export buttons to the specified state
        /// </summary>
        /// <param name="enabled">True to enable the buttons, or false to disable them</param>
        private void toggleExportButtonsEnabled(bool enabled)
        {
            exportPdfBtn.IsEnabled = enabled;
            exportFinalPdfBtn.IsEnabled = enabled;
            exportHtmlBtn.IsEnabled = enabled;
            exportFinalHtmlBtn.IsEnabled = enabled;
        }

        /// <summary>
        /// Performs an export to PDF operation with the specified watermark status
        /// </summary>
        private void doPdfExport(exportArgs exportArguments)
        {
            currentBua.convertHtmlToPdf(exportArguments.currentBua.getHtmlDocument(false, false), exportArguments.backgroundThread);
            currentBua.mergeMultiplePDFIntoSinglePDF(exportArguments.outputFilePath, exportArguments.appendixFilePaths, exportArguments.includeWatermark);
            exportArguments.backgroundThread.ReportProgress(100);
        }

        /// <summary>
        /// Performs an export to HTML operation with the specified watermark status
        /// </summary>
        private void doHtmlExport(exportArgs exportArguments)
        {
            currentBua.saveAsHtmlFile(currentBua.getHtmlDocument(exportArguments.includeWatermark, true), exportArguments.outputFilePath, exportArguments.backgroundThread);
            exportArguments.backgroundThread.ReportProgress(100);
        }

        #region Event handlers
        /// <summary>
        /// When the export folder "Browse..." button is clicked
        /// </summary>
        private void exportFolderBtn_Click(object sender, RoutedEventArgs e)
        {
            selectExportFolder();
        }

        /// <summary>
        /// When the export to PDF button is clicked
        /// Do a DRAFT export
        /// </summary>
        private void exportPdfBtn_Click(object sender, RoutedEventArgs e)
        {
            if(getMissingAppendicesCount() == 0) //Only do PDF export if all added appendices are present
            {
                exportWorker = new BackgroundWorker();
                exportWorker.DoWork += exportWorker_DoWork;
                exportWorker.ProgressChanged += exportWorker_ProgressChanged;
                exportWorker.RunWorkerCompleted += exportWorker_RunWorkerCompleted;
                exportWorker.WorkerReportsProgress = true;
                exportWorker.WorkerSupportsCancellation = false;

                currentAppendicesList = new List<string>();
                currentAppendicesList.Add("temp\\exported brief.pdf");
                foreach (string appendix in currentBua.appendicesList)
                {
                    currentAppendicesList.Add(appendix);
                }

                string selectedExportPath = exportFolderTxt.Text + "\\" + exportFileTxt.Text + ".pdf";

                exportArgs args = new exportArgs();
                args.exportMode = 0;
                args.currentBua = currentBua;
                args.outputFilePath = selectedExportPath;
                args.appendixFilePaths = currentAppendicesList;
                args.includeWatermark = true; //Not final, so include a draft watermark

                //Proceed if the specified export file doesn't already exist, or if the user has confirmed permission to replace it
                if (File.Exists(selectedExportPath))
                {
                    if (getOverwriteConfirmation(selectedExportPath))
                    {
                        toggleExportButtonsEnabled(false);
                        exportProgressWindow = new exportProgressScreen(args.exportMode);
                        exportProgressWindow.Visibility = Visibility.Visible;
                        exportWorker.RunWorkerAsync(args);
                    }
                }
                else
                {
                    toggleExportButtonsEnabled(false);
                    exportProgressWindow = new exportProgressScreen(args.exportMode);
                    exportProgressWindow.Visibility = Visibility.Visible;
                    exportWorker.RunWorkerAsync(args);
                }
            }
        }

        /// <summary>
        /// When the elevated-only final export to PDF button is clicked
        /// Do a FINAL export
        /// </summary>
        private void exportFinalPdfBtn_Click(object sender, RoutedEventArgs e)
        {
            if (getMissingAppendicesCount() == 0) //Only do PDF export if all added appendices are present
            {
                exportWorker = new BackgroundWorker();
                exportWorker.DoWork += exportWorker_DoWork;
                exportWorker.ProgressChanged += exportWorker_ProgressChanged;
                exportWorker.RunWorkerCompleted += exportWorker_RunWorkerCompleted;
                exportWorker.WorkerReportsProgress = true;
                exportWorker.WorkerSupportsCancellation = false;

                currentAppendicesList = new List<string>();
                currentAppendicesList.Add("temp\\exported brief.pdf");
                foreach (string appendix in currentBua.appendicesList)
                {
                    currentAppendicesList.Add(appendix);
                }

                string selectedExportPath = exportFolderTxt.Text + "\\" + exportFileTxt.Text + ".pdf";

                exportArgs args = new exportArgs();
                args.exportMode = 1;
                args.currentBua = currentBua;
                args.outputFilePath = selectedExportPath;
                args.appendixFilePaths = currentAppendicesList;
                args.includeWatermark = false; //Final, so don't include a draft watermark

                //Proceed if the specified export file doesn't already exist, or if the user has confirmed permission to replace it
                if (File.Exists(selectedExportPath))
                {
                    if (getOverwriteConfirmation(selectedExportPath))
                    {
                        toggleExportButtonsEnabled(false);
                        exportProgressWindow = new exportProgressScreen(args.exportMode);
                        exportProgressWindow.Visibility = Visibility.Visible;
                        exportWorker.RunWorkerAsync(args);
                    }
                }
                else
                {
                    toggleExportButtonsEnabled(false);
                    exportProgressWindow = new exportProgressScreen(args.exportMode);
                    exportProgressWindow.Visibility = Visibility.Visible;
                    exportWorker.RunWorkerAsync(args);
                }
            }
        }

        /// <summary>
        /// When the export to HTML button is clicked
        /// Do a DRAFT export
        /// </summary>
        private void exportHtmlBtn_Click(object sender, RoutedEventArgs e)
        {
            exportWorker = new BackgroundWorker();
            exportWorker.DoWork += exportWorker_DoWork;
            exportWorker.ProgressChanged += exportWorker_ProgressChanged;
            exportWorker.RunWorkerCompleted += exportWorker_RunWorkerCompleted;
            exportWorker.WorkerReportsProgress = true;
            exportWorker.WorkerSupportsCancellation = false;

            string selectedExportPath = exportFolderTxt.Text + "\\" + exportFileTxt.Text + ".html";

            exportArgs args = new exportArgs();
            args.exportMode = 2;
            args.currentBua = currentBua;
            args.outputFilePath = selectedExportPath;
            args.appendixFilePaths = new List<string>(); //Does not support appendices
            args.includeWatermark = true; //Not final, so include a draft watermark

            //Proceed if the specified export file doesn't already exist, or if the user has confirmed permission to replace it
            if (File.Exists(selectedExportPath)) {
                if (getOverwriteConfirmation(selectedExportPath)) {
                    toggleExportButtonsEnabled(false);
                    exportProgressWindow = new exportProgressScreen(args.exportMode);
                    exportProgressWindow.Visibility = Visibility.Visible;
                    exportWorker.RunWorkerAsync(args);
                }
            } else {
                toggleExportButtonsEnabled(false);
                exportProgressWindow = new exportProgressScreen(args.exportMode);
                exportProgressWindow.Visibility = Visibility.Visible;
                exportWorker.RunWorkerAsync(args);
            }
        }

        /// <summary>
        /// When the elevated-only final HTML export button is clicked
        /// Do a FINAL export
        /// </summary>
        private void exportFinalHtmlBtn_Click(object sender, RoutedEventArgs e)
        {
            exportWorker = new BackgroundWorker();
            exportWorker.DoWork += exportWorker_DoWork;
            exportWorker.ProgressChanged += exportWorker_ProgressChanged;
            exportWorker.RunWorkerCompleted += exportWorker_RunWorkerCompleted;
            exportWorker.WorkerReportsProgress = true;
            exportWorker.WorkerSupportsCancellation = false;

            string selectedExportPath = exportFolderTxt.Text + "\\" + exportFileTxt.Text + ".html";

            exportArgs args = new exportArgs();
            args.exportMode = 3;
            args.currentBua = currentBua;
            args.outputFilePath = selectedExportPath;
            args.appendixFilePaths = new List<string>(); //Does not support appendices
            args.includeWatermark = false; //Final, so don't include a draft watermark

            //Proceed if the specified export file doesn't already exist, or if the user has confirmed permission to replace it
            if (File.Exists(selectedExportPath)) {
                if (getOverwriteConfirmation(selectedExportPath)) {
                    toggleExportButtonsEnabled(false);
                    exportProgressWindow = new exportProgressScreen(args.exportMode);
                    exportProgressWindow.Visibility = Visibility.Visible;
                    exportWorker.RunWorkerAsync(args);
                }
            } else {
                toggleExportButtonsEnabled(false);
                exportProgressWindow = new exportProgressScreen(args.exportMode);
                exportProgressWindow.Visibility = Visibility.Visible;
                exportWorker.RunWorkerAsync(args);
            }
        }

        /// <summary>
        /// When the background export thread starts doing work
        /// </summary>
        void exportWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bw = sender as BackgroundWorker;
            exportArgs args = e.Argument as exportArgs;
            args.backgroundThread = bw;
            if(args.exportMode < 2) {
                doPdfExport(args);
            } else {
                doHtmlExport(args);
            }
        }

        /// <summary>
        /// When the background export thread reports new progress
        /// </summary>
        private void exportWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //Keep the user updated of progress by updating the progress window's UI
            exportProgressWindow.exportProg.Value = e.ProgressPercentage;
        }

        /// <summary>
        /// When the background export thread completes its work
        /// </summary>
        void exportWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            exportProgressWindow.Visibility = Visibility.Collapsed;
            toggleExportButtonsEnabled(true);

            //Every export means a new revision
            currentBua.revisionVer++;
            currentBua.saveAsJson(currentFilePath);
        }

        /// <summary>
        /// When the "add appendix file" button is clicked
        /// </summary>
        private void appendixAddBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog()
            {
                Filter = APPENDIX_FILE_FILTER,
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };
            if (ofd.ShowDialog() == true)
            {
                appendicesCanvas.Width += (APPENDIX_ITEM_WIDTH + 5);

                var newAppendixImg = new Image();
                newAppendixImg.Height = 60;
                newAppendixImg.Width = 60;
                newAppendixImg.Margin = new Thickness(5);
                if (File.Exists(ofd.FileName)) {
                    //White icon to indicate normal conditions - pointed-to appendix file exists
                    newAppendixImg.Source = new BitmapImage(new Uri("pack://application:,,,/img/icons/outline_description_white_48dp.png"));
                    newAppendixImg.ToolTip = ofd.FileName;
                    newAppendixImg.Tag = 1; //A tag of 1 indicates this file exists
                } else {
                    //Red icon to indicate an error - pointed-to appendix file does not exist on the system (might have sent this BUA to someone else)
                    newAppendixImg.Source = new BitmapImage(new Uri("pack://application:,,,/img/icons/outline_description_red_48dp.png"));
                    newAppendixImg.ToolTip = ofd.FileName + "\n\nThis file does not exist. It must be changed or removed.";
                    newAppendixImg.Tag = 0; //A tag of 0 indicates this file doesn't exist
                }

                var newAppendixNameTxt = new TextBox();
                newAppendixNameTxt.FontFamily = new FontFamily("Arial");
                newAppendixNameTxt.FontSize = 13;
                newAppendixNameTxt.Text = Path.GetFileName(ofd.FileName); //limited space, don't show full path, just the file name
                newAppendixNameTxt.Tag = ofd.FileName;
                newAppendixNameTxt.Height = 25;
                newAppendixNameTxt.Width = (APPENDIX_ITEM_WIDTH - 10);
                newAppendixNameTxt.Padding = new Thickness(2, 2, 2, 2);
                newAppendixNameTxt.Margin = new Thickness(5, 0, 5, 0);
                newAppendixNameTxt.IsReadOnly = true;
                newAppendixNameTxt.TextAlignment = TextAlignment.Center;
                newAppendixNameTxt.BorderThickness = new Thickness(0);
                newAppendixNameTxt.Background = new SolidColorBrush(Color.FromRgb(37, 37, 37));
                newAppendixNameTxt.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));

                var newAppendixChangeBtn = new Button();
                newAppendixChangeBtn.FontFamily = new FontFamily("Arial");
                newAppendixChangeBtn.FontSize = 13;
                newAppendixChangeBtn.Content = "Change";
                newAppendixChangeBtn.Height = 20;
                newAppendixChangeBtn.Width = (APPENDIX_ITEM_WIDTH - 10) / 2;
                newAppendixChangeBtn.Padding = new Thickness(5, 2, 5, 2);
                newAppendixChangeBtn.Margin = new Thickness(5, 0, 2.5, 5);
                newAppendixChangeBtn.Tag = appendicesFullStack.Children.Count;
                newAppendixChangeBtn.AddHandler(Button.ClickEvent, new RoutedEventHandler(appendixChangeBtn_Click));

                var newAppendixDeleteBtn = new Button();
                newAppendixDeleteBtn.FontFamily = new FontFamily("Arial");
                newAppendixDeleteBtn.FontSize = 13;
                newAppendixDeleteBtn.Content = "Delete";
                newAppendixDeleteBtn.Height = 20;
                newAppendixDeleteBtn.Width = (APPENDIX_ITEM_WIDTH - 10) / 2;
                newAppendixDeleteBtn.Padding = new Thickness(5, 2, 5, 2);
                newAppendixDeleteBtn.Margin = new Thickness(2.5, 0, 5, 5);
                newAppendixDeleteBtn.Tag = appendicesFullStack.Children.Count;
                newAppendixDeleteBtn.AddHandler(Button.ClickEvent, new RoutedEventHandler(appendixDeleteBtn_Click));

                var newControlStack = new StackPanel();
                newControlStack.Orientation = Orientation.Horizontal;
                newControlStack.Children.Add(newAppendixChangeBtn);
                newControlStack.Children.Add(newAppendixDeleteBtn);

                var newAppendixSingleItem = new StackPanel();
                newAppendixSingleItem.Name = "itemStk_" + appendicesFullStack.Children.Count;
                newAppendixSingleItem.Orientation = Orientation.Vertical;
                newAppendixSingleItem.Children.Add(newAppendixImg);
                newAppendixSingleItem.Children.Add(newAppendixNameTxt);
                newAppendixSingleItem.Children.Add(newControlStack);
                appendicesFullStack.Children.Add(newAppendixSingleItem);
                saveAppendixChanges();
                updateNumberOfAppendices();
            }
        }

        /// <summary>
        /// When a dynamic "change" appendix button is clicked
        /// </summary>
        private void appendixChangeBtn_Click(object sender, RoutedEventArgs e)
        {
            Button eventBtn = (Button)e.Source;
            int eventTag = (int)eventBtn.Tag; //the tag that indicates the appendix item (containing all the controls to be deleted)

            OpenFileDialog ofd = new OpenFileDialog()
            {
                Filter = APPENDIX_FILE_FILTER,
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };
            if (ofd.ShowDialog() == true)
            {
                for (int apdx = 0; apdx < appendicesFullStack.Children.Count; apdx++)
                {
                    StackPanel itemStack = (StackPanel)appendicesFullStack.Children[apdx];
                    if (itemStack.Name.Equals("itemStk_" + eventTag)) {

                        //Update image UI control to reflect new appendix path
                        Image itemStackImg = (Image)itemStack.Children[0]; //Child 0 is always the image
                        if (File.Exists(ofd.FileName)) {
                            //White icon to indicate normal conditions - pointed-to appendix file exists
                            itemStackImg.Source = new BitmapImage(new Uri("pack://application:,,,/img/icons/outline_description_white_48dp.png"));
                            itemStackImg.ToolTip = ofd.FileName;
                            itemStackImg.Tag = 1; //A tag of 1 indicates this file exists
                        } else {
                            //Red icon to indicate an error - pointed-to appendix file does not exist on the system (might have sent this BUA to someone else)
                            itemStackImg.Source = new BitmapImage(new Uri("pack://application:,,,/img/icons/outline_description_red_48dp.png"));
                            itemStackImg.ToolTip = ofd.FileName + "\nThis file does not exist. It must be changed or removed.";
                            itemStackImg.Tag = 0; //A tag of 0 indicates this file doesn't exist
                        }

                        //Update textbox UI control to reflect new appendix path
                        TextBox itemStackTxt = (TextBox)itemStack.Children[1]; //Child 1 is always the textbox
                        itemStackTxt.Text = Path.GetFileName(ofd.FileName);
                        itemStackTxt.Tag = ofd.FileName;
                    }
                }
                saveAppendixChanges();
            }
        }

        /// <summary>
        /// When a dynamic "delete" appendix button is clicked
        /// </summary>
        private void appendixDeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            appendicesCanvas.Width -= (APPENDIX_ITEM_WIDTH + 5);

            Button eventBtn = (Button)e.Source;
            int eventTag = (int)eventBtn.Tag; //the tag that indicates the appendix item (containing all the controls to be deleted)

            for (int apdx = 0; apdx < appendicesFullStack.Children.Count; apdx++)
            {
                StackPanel itemStack = (StackPanel)appendicesFullStack.Children[apdx];
                if (itemStack.Name.Equals("itemStk_" + eventTag)) {
                    appendicesFullStack.Children.RemoveAt(apdx);
                }
            }
            saveAppendixChanges();
            updateNumberOfAppendices();
        }
        #endregion

        /// <summary>
        /// Inner class used to pass arguments to the async export worker
        /// </summary>
        public class exportArgs
        {
            public int exportMode;
            public BUAssessment currentBua;
            public string outputFilePath;
            public List<string> appendixFilePaths;
            public bool includeWatermark;
            public BackgroundWorker backgroundThread;
        }
    }
}
