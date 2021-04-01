using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using HtmlAgilityPack;

namespace CATS
{
    //POSSIBLE BUG: Smith WPF editor image resize arrows sometimes don't work properly
    //(even if you drag outwards, the image just gets smaller in blocky increments) - used source mode afterwards, caused COM exception?

    /// <summary>
    /// Interaction logic for WYSIWYGPage.xaml
    /// </summary>
    public partial class WYSIWYGPage : Page
    {
        private const int MAXIMUM_IMAGE_WIDTH = 1000; //After 1000px, images start changing the proportions of the document

        private const string ASSESSMENT_TASK_PROMPT =
            "";
        private const string SUBMISSION_FORMAT_PROMPT =
            "Requirements for the format of what is to be submitted, including word count or its equivalence, " +
            "details of electronic copies/hard copies, where/how to submit...";
        private const string MARKING_CRITERIA_PROMPT =
            "Ensure these map to ILOs. The following criteria will be used to assess the assignment...";

        private PagedWindow callingWindow;
        private BUAssessment currentBua;
        private string areaBeingEdited;

        public WYSIWYGPage(PagedWindow caller, BUAssessment bua, string area)
        {
            InitializeComponent();
            smithEditor.DocumentReady += new RoutedEventHandler(smithEditor_DocumentReady);
            smithEditor.DocumentStateChanged += new RoutedEventHandler(smithEditor_DocumentStateChanged);
            smithEditor.ContentHtml = "<P align=center><FONT color=#99ccff size=6></FONT>&nbsp;</P><P align=center><FONT color=#99ccff size=6>Move mouse here...</FONT></P>";
            callingWindow = caller;
            currentBua = bua;
            areaBeingEdited = area;
            editAreaTb.Text = areaBeingEdited + " editor:";
            populateFields();
        }

        private void startSaveCooldown()
        {
            DispatcherTimer saveCooldownTimer = new DispatcherTimer();
            saveCooldownTimer.Tick += saveCooldownTimer_Tick;
            saveCooldownTimer.Interval = new TimeSpan(0, 0, 3);
            saveCooldownTimer.Start();
        }

        private void saveCooldownTimer_Tick(object sender, EventArgs e)
        {
            saveBtn.Content = "Save Changes";
        }

        /// <summary>
        /// Upon page load, populate the HTML editor with the HTML body stored in the bua object
        /// </summary>
        private void populateFields()
        {
            switch (areaBeingEdited)
            {
                case "Assessment Task":
                    editSideTb.Text = ASSESSMENT_TASK_PROMPT;
                    break;
                case "Submission Format":
                    editSideTb.Text = SUBMISSION_FORMAT_PROMPT;
                    break;
                case "Marking Criteria":
                    editSideTb.Text = MARKING_CRITERIA_PROMPT;
                    break;
                default:
                    Console.Error.WriteLine("ERROR: Invalid HTML editing area specified");
                    break;
            }
        }

        /// <summary>
        /// Saves changes made in the editor to the BUAssessment object and its save file
        /// </summary>
        private void saveChanges()
        {
            switch (areaBeingEdited)
            {
                case "Assessment Task":
                    currentBua.assessmentTaskHtml = limitOversizedImages(fixImageStyleSizes(smithEditor.ContentHtml));
                    smithEditor.ContentHtml = currentBua.assessmentTaskHtml;
                    break;
                case "Submission Format":
                    currentBua.submissionFormatHtml = limitOversizedImages(fixImageStyleSizes(smithEditor.ContentHtml));
                    smithEditor.ContentHtml = currentBua.submissionFormatHtml;
                    break;
                case "Marking Criteria":
                    currentBua.markingCriteriaHtml = limitOversizedImages(fixImageStyleSizes(smithEditor.ContentHtml));
                    smithEditor.ContentHtml = currentBua.markingCriteriaHtml;
                    break;
                default:
                    Console.Error.WriteLine("ERROR: Invalid HTML editing area specified");
                    break;
            }
            currentBua.saveAsJson(callingWindow.currentFilePath);
            startSaveCooldown();
        }

        /// 1000 pixels is the maximum width an image can have before it starts changing the proportions of the rest of the document
        private string limitOversizedImages(string html)
        {
            HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();
            try {
                htmlDoc.LoadHtml(html);
            } catch (ArgumentNullException) {
                Console.WriteLine("WARN: No HTML to process!");
                return String.Empty;
            }

            try {
                foreach (HtmlNode imageNode in htmlDoc.DocumentNode.SelectNodes("//img")) {
                    string srcVal = imageNode.Attributes["width"].Value;

                    try {
                        int widthVal = Int32.Parse(imageNode.Attributes["width"].Value);
                        if (widthVal > MAXIMUM_IMAGE_WIDTH) {
                            imageNode.Attributes["width"].Value = MAXIMUM_IMAGE_WIDTH + "";
                            imageNode.Attributes["height"].Value = "auto"; //Automatic height according to the image ratio
                        }
                    } catch (Exception) {
                        Console.WriteLine("WARN: Attempted to convert alphabet to int!");
                    }
                }
            } catch (NullReferenceException) {
                Console.Error.WriteLine("ERROR: No image nodes returned during image encoding");
            }
            return htmlDoc.DocumentNode.OuterHtml;
        }

        /// When an image is resized in Smith.WPF.Htmleditor, the new size is stored as a width and height inside the style attribute.
        /// This method moves these attributes out from the style attribute and back into the regular width and height attributes.
        /// Keeping width and height in the same attributes makes it easier to enforce width and height regulations.
        private string fixImageStyleSizes(string html)
        {
            HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();
            try {
                htmlDoc.LoadHtml(html);
            } catch (ArgumentNullException) {
                Console.WriteLine("WARN: No HTML to process!");
                return String.Empty;
            }

            try {
                foreach (HtmlNode imageNode in htmlDoc.DocumentNode.SelectNodes("//img")) {

                    if (imageNode.Attributes["style"] != null) {
                        //This image has a style attribute
                        Console.WriteLine("processing an image style attribute");

                        //Split the style attribute into seperate attributes and remove spaces
                        string[] styleAttributes = imageNode.Attributes["style"].Value.Split(';');
                        for(int style = 0; style < styleAttributes.Length; style++) {
                            styleAttributes[style] = Regex.Replace(styleAttributes[style], " ", ""); //remove spaces
                        }

                        //Split the seperated style attributes into key-value pairs (stored in parallel lists)
                        List<string> fullSplitNames = new List<string>();
                        List<string> fullSplitValues = new List<string>();
                        foreach(string styleAtt in styleAttributes)
                        {
                            string[] attValSplit = styleAtt.Split(':');
                            fullSplitNames.Add(attValSplit[0]); //Key (e.g., height)
                            fullSplitValues.Add(attValSplit[1]); //Value (e.g., 20px)
                        }

                        //If "width" and "height" style attributes are found, move those values to the regular "width" and "height" attributes
                        string rebuiltStyleAttribute = String.Empty;
                        for (int s = 0; s < fullSplitNames.Count; s++)
                        {
                            if(fullSplitNames[s].Equals("WIDTH")) {
                                //Found style width attribute - make the regular width attribute this value
                                imageNode.Attributes["width"].Value = fullSplitValues[s];
                            } else if(fullSplitNames[s].Equals("HEIGHT")) {
                                //Found style height attribute - make the regular height attribute this value
                                imageNode.Attributes["height"].Value = fullSplitValues[s];
                            } else {
                                //This preserves any non-width and non-height style attributes - effectively moving them out of the style attributes
                                rebuiltStyleAttribute += fullSplitNames[s] + ": " + fullSplitValues[s] + ";";
                            }
                        }
                        //Update the style tag to the rebuilt version which omits width and height
                        imageNode.Attributes["style"].Value = rebuiltStyleAttribute;
                    }
                }
            } catch (NullReferenceException) {
                Console.Error.WriteLine("ERROR: No image nodes returned during image encoding");
            }
            return htmlDoc.DocumentNode.OuterHtml;
        }

        /// <summary>
        /// When the Smith editor finishes loading
        /// </summary>
        private void smithEditor_DocumentReady(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("doucmentready");
            switch (areaBeingEdited)
            {
                case "Assessment Task":
                    smithEditor.ContentHtml = currentBua.assessmentTaskHtml;
                    break;
                case "Submission Format":
                    smithEditor.ContentHtml = currentBua.submissionFormatHtml;
                    break;
                case "Marking Criteria":
                    smithEditor.ContentHtml = currentBua.markingCriteriaHtml;
                    break;
                default:
                    Console.Error.WriteLine("ERROR: Invalid HTML editing area specified");
                    break;
            }
        }

        private void smithEditor_DocumentStateChanged(object sender, RoutedEventArgs e)
        {
            //This event seems to fire quite randomly
        }

        /// <summary>
        /// When the save changes button is clicked
        /// </summary>
        private void saveBtn_Click(object sender, RoutedEventArgs e)
        {
            saveChanges();
            saveBtn.Content = "Saved";
        }
    }
}
