using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace CATS
{
    /// <summary>
    /// Interaction logic for ExportPage.xaml
    /// </summary>
    public partial class ExportPage : Page
    {
        private BUAssessment currentBua;
        private string currentFilePath;

        public ExportPage(BUAssessment bua, string path)
        {
            InitializeComponent();
            currentBua = bua;
            currentFilePath = path;
        }

        #region Event handlers
        /// <summary>
        /// When the export to PDF button is clicked
        /// </summary>
        private void exportPdfBtn_Click(object sender, RoutedEventArgs e)
        {
            currentBua.convertHtmlToPdf(currentBua.getHtmlDocument());

            List<string> appendicies = new List<string>();
            appendicies.Add("exported brief.pdf");
            appendicies.Add("random.pdf");
            appendicies.Add("random.pdf");
            currentBua.mergeMultiplePDFIntoSinglePDF("FINAL exported brief.pdf", appendicies);
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
    }
}
