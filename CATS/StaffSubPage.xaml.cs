using System;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CATS
{
    /// <summary>
    /// Interaction logic for StaffSubPage.xaml
    /// </summary>
    public partial class StaffSubPage : Page
    {
        private BUAssessment currentBua;
        private bool markersPopulated = false; //has the markers multiline field been initially filled?

        public StaffSubPage(BUAssessment bua)
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
            unitLeaderTxt.Text = currentBua.unitLeader;
            qualityAssessorTxt.Text = currentBua.qualityAssessor;

            for (int m = 0; m < currentBua.markersList.Count; m++)
            {
                markersTxt.Text += currentBua.markersList[m];
                if (m != (currentBua.markersList.Count - 1))
                {
                    markersTxt.Text += Environment.NewLine;
                }
            }
            markersPopulated = true;

            switch(currentBua.submissionLocation)
            {
                case "Turnitin":
                    subLocationCmb.SelectedIndex = 0;
                    break;
                case "Turnitin (and large file submission)":
                    subLocationCmb.SelectedIndex = 1;
                    break;
                case "Test (in class)":
                    subLocationCmb.SelectedIndex = 2;
                    break;
                case "Test (online)":
                    subLocationCmb.SelectedIndex = 3;
                    break;
                case "Presentation (in class)":
                    subLocationCmb.SelectedIndex = 4;
                    break;
                case "Presentation (Panopto)":
                    subLocationCmb.SelectedIndex = 5;
                    break;
                case "Hard copy":
                    subLocationCmb.SelectedIndex = 6;
                    break;
                case "Large file":
                    subLocationCmb.SelectedIndex = 7;
                    break;
                case "Online blog":
                    subLocationCmb.SelectedIndex = 8;
                    break;
                case "Online independent platform":
                    subLocationCmb.SelectedIndex = 9;
                    break;
                case "Online wiki":
                    subLocationCmb.SelectedIndex = 10;
                    break;
                case "Peer assessment":
                    subLocationCmb.SelectedIndex = 11;
                    break;
                case "Portfolio (Mahara)":
                    subLocationCmb.SelectedIndex = 12;
                    break;
                case "Portfolio (Opal)":
                    subLocationCmb.SelectedIndex = 13;
                    break;
                case "Practical assessment":
                    subLocationCmb.SelectedIndex = 14;
                    break;
                case "Video submission (Panopto)":
                    subLocationCmb.SelectedIndex = 15;
                    break;
                case "Other":
                    subLocationCmb.SelectedIndex = 16;
                    break;
                default:
                    subLocationCmb.SelectedIndex = 0;
                    Console.Error.WriteLine("ERROR: Invalid Submission Location specified");
                    break;
            }

            switch(currentBua.feedbackMethod)
            {
                case "Turnitin":
                    fedMethodCmb.SelectedIndex = 0;
                    break;
                case "Brightspace":
                    fedMethodCmb.SelectedIndex = 1;
                    break;
                case "Hard copy":
                    fedMethodCmb.SelectedIndex = 2;
                    break;
                case "In class":
                    fedMethodCmb.SelectedIndex = 3;
                    break;
                case "Other":
                    fedMethodCmb.SelectedIndex = 4;
                    break;
                default:
                    fedMethodCmb.SelectedIndex = 0;
                    Console.Error.WriteLine("ERROR: Invalid Feedback Method specified");
                    break;
            }
        }

        private void unitLeaderTxt_TextChanged(object sender, TextChangedEventArgs e)
        {
            try {
                //Keep this code absolutely minimal, as this event fires every time a character changes in the textbox
                currentBua.unitLeader = unitLeaderTxt.Text;
            } catch (NullReferenceException) {
                Console.Error.WriteLine("WARN: Event fired before object initialisation ");
            }
        }

        /// <summary>
        /// NOTE: This method fires as soon as the page loads and even while the markers field is being initially populated
        /// </summary>
        private void markersTxt_TextChanged(object sender, TextChangedEventArgs e)
        {
            try {
                //Keep this code absolutely minimal, as this event fires every time a character changes in the textbox
                if (markersPopulated) { //prevent this from running if it was triggered while initially populating the field
                    currentBua.markersList = new List<string>(markersTxt.Text.Split(new string[] { Environment.NewLine }, StringSplitOptions.None));                  
                }
            } catch (NullReferenceException) {
                Console.Error.WriteLine("WARN: Event fired before object initialisation ");
            }
        }

        private void qualityAssessorTxt_TextChanged(object sender, TextChangedEventArgs e)
        {
            try {
                //Keep this code absolutely minimal, as this event fires every time a character changes in the textbox
                currentBua.qualityAssessor = qualityAssessorTxt.Text;
            } catch (NullReferenceException) {
                Console.Error.WriteLine("WARN: Event fired before object initialisation ");
            }
        }

        private void subLocationCmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try {
                currentBua.submissionLocation = subLocationCmb.SelectedValue.ToString();
            } catch (NullReferenceException) {
                Console.Error.WriteLine("WARN: Event fired before object initialisation");
            }
        }

        private void fedMethodCmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try {
                currentBua.feedbackMethod = fedMethodCmb.SelectedValue.ToString();
            } catch (NullReferenceException) {
                Console.Error.WriteLine("WARN: Event fired before object initialisation");
            }
        }
    }
}
