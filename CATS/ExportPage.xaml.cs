﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Imaging;
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
        BackgroundWorker exportWorker;
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
        }

        /// <summary>
        /// Performs an export to PDF operation with the specified watermark status
        /// </summary>
        private void doPdfExport(exportArgs exportArguments)
        {
            currentBua.convertHtmlToPdf(exportArguments.currentBua.getHtmlDocument(), exportArguments.backgroundThread);
            currentBua.mergeMultiplePDFIntoSinglePDF(exportArguments.outputFilePath, exportArguments.appendixFilePaths, exportArguments.includeWatermark);
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

            List<string> appendices = new List<string>();
            appendices.Add("temp\\exported brief.pdf");
            appendices.Add("random.pdf");
            appendices.Add("random.pdf");
            string selectedExportPath = exportFolderTxt.Text + "\\" + exportFileTxt.Text + ".pdf";

            exportArgs args = new exportArgs();
            args.currentBua = currentBua;
            args.outputFilePath = selectedExportPath;
            args.appendixFilePaths = appendices;
            args.includeWatermark = true; //Not final, so include a draft watermark

            exportPdfBtn.IsEnabled = false;
            exportFinalPdfBtn.IsEnabled = false;
            exportProgressWindow = new exportProgressScreen(0);
            exportProgressWindow.Visibility = Visibility.Visible;
            exportWorker.RunWorkerAsync(args);
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

            List<string> appendices = new List<string>();
            appendices.Add("temp\\exported brief.pdf");
            appendices.Add("random.pdf");
            appendices.Add("random.pdf");
            string selectedExportPath = exportFolderTxt.Text + "\\" + exportFileTxt.Text + ".pdf";

            exportArgs args = new exportArgs();
            args.currentBua = currentBua;
            args.outputFilePath = selectedExportPath;
            args.appendixFilePaths = appendices;
            args.includeWatermark = false; //Final, so don't include a draft watermark

            exportPdfBtn.IsEnabled = false;
            exportFinalPdfBtn.IsEnabled = false;
            exportProgressWindow = new exportProgressScreen(1);
            exportProgressWindow.Visibility = Visibility.Visible;
            exportWorker.RunWorkerAsync(args);
        }

        /// <summary>
        /// When the background export thread starts doing work
        /// </summary>
        void exportWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bw = sender as BackgroundWorker;
            exportArgs args = e.Argument as exportArgs;
            args.backgroundThread = bw;
            doPdfExport(args);
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
            exportPdfBtn.IsEnabled = true;
            exportFinalPdfBtn.IsEnabled = true;
        }
        #endregion

        private void exportPdfBtn1_Click(object sender, RoutedEventArgs e)
        {
            FileStream fs;
            fs = new FileStream("D:\\Home\\Pictures\\Camera Roll\\7.jpg", FileMode.Open);
            Console.WriteLine("Raw size: " + fs.Length + "bytes");
            fs.Close();

            MemoryStream ms;
            ms = new MemoryStream();
            System.Drawing.Image imggg = currentBua.compressImage("D:\\Home\\Pictures\\Camera Roll\\7.jpg", 50);
            imggg.Save(ms, ImageFormat.Jpeg);
            Console.WriteLine("Compressed size: " + ms.Length + "bytes");
            ms.Close();
        }

        /// <summary>
        /// Inner class used to pass arguments to the async export worker
        /// </summary>
        public class exportArgs
        {
            public BUAssessment currentBua;
            public string outputFilePath;
            public List<string> appendixFilePaths;
            public bool includeWatermark;
            public BackgroundWorker backgroundThread;
        }
    }
}