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
    /// Interaction logic for WeightDatePage.xaml
    /// </summary>
    public partial class WeightDatePage : Page
    {
        private BUAssessment currentBua;

        public WeightDatePage(BUAssessment bua)
        {
            InitializeComponent();
            currentBua = bua;
            populateFields();
        }

        private void impBtn_Click(object sender, RoutedEventArgs e)
        {
            selectImpFile();
        }

        private void sddBtn_Click(object sender, RoutedEventArgs e)
        {
            changeSdd();
        }

        /// <summary>
        /// Upon page load, populate the fields with the values stored in the bua object
        /// </summary>
        private void populateFields()
        {
            if(!currentBua.isGroup) {
                asmtTypeCmb.SelectedIndex = 0;
            } else {
                asmtTypeCmb.SelectedIndex = 1;
            }

            weightingTxt.Text = currentBua.weighting.ToString();

            switch(currentBua.creditValue)
            {
                case 20:
                    creditValueCmb.SelectedIndex = 0;
                    break;
                case 40:
                    creditValueCmb.SelectedIndex = 1;
                    break;
                case 60:
                    creditValueCmb.SelectedIndex = 2;
                    break;
                default:
                    creditValueCmb.SelectedIndex = 0;
                    Console.WriteLine("ERROR: Invalid credit value specified");
                    break;
            }

            updateSddValue(true);

            //TODO: if(currentBua.hasValidSubmissionDueDate() && <no path to imp>) { show "manually set" warning }
        }

        /// <summary>
        /// Sets the submission due date value to update and display if it's valid, otherwise displays a placeholder
        /// </summary>
        public void updateSddValue(bool isSetAutomatically)
        {
            if(currentBua.hasValidSubmissionDueDate()) {
                sddValTb.Text = currentBua.submissionDueDate.ToString("dd/MM/yyyy (h:mm tt)");
            } else {
                sddValTb.Text = "Not set";
            }

            if(!isSetAutomatically) {
                sddWarnTb.Visibility = Visibility.Visible;
            }
        }

        private void selectImpFile()
        {
            
        }

        private void changeSdd()
        {
            ChangeDateDialog changedatedialog = new ChangeDateDialog(this, currentBua);
            changedatedialog.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// On selection changed, update the bua object to match the assessment type combobox content
        /// </summary>
        private void asmtTypeCmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try {
                if (asmtTypeCmb.SelectedValue.Equals("Individual")) {
                    currentBua.isGroup = false;
                } else {
                    currentBua.isGroup = true;
                }
            } catch (NullReferenceException) {
                Console.Error.WriteLine("WARN: Event fired before object initialisation");
            }
        }

        /// <summary>
        /// On text changed, update the bua object to match the weighting textbox content
        /// </summary>
        private void weightingTxt_TextChanged(object sender, TextChangedEventArgs e)
        {
            try {
                //Keep this code absolutely minimal, as this event fires every time a character changes in the textbox
                currentBua.weighting = int.Parse(weightingTxt.Text);
            } catch (FormatException) {
                Console.Error.WriteLine("ERROR: Attempted to convert alphabet to int");
            } catch (NullReferenceException) {
                Console.Error.WriteLine("WARN: Event fired before object initialisation");
            }
        }

        /// <summary>
        /// On selection changed, update the bua object to match the credit value combobox content
        /// </summary>
        private void creditValueCmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try {
                currentBua.creditValue = int.Parse(creditValueCmb.SelectedValue.ToString());
            } catch (FormatException) {
                Console.Error.WriteLine("ERROR: Attempted to convert alphabet to int");
            } catch (NullReferenceException) {
                Console.Error.WriteLine("WARN: Event fired before object initialisation");
            }
        }
    }
}
