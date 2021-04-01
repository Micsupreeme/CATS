using System;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace CATS
{
    /// <summary>
    /// Interaction logic for QuestionsSignaturePage.xaml
    /// </summary>
    public partial class QuestionsSignaturePage : Page
    {
        private BUAssessment currentBua;

        public QuestionsSignaturePage(BUAssessment bua)
        {
            InitializeComponent();
            currentBua = bua;
            populateFields();
        }

        /// <summary>
        /// Upon page load, populate the fields with the values stored in the bua object
        /// </summary>
        private void populateFields()
        {
            questionsTxt.Text = prepareHtml(currentBua.questionsAboutBrief, false);

            //if no explicit signature marker is set, autofill the unit leader
            if(currentBua.signatureMarker.Length > 0) {
                signatureTxt.Text = currentBua.signatureMarker;
            } else {
                signatureTxt.Text = currentBua.unitLeader;
            }
        }

        /// <summary>
        /// Adds/removes HTML visual clutter tags (e.g. "<br>") for webbrowsers/humans
        /// </summary>
        /// <param name="html">The HTML string to transform</param>
        /// <param name="forWeb">If true, prepares the HTMl for the web (adds tags), else prepares the HTML for a human (removes tags)</param>
        /// <returns>The transformed version of the "html" input string</returns>
        private string prepareHtml(string html, bool forWeb)
        {
            string treatedHtml;
            if (forWeb) {
                treatedHtml = Regex.Replace(html, Environment.NewLine, "<br>"); //Prevent users from having to use <br> tags manually
            } else {
                treatedHtml = Regex.Replace(html, "<br>", Environment.NewLine); //Users should not have to see <br> tags as they are visual clutter, remove them
            }
            return treatedHtml;
        }

        #region Event handlers
        /// <summary>
        /// When the contents of the questions-about-the-brief multiline textbox changes
        /// </summary>
        private void questionsTxt_TextChanged(object sender, TextChangedEventArgs e)
        {
            try {
                //Keep this code absolutely minimal, as this event fires every time a character changes in the textbox
                currentBua.questionsAboutBrief = prepareHtml(questionsTxt.Text, true);
            } catch (NullReferenceException) {
                Console.Error.WriteLine("WARN: Event fired before object initialisation ");
            }
        }

        /// <summary>
        /// When the contents of the signature marker text box changes
        /// </summary>
        private void signatureTxt_TextChanged(object sender, TextChangedEventArgs e)
        {
            try {
                //Keep this code absolutely minimal, as this event fires every time a character changes in the textbox
                currentBua.signatureMarker = signatureTxt.Text;
            } catch (NullReferenceException) {
                Console.Error.WriteLine("WARN: Event fired before object initialisation ");
            }
        }
        #endregion
    }
}
