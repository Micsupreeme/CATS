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

        /// <summary>
        /// On text changed, update the bua object to match the unit title textbox content
        /// </summary>
        private void unitTitleTxt_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                //Keep this code absolutely minimal, as this event fires every time a character changes in the textbox
                currentBua.unitTitle = unitTitleTxt.Text;
            }
            catch (NullReferenceException)
            {
                Console.Error.WriteLine("WARN: Event fired before object initialisation ");
            }
        }

        /// <summary>
        /// On text changed, update the bua object to match the assessment title textbox content
        /// </summary>
        private void asmtTitleTxt_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                //Keep this code absolutely minimal, as this event fires every time a character changes in the textbox
                currentBua.asmtTitle = asmtTitleTxt.Text;
            }
            catch (NullReferenceException)
            {
                Console.Error.WriteLine("WARN: Event fired before object initialisation");
            }
        }

        /// <summary>
        /// On selection changed, update the bua object to match the level combobox content
        /// </summary>
        private void levelCmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                currentBua.level = Int32.Parse(levelCmb.SelectedValue.ToString());
            } catch(NullReferenceException) {
                Console.Error.WriteLine("WARN: Event fired before object initialisation");
            }
        }

        /// <summary>
        /// On selection changed, update the bua object to match the level combobox content
        /// and enable/disable the assessment number fields depending on the selected item
        /// </summary>
        private void subResubCmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
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
        /// On selection changed, update the bua object to match the assessment number x combobox content
        /// </summary>
        private void asmtNoXCmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {           
            try {
                currentBua.asmtNoX = Int32.Parse(asmtNoXCmb.SelectedValue.ToString());
            } catch(NullReferenceException) {
                Console.Error.WriteLine("WARN: Event fired before object initialisation");
            }
        }

        /// <summary>
        /// On selection changed, update the bua object to match the assessment number y combobox content
        /// </summary>
        private void asmtNoYCmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try {
                currentBua.asmtNoY = Int32.Parse(asmtNoYCmb.SelectedValue.ToString());
            } catch (NullReferenceException) {
                Console.Error.WriteLine("WARN: Event fired before object initialisation");
            }
        }
    }
}
