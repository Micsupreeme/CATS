using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Nancy.Helpers;
using System.Drawing;
using System.Drawing.Imaging;
using System.ComponentModel;

namespace CATS
{
    public class BUAssessment
    {
        public BUAssessment()
        {
            setDefaultValues();
        }

        /// <summary>
        /// Set all fields of this instance to the default values
        /// </summary>
        private void setDefaultValues()
        {
            this.createdDate = DateTime.Now;
            this.templateVer = 19.0f; //Hardcoded single template at the moment

            //TitleLevelPage
            this.unitTitle = String.Empty;
            this.asmtTitle = String.Empty;
            this.level = 4;
            this.isResub = false;
            this.asmtNoX = 1;
            this.asmtNoY = 2;

            //WeightDatePage
            this.isGroup = false;
            this.weighting = 50;
            this.creditValue = 20;
            this.submissionDueDate = new DateTime(1999, 1, 19, 19, 19, 19); //Arbitrary initialisation date to ignore
            this.impPath = String.Empty;

            //StaffSubPage
            this.unitLeader = String.Empty;
            this.qualityAssessor = String.Empty;
            this.markersList = new List<string>();
            this.submissionLocation = "Turnitin";
            this.feedbackMethod = "Turnitin";

            //RichtextPage (Reusable)
            this.assessmentTaskHtml = String.Empty;
            this.submissionFormatHtml = String.Empty;
            this.markingCriteriaHtml = String.Empty;

            //ILOsPage
            this.ILOsList = new List<string>();

            //QuestionsSignaturePage
            this.questionsAboutBrief = String.Empty;
            this.signatureMarker = this.unitLeader;
        }

        /// <summary>
        /// Checks whether the submission due date is greater than the initialisation date (of 1999)
        /// </summary>
        /// <returns>True if the submission due date is greater, false otherwise</returns>
        public bool hasValidSubmissionDueDate()
        {
            DateTime initialisationDate = new DateTime(1999, 1, 19, 19, 19, 19); //Arbitrary initialisation date to ignore
            if (this.submissionDueDate > initialisationDate) {
                return true;
            } else {
                return false;
            }
        }

        /// <summary>
        /// Populates (loads) this object from a specified JSON format filepath
        /// </summary>
        /// <param name="filePath">The full filepath to load</param>
        public void loadFromJson(string filePath)
        {
            MemoryStream ms = new MemoryStream();
            try
            {
                using (FileStream file = new FileStream(filePath, FileMode.Open, System.IO.FileAccess.Read))
                {
                    byte[] bytes = new byte[file.Length];
                    file.Read(bytes, 0, (int)file.Length);
                    ms.Write(bytes, 0, (int)file.Length);
                    Console.WriteLine("Successfully loaded " + filePath);
                }
                string json = Encoding.UTF8.GetString(ms.ToArray());
                ms.Close();

                JsonConvert.PopulateObject(json, this); //Set the values of this BUAssessment instance to match those loaded from the JSON file
            }
            catch (Exception)
            {
                //File cannot be accessed (e.g. used by another process)
                Console.WriteLine("ERROR: Unable to load " + filePath + " - used by another process?");
                MessageBox.Show("Unable to load " + filePath + ". It might be locked by another process.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Saves this object in JSON format to the specified filepath
        /// </summary>
        /// <param name="filepath">The full filepath to save/overwrite</param>
        public void saveAsJson(string filePath)
        {
            string json = JsonConvert.SerializeObject(this, Formatting.Indented);
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(json));

            try
            {
                using (FileStream file = new FileStream(filePath, FileMode.Create, System.IO.FileAccess.Write))
                {
                    byte[] bytes = new byte[ms.Length];
                    ms.Read(bytes, 0, (int)ms.Length);
                    file.Write(bytes, 0, bytes.Length);
                    Console.WriteLine("Successfully saved " + filePath);
                    ms.Close();
                }
            }
            catch (Exception)
            {
                //File cannot be accessed (e.g. used by another process)
                Console.WriteLine("ERROR: Unable to write to " + filePath + " - used by another process?");
                MessageBox.Show("Unable to save " + filePath + ". It might be locked by another process.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Gets the Base64 data string for the specified image
        /// </summary>
        /// <param name="fullImagePath">The full image path of the file to get the Base64 data string for</param>
        /// <returns>The Base64 data string for the specified image</returns>
        private string getBase64ImageString(string fullImagePath)
        {
            //The Smith editor inserts images with data URI sources - convert them to straight-up filepaths
            string decodedImagePath = fullImagePath.Substring(8); //ignore the "file///" URI prefix
            decodedImagePath = HttpUtility.UrlDecode(decodedImagePath);

            //We have the filepath, now compress the image - don't create a Base 64 data URI for a raw 4MB (or so) photograph
            MemoryStream compressedImageStream = new MemoryStream();
            Image compressedImage = compressImage(decodedImagePath, 40); //40% JPEG quality for compressed images

            //Base 64 encode the byte array of the compressed image
            compressedImage.Save(compressedImageStream, ImageFormat.Jpeg);
            byte[] compressedImageBytes = compressedImageStream.ToArray();
            return Convert.ToBase64String(compressedImageBytes);
        }

        #region https://stackoverflow.com/users/3152130/taw's lossy image compressor
        /// <summary>
        /// Compresses the specified image to a JPEG with the specified quality percent
        /// </summary>
        /// <param name="imageFilePath">The file to compress</param>
        /// <param name="imageQualityPercent">The JPEG quality percentage of the returned image</param>
        /// <returns>The compressed version of the specified image (always a JPG)</returns>
        public Image compressImage(string imageFilePath, int imageQualityPercent)
        {
            Image image = Image.FromFile(imageFilePath);
            using (Image memImage = new Bitmap(image, image.Width, image.Height))
            {
                //Declarations
                ImageCodecInfo imageCodecInfo;
                System.Drawing.Imaging.Encoder imageEncoder;
                EncoderParameter encoderParameter;
                EncoderParameters encoderParameters;

                //Set up the compression encoder
                imageCodecInfo = GetEncoderInfo("image/jpeg");
                imageEncoder = System.Drawing.Imaging.Encoder.Quality;
                encoderParameters = new EncoderParameters(1);
                encoderParameter = new EncoderParameter(imageEncoder, imageQualityPercent);
                encoderParameters.Param[0] = encoderParameter;

                //Save the compressed image using the encoder
                MemoryStream memStream = new MemoryStream();
                memImage.Save(memStream, imageCodecInfo, encoderParameters);
                Image newImage = Image.FromStream(memStream);

                //Set the size of the image - currently keeps default size (can replace newImage.Width/Height with desired size)
                using (Graphics g = Graphics.FromImage(newImage))
                {
                    g.InterpolationMode =
                      System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.DrawImage(newImage, new Rectangle(System.Drawing.Point.Empty, newImage.Size), 0, 0,
                      newImage.Width, newImage.Height, GraphicsUnit.Pixel, new ImageAttributes());
                }
                Console.WriteLine("compressed " + imageFilePath);
                return newImage;
            }
        }

        /// <summary>
        /// Gets a valid specified image encoder
        /// </summary>
        /// <param name="mimeType">The mime type of the output image</param>
        /// <returns>A valid image encoder</returns>
        private static ImageCodecInfo GetEncoderInfo(String mimeType)
        {
            ImageCodecInfo[] encoders;
            encoders = ImageCodecInfo.GetImageEncoders();
            foreach (ImageCodecInfo ici in encoders) {
                if (ici.MimeType == mimeType) {
                    return ici;
                }
            }
            return null;
        }
        #endregion

        /// <summary>
        /// Converts the filepath image sources within the provided HTML to base 64 data sources
        /// </summary>
        /// <param name="html">The HTML to be treated</param>
        /// <returns>The HTML with image sources replaced with base 64 strings</returns>
        public string encodeImageSources(string html)
        {
            HtmlDocument htmlDoc = new HtmlDocument();
            try {
                htmlDoc.LoadHtml(html);
            } catch(ArgumentNullException) {
                Console.WriteLine("WARN: No HTML to process!");
                return String.Empty;
            }
            try {
                foreach (HtmlNode imageNode in htmlDoc.DocumentNode.SelectNodes("//img")) {
                    string srcVal = imageNode.Attributes["src"].Value;

                    string base64Prefix = "data:image/";
                    switch (srcVal.Substring(srcVal.Length - 3)) {
                        case "jpg":
                            base64Prefix += "jpg;base64,";
                            break;
                        case "peg":
                            base64Prefix += "jpg;base64,";
                            break;
                        case "png":
                            base64Prefix += "png;base64,";
                            break;
                        default:
                            Console.WriteLine("WARN: encoding non-jpg|jpeg|png image");
                            base64Prefix += srcVal.Substring(srcVal.Length - 3) + ";base64,";
                            break;
                    }
                    string base64ImageString = base64Prefix + getBase64ImageString(srcVal);

                    imageNode.Attributes["src"].Value = base64ImageString;
                }
            } catch(NullReferenceException) {
                Console.Error.WriteLine("ERROR: No image nodes returned during image encoding");
            }            
            return htmlDoc.DocumentNode.OuterHtml;
        }

        /// <summary>
        /// Stitches together a HTML document for this assessment brief instance by combining template HTML code with variables
        /// </summary>
        /// <returns>The HTML document string for this instance</returns>
        public string getHtmlDocument()
        {
            //Some fields need to be prepared before they can be injected into the HTML

            //Markers
            string formattedMarkersStr = String.Empty;
            if(this.markersList.Count > 1) {
                //multiple markers, make a grammar-seperated list (e.g., "comma, comma, and ")
                for (int m = 0; m < this.markersList.Count; m++) {
                    if (m == (markersList.Count - 1)) {
                        formattedMarkersStr += "and " + this.markersList[m];
                    } else {
                        formattedMarkersStr += this.markersList[m] + ", ";
                    }
                }
            } else {
                //1 marker, just output it
                formattedMarkersStr += this.markersList[0];
            }

            //Group/Individual Weighting sentence
            string formattedWeightingStr;
            if(this.isGroup) {
                formattedWeightingStr = "This is a group assignment which carries " + this.weighting + "% of the final unit mark";
            } else {
                formattedWeightingStr = "This is an individual assignment which carries " + this.weighting + "% of the final unit mark";
            }

            //ILOs
            string formattedILOsStr = "This assignment tests your ability to:<br><ol>";
            foreach(string ilo in this.ILOsList)
            {
                formattedILOsStr += "<li>" + ilo + "</li>";
            }
            formattedILOsStr += "</ol>";

            //Stich together HTML sections:

            string EXPORT_HEAD_BLOCK =
               "<html lang = \"en\"><head><title>" + this.asmtTitle + "</title>";
             string EXPORT_STYLE_BLOCK =
            "<style type=\"text/css\">" +
                "h1 {font-size: 12pt;}" +
                "h1, h2 {font-weight: bold;}" +
                "ol, ul {margin-top: 3px; margin-bottom: 3px;}" +
                "td, th {padding: 2px; border: 1px solid black;}" +
                "img {page-break-inside: avoid;}" +
                "table {width: 95%; border-collapse: collapse; page-break-inside: avoid; display: block; margin-left: auto; margin-right: auto;}" +
                "@page {margin: 2.54cm;}" + //Only affects default Web Browser print methods - SelectPDF HTML-to-PDF has its own settings to specify margins
                "* {font-family: Arial; font-size: 10pt;}" +
            "</style>";
            string EXPORT_DETAILS_BLOCK =
                "</head><body><section id=\"content\">" +
                "<h1 style=\"font-family: Arial; font-size: 12pt; font-weight:bold; background-color: #E5E5E5; text-align: center; padding-top: 5px; padding-bottom: 10px; margin: 0px; line-height: 1.5;\">Faculty of Science and Technology - Department of Computing and Informatics</h1>" +
                "<table style=\"table-layout: fixed; width: 95%; border-collapse: collapse; margin-left: auto; margin-right: auto;  box-shadow: 25px 0px #E5E5E5, -26px 0px #E5E5E5; background-color: #E5E5E5;\">" +         
                "<tbody>" +
                "<tr style=\"padding:5px;\">" +
                "<th colspan=\"5\" style=\"padding-left: 0px; border: 1px solid black; font-weight: bold; padding-right: 5px;\">Unit Title: " + this.unitTitle + "</th>" +
                "</tr>" +
                "<tr>" +
                "<th colspan=\"5\" style=\"text-align:left; border: 1px solid black; font-weight: bold; padding-left: 5px; padding-right: 5px;\">Assessment Title: " + this.asmtTitle +
                "</th></tr>" +
                "<tr>" +
                "<td colspan=\"2\" style=\"border: 1px solid black; padding-left: 5px; padding-right: 5px;\"><b>Unit Level: </b>" + this.level + "</td>" +
                "<td colspan=\"3\" style=\"border: 1px solid black; padding-left: 5px; padding-right: 5px;\"><b>Assessment Number:</b>&nbsp;&nbsp;&nbsp;&nbsp;" + this.asmtNoX + "&nbsp;&nbsp;&nbsp;&nbsp;of&nbsp;&nbsp;&nbsp;&nbsp;" + this.asmtNoY + "</td>" +
                "</tr>" +
                "<tr><td colspan=\"2\" style=\"border: 1px solid black; padding-left: 5px; padding-right: 5px;\"><b>Credit Value of Unit: </b>" + this.creditValue + "</td>" +
                "<td colspan=\"3\" style=\"border: 1px solid black; padding-left: 5px; padding-right: 5px;\"><b>Date Issued: </b>" + this.createdDate.ToShortDateString() + "</td>" +
                "</tr></tbody>" +
                "<tbody><tr>" +
                "<td colspan=\"2\" style=\"border: 1px solid black; padding-left: 5px; padding-right: 5px; word-wrap: break-word;\"><b>Marker(s): </b>" + formattedMarkersStr + "</td>" +
                "<td colspan=\"3\" style=\"border: 1px solid black; padding-left: 5px; padding-right: 5px; word-wrap: break-word;\"><b>Submission Due Date: </b>" + this.submissionDueDate.ToShortDateString() + "<b> Time: </b>" + this.submissionDueDate.ToShortTimeString() + "</td>" +
                "</tr>" +
                "<tr>" +
                "<td colspan=\"2\" style=\"border: 1px solid black; padding-left: 5px; padding-right: 5px; word-wrap: break-word;\"><b>Quality Assessor: </b>" + this.qualityAssessor + "</td>" +
                "<td colspan=\"3\" style=\"border: 1px solid black; padding-left: 5px; padding-right: 5px; word-wrap: break-word;\"><b>Submission Location: </b>" + this.submissionLocation + "</td>" +
                "</tr>" +
                "<tr>" +
                "<td colspan=\"2\" style=\"border-left: 0px solid black; border-bottom: 0px solid black; border-top: 1px solid black; border-right: 1px solid black; padding-left: 5px; padding-right: 5px;\">&nbsp</td>" +
                "<td colspan=\"3\" style=\"border: 1px solid black; padding-left: 5px; padding-right: 5px; word-wrap: break-word;\"><b>Feedback Method: </b>" + this.feedbackMethod + "</td>" +
                "</tr>" +
                "<tr>" +
                "<td colspan=\"5\" style=\"border: 0px solid black; line-height: 0.3; padding-left: 5px; padding-right: 5px;\">&nbsp</td>" +
                "</tr>" +
                "</tbody></table>" +
                "<h1 style=\"font-family: Arial; font-size: 10pt; font-weight:bold; background-color: #E5E5E5; text-align: center; padding-top: 5px; padding-bottom: 10px; margin: 0px; line-height: 1.5;\">" + formattedWeightingStr + "</h1>";
            string EXPORT_ASMT_TASK_BLOCK =
                "<h2 style=\"padding-top: 10px; font-family: Arial; font-weight: bold; margin-bottom: 2px;\">ASSESSMENT TASK 1</h2>" +
                "<p style=\"line-height: 1.1; margin-top: 0px;\">" + encodeImageSources(this.assessmentTaskHtml) + "</p>";
            string EXPORT_SUBMISSION_FORMAT_BLOCK =
                "<h2 style=\"padding-top: 10px; font-family: Arial; font-weight: bold; margin-bottom: 2px;\">SUBMISSION FORMAT</h2>" +
                "<p style=\"line-height: 1.1; margin-top: 0px;\">" + encodeImageSources(this.submissionFormatHtml) + "</p>";
            string EXPORT_MARKING_CRITERIA_BLOCK =
                "<h2 style=\"padding-top: 10px; font-family: Arial; font-weight: bold; margin-bottom: 2px;\">MARKING CRITERIA</h2>" +
                "<p style=\"line-height: 1.1; margin-top: 0px;\">" + encodeImageSources(this.markingCriteriaHtml) + "</p>";
            string EXPORT_ILOS_BLOCK =
                "<h2 style=\"padding-top: 10px; font-family: Arial; font-weight: bold; margin-bottom: 2px;\">INTENDED LEARNING OUTCOMES</h2>" +
                "<p style=\"line-height: 1.1; margin-top: 0px;\">" + formattedILOsStr + "</p>";
            string EXPORT_QUESTIONS_BLOCK =
                "<h2 style=\"padding-top: 10px; font-family: Arial; font-weight: bold; margin-bottom: 2px;\">QUESTIONS ABOUT THE BRIEF</h2>" +
                "<p style=\"line-height: 1.1; margin-top: 0px;\">" + this.questionsAboutBrief + "</p>";
            string EXPORT_SIGNATURE_BLOCK =
                "<h2 style=\"padding-top: 10px; font-family: Arial; font-weight: normal; margin-top: 10px; margin-bottom: 10px;\"><b>Signature Marker: </b>" + this.signatureMarker + "</h2>";
            string EXPORT_SUPPORT_BLOCK =
                "<h2 style=\"padding-top: 10px; font-family: Arial; font-weight: bold; margin-bottom: 2px;\">HELP AND SUPPORT</h2>" +
                "<p style=\"line-height: 1.1; margin-top: 0px;\">something here</p>";
            string EXPORT_END_BLOCK =
                "</section>" +
                "</body></html>";
            return 
                EXPORT_HEAD_BLOCK + 
                EXPORT_STYLE_BLOCK + 
                EXPORT_DETAILS_BLOCK + 
                EXPORT_ASMT_TASK_BLOCK + 
                EXPORT_SUBMISSION_FORMAT_BLOCK + 
                EXPORT_MARKING_CRITERIA_BLOCK + 
                EXPORT_ILOS_BLOCK + 
                EXPORT_QUESTIONS_BLOCK + 
                EXPORT_SIGNATURE_BLOCK +
                EXPORT_SUPPORT_BLOCK + 
                EXPORT_END_BLOCK;
        }

        /// <summary>
        /// Converts the specified HTML document string to an assessment brief PDF
        /// </summary>
        /// <param name="htmlDocument">The HTML document string to convert to PDF</param>
        /// <param name="backgroundThread">The thread to report progress percentage to</param>
        public void convertHtmlToPdf(string htmlDocument, BackgroundWorker backgroundThread)
        {
            backgroundThread.ReportProgress(0);
            SelectPdf.PdfHtmlSection headerHtml = new SelectPdf.PdfHtmlSection("<p style=\"font-family: Arial; font-size: 7pt; text-align: right;\">June 2019 v1</p>", "");
            headerHtml.AutoFitHeight = SelectPdf.HtmlToPdfPageFitMode.AutoFit;
            SelectPdf.HtmlToPdf converter = new SelectPdf.HtmlToPdf();

            converter.Options.MarginTop = 39;
            converter.Options.MarginBottom = 53;
            converter.Options.MarginLeft = 68;
            converter.Options.MarginRight = 68; //72 = 1 inch (36 = 0.5 inch)
            converter.Options.DisplayHeader = true;
            converter.Options.DisplayFooter = true;
            converter.Header.Height = 14;
            converter.Header.Add(headerHtml);
            backgroundThread.ReportProgress(10);

            try {
                SelectPdf.PdfDocument doc = converter.ConvertHtmlString(htmlDocument);
                backgroundThread.ReportProgress(80);
                Console.WriteLine("Resulting page count: " + converter.ConversionResult.PdfPageCount);

                doc.Save("temp\\exported brief.pdf");
                doc.Close();
                backgroundThread.ReportProgress(90);
            } catch(OutOfMemoryException) {
                Console.Error.WriteLine("Too much image data URI content - out of memory!");
                //TODO: output to HTML only instead
            } catch(Exception e) {
                Console.Error.WriteLine(e.ToString());
                //TODO: output to HTML only instead
            }
        }

        #region based on https://www.c-sharpcorner.com/blogs/how-to-merge-multiple-pdf-files-with-page-number-using-pdfsharp-in-c-sharp
        /// <summary>
        /// Merges the specified PDF files to a document that is output to the specified file
        /// And adds page numbers for the brief
        /// And adds "DRAFT" watermark for the brief
        /// </summary>
        /// <param name="outputFilePath">The destination for the merged and finalised PDF file</param>
        /// <param name="appendixFilePaths">The list of PDF files to stitch together - the first should be the brief</param>
        /// <param name="includeWatermark">Whether or not to add a "DRAFT" watermark to this export document</param>
        public void mergeMultiplePDFIntoSinglePDF(string outputFilePath, List<string> appendixFilePaths, bool includeWatermark)
        {
            int pageCountOnlyForBrief = 0;

            PdfSharp.Pdf.PdfDocument finalPdfDocument = new PdfSharp.Pdf.PdfDocument();
            for(int fileCount = 0; fileCount < appendixFilePaths.Count; fileCount++)
            {
                PdfSharp.Pdf.PdfDocument pdfDocument = PdfSharp.Pdf.IO.PdfReader.Open(appendixFilePaths[fileCount], PdfSharp.Pdf.IO.PdfDocumentOpenMode.Import);
                finalPdfDocument.Version = pdfDocument.Version; //necessary?
                foreach (PdfSharp.Pdf.PdfPage pdfPage in pdfDocument.Pages)
                {
                    if(fileCount == 0) {
                        pageCountOnlyForBrief++;
                    }
                    finalPdfDocument.AddPage(pdfPage);
                }
            }
            //The PDF brief without appendices is only a temporary document, delete it once it's been appended to the final document
            File.Delete(appendixFilePaths[0]);

            //Set font for paging  
            PdfSharp.Drawing.XFont footerFont = new PdfSharp.Drawing.XFont("Arial", 10);
            PdfSharp.Drawing.XBrush footerFontColour = PdfSharp.Drawing.XBrushes.Black;

            //Set font for watermark
            PdfSharp.Drawing.XFont watermarkFont = new PdfSharp.Drawing.XFont("Arial", 120);
            PdfSharp.Drawing.XBrush watermarkFontColour = PdfSharp.Drawing.XBrushes.Red;

            //Set for loop of document page count and set page number using DrawString function of PdfSharp  
            for (int pageCount = 0; pageCount < pageCountOnlyForBrief; ++pageCount)
            {
                PdfSharp.Pdf.PdfPage page = finalPdfDocument.Pages[pageCount];

                //Draw page numbers
                PdfSharp.Drawing.XRect footerArea = new PdfSharp.Drawing.XRect(0 /*X*/ , page.Height - 30 /*Y*/ , page.Width /*Width*/ , footerFont.Height /*Height*/ );
                PdfSharp.Drawing.XGraphics gfx = PdfSharp.Drawing.XGraphics.FromPdfPage(page); //FromPdfPage specifies the page to start drawing on (i.e. the current page in the loop)
                gfx.DrawString("Page " + (pageCount + 1).ToString() + " of " + pageCountOnlyForBrief, footerFont, footerFontColour, footerArea, PdfSharp.Drawing.XStringFormats.Center); //XStringFormats.Center centers the string within the footerarea
                gfx.Dispose();

                if(includeWatermark) {
                    //Draw watermark
                    PdfSharp.Drawing.XRect watermarkArea = new PdfSharp.Drawing.XRect(0 - (page.Width / 4) /*X*/, (page.Height / 2) - watermarkFont.Height /*Y*/, page.Width, (page.Height / 2));
                    PdfSharp.Drawing.XGraphics wfx = PdfSharp.Drawing.XGraphics.FromPdfPage(page);

                    PdfSharp.Drawing.XGraphicsState state = wfx.Save();
                    wfx.RotateAtTransform(-20, new PdfSharp.Drawing.XPoint(0, 0)); //Rotate text by -20 degrees
                    wfx.DrawString("DRAFT", watermarkFont, watermarkFontColour, watermarkArea, PdfSharp.Drawing.XStringFormats.Center);
                    wfx.Restore(state);
                    wfx.Dispose();
                }
            }

            finalPdfDocument.Options.CompressContentStreams = true;
            finalPdfDocument.Options.NoCompression = false;

            // In the final stage, all documents are merged and save in your output file path.
            try {
                finalPdfDocument.Save(outputFilePath);
            } catch(IOException) {
                //File cannot be accessed (e.g. used by another process)
                Console.WriteLine("ERROR: Unable to write to " + outputFilePath + " - used by another process?");
                MessageBox.Show("Unable to write to " + outputFilePath + ". It might be locked by another process.", "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        //Metadata
        public DateTime createdDate { get; set; }
        public float templateVer { get; set; }

        //TitleLevelPage
        public string unitTitle { get; set; }
        public string asmtTitle { get; set; }
        public int level { get; set; }
        public bool isResub { get; set; }
        public int asmtNoX { get; set; }
        public int asmtNoY { get; set; }

        //WeightDatePage
        public bool isGroup { get; set; }
        public int weighting { get; set; }
        public int creditValue { get; set; }
        public DateTime submissionDueDate { get; set; }
        public string impPath { get; set; }

        //StaffSubPage
        public string unitLeader { get; set; }
        public string qualityAssessor { get; set; }
        public List<string> markersList { get; set; }
        public string submissionLocation { get; set; }
        public string feedbackMethod { get; set; }

        //RichtextPage (Reusable)
        public string assessmentTaskHtml { get; set; }
        public string submissionFormatHtml { get; set; }
        public string markingCriteriaHtml { get; set; }

        //ILOsPage
        public List<string> ILOsList { get; set; }

        //QuestionsSignaturePage
        public string questionsAboutBrief { get; set; }
        public string signatureMarker { get; set; }
    }
}
