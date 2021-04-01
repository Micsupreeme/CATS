using System;
using System.Windows.Controls;

namespace CATS
{
    /// <summary>
    /// Interaction logic for TitleLevelPage.xaml
    /// </summary>
    public partial class TitleLevelPage : Page
    {
        private BUAssessment currentBua;

        public TitleLevelPage(BUAssessment bua)
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
            unitTitleTxt.Text = currentBua.unitTitle;
            asmtTitleTxt.Text = currentBua.asmtTitle;

            switch(currentBua.level)
            {
                case 4:
                    levelCmb.SelectedIndex = 0;
                    break;
                case 5:
                    levelCmb.SelectedIndex = 1;
                    break;
                case 6:
                    levelCmb.SelectedIndex = 2;
                    break;
                case 7:
                    levelCmb.SelectedIndex = 3;
                    break;
                default:
                    Console.WriteLine("ERROR: Invalid level specified");
                    levelCmb.SelectedIndex = 0;
                    break;
            }

            if(!currentBua.isResub) {
                subResubCmb.SelectedIndex = 0;
                asmtNoXCmb.SelectedIndex = currentBua.asmtNoX - 1;
                asmtNoYCmb.SelectedIndex = currentBua.asmtNoY - 1;
            } else {
                subResubCmb.SelectedIndex = 1;
            }
        }

        /// <summary>
        /// Enables/disables the assessment number controls depending on the boolean input
        /// </summary>
        /// <param name="isResub">Is "Resubmission" the currently selected item in the combobox?</param>
        private void setSubState(bool isResub)
        {
            try {
                if (!isResub) {
                    //Submission state
                    asmtNoXCmb.SelectedIndex = 0;
                    asmtNoYCmb.SelectedIndex = 1;
                    asmtNoXCmb.IsEnabled = true;
                    asmtNoYCmb.IsEnabled = true;
                } else {
                    //Resubmission state
                    asmtNoXCmb.SelectedIndex = 0;
                    asmtNoYCmb.SelectedIndex = 0;
                    asmtNoXCmb.IsEnabled = false;
                    asmtNoYCmb.IsEnabled = false; //Resubs are always "1 of 1"
                }
            } catch(NullReferenceException) {
                Console.Error.WriteLine("WARN: Attempted to access uninstantiated control");
            }
        }

        #region Event handlers
        /// <summary>
        /// Update the bua object to match the unit title textbox content
        /// When the contents of the unit leader textbox changes
        /// </summary>
        private void unitTitleTxt_TextChanged(object sender, TextChangedEventArgs e)
        {
            try {
                //Keep this code absolutely minimal, as this event fires every time a character changes in the textbox
                currentBua.unitTitle = unitTitleTxt.Text;
            } catch (NullReferenceException) {
                Console.Error.WriteLine("WARN: Event fired before object initialisation ");
            }
        }

        /// <summary>
        /// Update the bua object to match the assessment title textbox content
        /// When the contents of the assessment title textbox changes
        /// </summary>
        private void asmtTitleTxt_TextChanged(object sender, TextChangedEventArgs e)
        {
            try {
                //Keep this code absolutely minimal, as this event fires every time a character changes in the textbox
                currentBua.asmtTitle = asmtTitleTxt.Text;
            } catch (NullReferenceException) {
                Console.Error.WriteLine("WARN: Event fired before object initialisation");
            }
        }

        /// <summary>
        /// Update the bua object to match the level combobox content
        /// When the dropdownitem for the level field changes
        /// </summary>
        private void levelCmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try{
                currentBua.level = int.Parse(levelCmb.SelectedValue.ToString());
            } catch (FormatException) {
                Console.Error.WriteLine("ERROR: Attempted to convert alphabet to int");
            } catch (NullReferenceException) {
                Console.Error.WriteLine("WARN: Event fired before object initialisation");
            }
        }

        /// <summary>
        /// Update the bua object to match the level combobox content
        /// and enable/disable the assessment number fields depending on the selected item
        /// When the dropdownitem for the submission/resubmission field changes
        /// </summary>
        private void subResubCmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try {
                if (subResubCmb.SelectedValue.Equals("Submission")) {
                    currentBua.isResub = false;
                    setSubState(false);
                } else {
                    currentBua.isResub = true;
                    setSubState(true);
                }
            } catch(NullReferenceException) {
                Console.Error.WriteLine("WARN: Event fired before object initialisation");
            }
        }

        /// <summary>
        /// Update the bua object to match the assessment number x combobox content
        /// When the dropdownitem for the "assessment number X of Y" (where this is X) field changes
        /// </summary>
        private void asmtNoXCmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {           
            try {
                currentBua.asmtNoX = int.Parse(asmtNoXCmb.SelectedValue.ToString());
            } catch (FormatException) {
                Console.Error.WriteLine("ERROR: Attempted to convert alphabet to int");
            } catch (NullReferenceException) {
                Console.Error.WriteLine("WARN: Event fired before object initialisation");
            }
        }

        /// <summary>
        /// Update the bua object to match the assessment number y combobox content
        /// When the dropdownitem for the "assessment number X of Y" (where this is Y) field changes
        /// </summary>
        private void asmtNoYCmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try {
                currentBua.asmtNoY = int.Parse(asmtNoYCmb.SelectedValue.ToString());
            } catch (FormatException) {
                Console.Error.WriteLine("ERROR: Attempted to convert alphabet to int");
            } catch (NullReferenceException) {
                Console.Error.WriteLine("WARN: Event fired before object initialisation");
            }
        }
        #endregion
    }
}
