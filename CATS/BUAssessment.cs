using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using HtmlAgilityPack;
using Newtonsoft.Json;


namespace CATS
{
    public class BUAssessment
    {
        public BUAssessment()
        {
            setDefaultValues();
        }

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

            //TextEditorPage (Reusable)
            this.assessmentTaskHtml = String.Empty;
            this.submissionFormatHtml = String.Empty;
            this.markingCriteriaHtml = String.Empty;
            this.questionsAboutBriefHtml = String.Empty;

            //ILOsPage
            this.ILOsList = new List<string>();
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

        //TODO: expand on these export-related methods and replace image sources with Base64 data URIs so that the final HTML (and PDF) is essentially packaged WITH its included images
        private string getBase64ImageString(string fullImagePath)
        {
            byte[] imageBytes = File.ReadAllBytes(fullImagePath);
            return Convert.ToBase64String(imageBytes);
        }

        /// <summary>
        /// Converts the filepath image sources within the provided HTML to base 64 data sources
        /// </summary>
        /// <param name="html">The HTML to be treated</param>
        /// <returns>The HTML with image sources replaced with base 64 strings</returns>
        public string encodeImageSources(string html)
        {
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);
            try
            {
                foreach (HtmlNode imageNode in htmlDoc.DocumentNode.SelectNodes("//img"))
                {
                    string srcVal = imageNode.Attributes["src"].Value;

                    string base64Prefix = "data:image/";
                    switch (srcVal.Substring(srcVal.Length - 3))
                    {
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

        public string getHtmlDocument()
        {
            string EXPORT_HEAD_BLOCK =
               "<html lang = \"en\"><head><title>" + this.asmtTitle + "</title>";
             string EXPORT_STYLE_BLOCK =
            "<style type=\"text/css\">" +
                "h1 {font-size: 12pt;}" +
                "h1, h2 {font-weight: bold;}" +
                "ol, ul {margin-top: 3px; margin-bottom: 3px;}" +
                "td, th {padding: 2px; border: 1px solid black;}" +
                "table {width: 95%; border-collapse: collapse;}" +
                "img {page-break-inside: avoid;}" +
                "img, table {display: block; margin-left: auto; margin-right: auto;}" +
                ".caption {text-align: center; display: block; margin-left: auto; margin-right: auto;}" +
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
                "<td colspan=\"2\" style=\"border: 1px solid black; padding-left: 5px; padding-right: 5px; word-wrap: break-word;\"><b>Marker(s): </b>" + this.markersList[0] + "</td>" + //TODO include all markers
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
                "<h1 style=\"font-family: Arial; font-size: 10pt; font-weight:bold; background-color: #E5E5E5; text-align: center; padding-top: 5px; padding-bottom: 10px; margin: 0px; line-height: 1.5;\">This is an individual assignment " + !this.isGroup + " which carries " + this.weighting + "% of the final unit mark</h1>"; //TODO: format string properly
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
                "<p style=\"line-height: 1.1; margin-top: 0px;\">" + "ILOS HERE" + "</p>"; //TODO: include all ILOs
            string EXPORT_QUESTIONS_BLOCK =
                "<h2 style=\"padding-top: 10px; font-family: Arial; font-weight: bold; margin-bottom: 2px;\">QUESTIONS ABOUT THE BRIEF</h2>" +
                "<p style=\"line-height: 1.1; margin-top: 0px;\">" + encodeImageSources(this.questionsAboutBriefHtml)+ "</p>";
            string EXPORT_SIGNATURE_BLOCK =
                "<h2 style=\"padding-top: 10px; font-family: Arial; font-weight: normal; margin-top: 10px; margin-bottom: 10px;\"><b>Signature Marker: </b>" + this.unitLeader + "</h2>";
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

        public void convertHtmlToPdf(string htmlDocument)
        {
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

            SelectPdf.PdfDocument doc = converter.ConvertHtmlString(htmlDocument);
            Console.WriteLine("Resulting page count: " + converter.ConversionResult.PdfPageCount);

            doc.Save("exported brief.pdf");
            doc.Close();
        }

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

        //TextEditorPage (Reusable)
        public string assessmentTaskHtml { get; set; }
        public string submissionFormatHtml { get; set; }
        public string markingCriteriaHtml { get; set; }
        public string questionsAboutBriefHtml { get; set; }

        //ILOsPage
        public List<string> ILOsList { get; set; }
    }
}
