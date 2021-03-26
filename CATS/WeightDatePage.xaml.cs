using Microsoft.Win32;
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
        private const string IMP_FILE_FILTER =
            "Comma Separated Values File (*.csv)|*.csv";

        private BUAssessment currentBua;

        public WeightDatePage(BUAssessment bua)
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

            impTxt.Text = currentBua.impPath;
            if(currentBua.hasValidSubmissionDueDate() && currentBua.impPath.Length < 1) {
                updateSddValue(false);
            } else {
                updateSddValue(true);
            }           
        }

        /// <summary>
        /// Sets the submission due date value to update and display if it's valid, otherwise displays a placeholder
        /// </summary>
        public void updateSddValue(bool isSetAutomatically)
        {
            if(currentBua.hasValidSubmissionDueDate() && !isSetAutomatically) {
                //Manual date
                sddValTb.Text = currentBua.submissionDueDate.ToString("dd/MM/yyyy (h:mm tt)");
                currentBua.impPath = String.Empty;
                impTxt.Text = currentBua.impPath;
                sddWarnTb.Visibility = Visibility.Visible;
            } else if(currentBua.hasValidSubmissionDueDate()) {
                //Auto date
                sddValTb.Text = currentBua.submissionDueDate.ToString("dd/MM/yyyy (h:mm tt)");
                sddWarnTb.Visibility = Visibility.Hidden;
            } else {
                //No valid date
                sddValTb.Text = "Not set";
                sddWarnTb.Visibility = Visibility.Hidden;
            }
        }

        /// <summary>
        /// Use an open file dialog to select an IMP csv file,
        /// then display the select submission due date dialog, which provides the facilities to select a pre-approved submission due date
        /// </summary>
        private void selectImpFile()
        {
            OpenFileDialog ofd = new OpenFileDialog() {
                Filter = IMP_FILE_FILTER,
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };
            if (ofd.ShowDialog() == true) {
                currentBua.impPath = ofd.FileName;
                impTxt.Text = currentBua.impPath;

                SelectSubmissionDueDateDialog selectsubmissionduedatedialog = new SelectSubmissionDueDateDialog(this, currentBua);
                selectsubmissionduedatedialog.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// Display the change date dialog, which provides the facilities to overwrite the submission due date
        /// </summary>
        private void changeSdd()
        {
            ChangeDateDialog changedatedialog = new ChangeDateDialog(this, currentBua);
            changedatedialog.Visibility = Visibility.Visible;
        }

        #region Event handlers
        /// <summary>
        /// When the Browse button is clicked
        /// </summary>
        private void impBtn_Click(object sender, RoutedEventArgs e)
        {
            selectImpFile();
        }

        /// <summary>
        /// When the Change Date button is clicked
        /// </summary>
        private void sddBtn_Click(object sender, RoutedEventArgs e)
        {
            changeSdd();
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
        /// Update the bua object to match the weighting textbox content
        /// When the contents of the weighting textbox changes
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
        /// Update the bua object to match the credit value combobox content
        /// When the dropdownitem for the credit value field changes
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
#endregion
    }
}
