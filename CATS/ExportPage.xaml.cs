using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.WindowsAPICodePack.Dialogs; //enables open-folder dialogs

namespace CATS
{
    /// <summary>
    /// Interaction logic for ExportPage.xaml
    /// </summary>
    public partial class ExportPage : Page
    {
        private BUAssessment currentBua;
        private string currentFilePath;
        private List<string> currentAppendicesList;
        BackgroundWorker exportWorker; //does export work in the background to prevent the UI from freezing
        exportProgressScreen exportProgressWindow; //where to report export progress to

        public ExportPage(BUAssessment bua, string path, bool elevate)
        {
            InitializeComponent();
            currentBua = bua;
            currentFilePath = path;
            if (elevate) {
                enableElevatedOptions();
            }
            populateFields();
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
            exportWorker = new BackgroundWorker();
            exportWorker.DoWork += exportWorker_DoWork;
            exportWorker.ProgressChanged += exportWorker_ProgressChanged;
            exportWorker.RunWorkerCompleted += exportWorker_RunWorkerCompleted;
            exportWorker.WorkerReportsProgress = true;
            exportWorker.WorkerSupportsCancellation = false;

            currentAppendicesList = new List<string>();
            currentAppendicesList.Add("temp\\exported brief.pdf");
            currentAppendicesList.Add("random.pdf");
            currentAppendicesList.Add("random.pdf");

            string selectedExportPath = exportFolderTxt.Text + "\\" + exportFileTxt.Text + ".pdf";

            exportArgs args = new exportArgs();
            args.exportMode = 0;
            args.currentBua = currentBua;
            args.outputFilePath = selectedExportPath;
            args.appendixFilePaths = currentAppendicesList;
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
        /// When the elevated-only final export to PDF button is clicked
        /// Do a FINAL export
        /// </summary>
        private void exportFinalPdfBtn_Click(object sender, RoutedEventArgs e)
        {
            exportWorker = new BackgroundWorker();
            exportWorker.DoWork += exportWorker_DoWork;
            exportWorker.ProgressChanged += exportWorker_ProgressChanged;
            exportWorker.RunWorkerCompleted += exportWorker_RunWorkerCompleted;
            exportWorker.WorkerReportsProgress = true;
            exportWorker.WorkerSupportsCancellation = false;

            currentAppendicesList = new List<string>();
            currentAppendicesList.Add("temp\\exported brief.pdf");
            currentAppendicesList.Add("random.pdf");
            currentAppendicesList.Add("random.pdf");

            string selectedExportPath = exportFolderTxt.Text + "\\" + exportFileTxt.Text + ".pdf";

            exportArgs args = new exportArgs();
            args.exportMode = 1;
            args.currentBua = currentBua;
            args.outputFilePath = selectedExportPath;
            args.appendixFilePaths = currentAppendicesList;
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
