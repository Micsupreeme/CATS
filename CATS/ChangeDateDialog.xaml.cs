using System;
using System.Windows;
using System.Windows.Media;

namespace CATS
{
    /// <summary>
    /// Interaction logic for ChangeDateDialog.xaml
    /// </summary>
    public partial class ChangeDateDialog : Window
    {
        WeightDatePage callingPage; //Storing this enables this window to call for the WeightDate page to update its sdd value according to the newly change bua object
        private BUAssessment currentBua;

        public ChangeDateDialog(WeightDatePage caller, BUAssessment bua, bool elevate)
        {
            InitializeComponent();
            callingPage = caller;
            currentBua = bua;
            if(elevate) {
                enableElevatedOptions();
            }
            populateFields();
        }

        /// <summary>
        /// Enables elevated user exclusive options
        /// </summary>
        private void enableElevatedOptions()
        {
            //Normally these controls have greyed-out colours to indicate restricted interaction
            //Changing the colours adds a visual confirmation that they have been unlocked
            SolidColorBrush buRed2Brush = new SolidColorBrush(Color.FromRgb(243, 58, 63)); //#C32129
            SolidColorBrush whiteBrush = new SolidColorBrush(Color.FromRgb(255, 255, 255));

            changeTimehhTxt.IsReadOnly = false;
            changeTimehhTxt.Foreground = buRed2Brush;
            changeTimehhTxt.Background = whiteBrush;

            changeTimemmTxt.IsReadOnly = false;
            changeTimemmTxt.Foreground = buRed2Brush;
            changeTimemmTxt.Background = whiteBrush;

            changeTimeAMPMCmb.IsEnabled = true;
            changeTimeAMPMCmb.Foreground = buRed2Brush;
            changeTimeAMPMCmb.Background = whiteBrush;
        }

        /// <summary>
        /// Upon window load, populate the fields with the values stored in the bua object
        /// </summary>
        private void populateFields()
        {
            changeDateDDTxt.Text = currentBua.submissionDueDate.Day.ToString();
            changeDateMMTxt.Text = currentBua.submissionDueDate.Month.ToString();
            changeDateYYYYTxt.Text = currentBua.submissionDueDate.Year.ToString();
            
            //Display 24-hour times as 12-hour
            if(currentBua.submissionDueDate.Hour > 12) {
                //e.g. display the 14th hour as 2
                changeTimehhTxt.Text = currentBua.submissionDueDate.Hour - 12 + "";
            } else if(currentBua.submissionDueDate.Hour == 0) {
                //e.g. display the 0th hour as 12
                changeTimehhTxt.Text = 12 + "";
            }  else {
                changeTimehhTxt.Text = currentBua.submissionDueDate.Hour + "";
            }

            changeTimemmTxt.Text = currentBua.submissionDueDate.Minute.ToString();
            
            if(currentBua.submissionDueDate.ToString("tt").Equals("AM")) {
                changeTimeAMPMCmb.SelectedIndex = 0; //Select AM
            } else {
                changeTimeAMPMCmb.SelectedIndex = 1; //Select PM
            }
        }

        /// <summary>
        /// Updates the recieved bua object's submission due date to match the current state of the date/time fields
        /// </summary>
        private void applyDate()
        {
            try {
                DateTime specifiedDate;
                if(changeTimeAMPMCmb.SelectedValue.Equals("PM") && !changeTimehhTxt.Text.Equals("12")) {
                    //PM - Adds 12 to the hours (DateTime must take 24-hour format)
                    specifiedDate = new DateTime(int.Parse(changeDateYYYYTxt.Text), int.Parse(changeDateMMTxt.Text), int.Parse(changeDateDDTxt.Text), int.Parse(changeTimehhTxt.Text) + 12, int.Parse(changeTimemmTxt.Text), 0);
                } else if(changeTimeAMPMCmb.SelectedValue.Equals("AM") && changeTimehhTxt.Text.Equals("12")) {
                    //Midnight - normally midnight can only be specified with "0 AM", this allows "12 AM" to also count as midnight
                    specifiedDate = new DateTime(int.Parse(changeDateYYYYTxt.Text), int.Parse(changeDateMMTxt.Text), int.Parse(changeDateDDTxt.Text), 0, int.Parse(changeTimemmTxt.Text), 0);
                } else {
                    //AM - Does not add 12 to the hours
                    specifiedDate = new DateTime(int.Parse(changeDateYYYYTxt.Text), int.Parse(changeDateMMTxt.Text), int.Parse(changeDateDDTxt.Text), int.Parse(changeTimehhTxt.Text), int.Parse(changeTimemmTxt.Text), 0);
                }              
                currentBua.submissionDueDate = specifiedDate;

                callingPage.updateSddValue(false); //Indicate that this is a manual submission due date override
                this.Visibility = Visibility.Collapsed;

            } catch(ArgumentOutOfRangeException) {
                invalidDateErrorTb.Visibility = Visibility.Visible;
                Console.Error.WriteLine("ERROR: Attempted to create impossible date/time");
            }  catch(FormatException) {
                invalidDateErrorTb.Visibility = Visibility.Visible;
                Console.Error.WriteLine("ERROR: Attempted to convert alphabet to int");
            }         
        }

        #region Event handlers
        /// <summary>
        /// When the OK button is clicked
        /// </summary>
        private void okBtn_Click(object sender, RoutedEventArgs e)
        {
            applyDate();
        }

        /// <summary>
        /// When the Cancel button is clicked
        /// </summary>
        private void cancelBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
        }
        #endregion
    }
}
