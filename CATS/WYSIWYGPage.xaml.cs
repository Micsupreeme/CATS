using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Smith.WPF.HtmlEditor;

namespace CATS
{
    /// <summary>
    /// Interaction logic for WYSIWYGPage.xaml
    /// </summary>
    public partial class WYSIWYGPage : Page
    {
        private const string ASSESSMENT_TASK_PROMPT =
            "";
        private const string SUBMISSION_FORMAT_PROMPT =
            "Requirements for the format of what is to be submitted, including word count or its equivalence, " +
            "details of electronic copies/hard copies, where/how to submit...";
        private const string MARKING_CRITERIA_PROMPT =
            "Ensure these map to ILOs. The following criteria will be used to assess the assignment...";
        private const string QUESTIONS_ABOUT_BRIEF_PROMPT =
            "Describe how questions about the brief will be handled (e.g., tutorials/seminar/forum)...";

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
                    currentBua.assessmentTaskHtml = smithEditor.ContentHtml;
                    break;
                case "Submission Format":
                    currentBua.submissionFormatHtml = smithEditor.ContentHtml;
                    break;
                case "Marking Criteria":
                    currentBua.markingCriteriaHtml = smithEditor.ContentHtml;
                    break;
                default:
                    Console.Error.WriteLine("ERROR: Invalid HTML editing area specified");
                    break;
            }
            currentBua.saveAsJson(callingWindow.currentFilePath);
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
        }
    }
}
