using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.IO;
using LumenWorks.Framework.IO.Csv;

namespace CATS
{
    /// <summary>
    /// Interaction logic for SelectSubmissionDueDateDialog.xaml
    /// </summary>
    public partial class SelectSubmissionDueDateDialog : Window
    {
        //If the structure of the IMP changes, just change these values
        private const int IMP_SEMESTER_COL_INDEX = 7;
        private const int IMP_LEVEL_COL_INDEX = 6;
        private const int IMP_UNIT_TITLE_COL_INDEX = 4;
        private const int IMP_SUBMISSION_DATE_COL_INDEX = 25;
        private const int IMP_RECORDS_LIMIT = 100;              //Only show up to 100 records at a time
        private const int IMP_RECORD_ROW_HEIGHT = 35;           //Components are 25px with 10px bottom padding = 35px

        private const string IMP_RECORD_FONT_FAMILY = "Arial";
        private const int IMP_RECORD_FONT_SIZE = 12;

        private const string DEFAULT_TIME_SUFFIX = "12:30:00";

        WeightDatePage callingPage; //Storing this enables this window to call for the WeightDate page to update its sdd value according to the newly change bua object
        private BUAssessment currentBua;
        private CachedCsvReader csvReader;

        public SelectSubmissionDueDateDialog(WeightDatePage caller, BUAssessment bua)
        {
            InitializeComponent();
            callingPage = caller;
            currentBua = bua;

            //If a unit title has been set, use that in the initial search, otherwise accept any unit title ("")
            if(currentBua.unitTitle.Length > 3)
            {
                string firstCharsOfUnitTitle = currentBua.unitTitle.Substring(0, 3).ToUpper();
                unitTitleSearchTxt.Text = firstCharsOfUnitTitle;
                createDynamicControls(getCsvDataArray(currentBua.impPath, firstCharsOfUnitTitle));
            } else {
                createDynamicControls(getCsvDataArray(currentBua.impPath, ""));
            }
        }

        /// <summary>
        /// Gets a filtered IMP csv dataset for the specified path, using the specified search term to search by unit title
        /// </summary>
        /// <param name="csvPath">The full path to the IMP csv file</param>
        /// <param name="searchTerm">The unit title search term to filter records by</param>
        /// <returns></returns>
        List<string[]> getCsvDataArray(string csvPath, string searchTerm)
        {
            try {
                using (csvReader = new CachedCsvReader(new StreamReader(csvPath), true)) {
                    List<string[]> filteredRecords = new List<string[]>();
                    csvReader.ReadToEnd();

                    //i = 1 because the first record is not a real record, it's the headers
                    for (int i = 1; i < csvReader.Records.Count; i++) {

                        //Only add the record if it matches the filters
                        if (submissionDateChe.IsChecked == true) {
                            //Search term + Has submission due date
                            if (filteredRecords.Count < IMP_RECORDS_LIMIT) {
                                //Only add records if the record limit is not exceeded
                                if (csvReader.Records[i][4].Contains(searchTerm) && csvReader.Records[i][IMP_SUBMISSION_DATE_COL_INDEX].Length > 0) {
                                    string[] trimmedRecord = new string[4] { csvReader.Records[i][IMP_SEMESTER_COL_INDEX], csvReader.Records[i][IMP_LEVEL_COL_INDEX], csvReader.Records[i][IMP_UNIT_TITLE_COL_INDEX], csvReader.Records[i][IMP_SUBMISSION_DATE_COL_INDEX] };
                                    filteredRecords.Add(trimmedRecord);
                                }
                            }
                        } else {
                            //Search term only
                            if (filteredRecords.Count < IMP_RECORDS_LIMIT) {
                                //Only add records if the record limit is not exceeded
                                if (csvReader.Records[i][4].Contains(searchTerm)) {
                                    string[] trimmedRecord = new string[4] { csvReader.Records[i][IMP_SEMESTER_COL_INDEX], csvReader.Records[i][IMP_LEVEL_COL_INDEX], csvReader.Records[i][IMP_UNIT_TITLE_COL_INDEX], csvReader.Records[i][IMP_SUBMISSION_DATE_COL_INDEX] };
                                    filteredRecords.Add(trimmedRecord);
                                }
                            }
                        }
                    }
                    return filteredRecords;
                }
            } catch (IOException) {
                //File cannot be accessed (e.g. used by another process)
                Console.WriteLine("ERROR: Unable to write to " + csvPath + " - used by another process?");
                MessageBox.Show("Unable to open " + csvPath + ". It might be locked by another process.", "Open Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            } catch (IndexOutOfRangeException) {
                //Unrecognised CSV format (i.e, not a valid IMP)
                Console.WriteLine("ERROR: " + csvPath + " is not a recognised standard BU IMP file");
                MessageBox.Show("Unable to open " + csvPath + ". The file contains an unrecognised data format.", "Format Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        /// <summary>
        /// Draw the specified csv dataset to the canvas
        /// </summary>
        /// <param name="csvData">A pre-filtered list of csv data records </param>
        private void createDynamicControls(List<string[]> csvData)
        {
            if(csvData != null) {
                this.Visibility = Visibility.Visible;
                try {
                    //Ensure canvas is reset before drawing
                    csvDataCanvas.Children.Clear();
                    csvDataCanvas.Height = 25;
                    int canvastop = 0; //keeps track of where to draw the next row of csvData controls ()
                    string currentDueDate;

                    //Check to see if there is data to draw
                    if (csvData.Count > 0) {
                        if (csvData.Count == 100) {
                            recordCountTb.Text = "Large dataset - displaying only the first 100 records";
                        } else {
                            recordCountTb.Text = "Displaying " + csvData.Count + " records";
                        }

                        //Draw records
                        for (int r = 0; r < csvData.Count; r++) {
                            csvDataCanvas.Height += IMP_RECORD_ROW_HEIGHT;

                            var levelTb = new TextBlock();
                            levelTb.FontFamily = new FontFamily(IMP_RECORD_FONT_FAMILY);
                            levelTb.FontSize = IMP_RECORD_FONT_SIZE;
                            levelTb.Text = csvData[r][0];
                            levelTb.Height = 25;
                            levelTb.Width = 100;
                            levelTb.Padding = new Thickness(5, 5, 5, 5);
                            csvDataCanvas.Children.Add(levelTb);
                            Canvas.SetLeft(levelTb, 0);
                            Canvas.SetTop(levelTb, canvastop);

                            var semesterTb = new TextBlock();
                            semesterTb.FontFamily = new FontFamily(IMP_RECORD_FONT_FAMILY);
                            semesterTb.FontSize = IMP_RECORD_FONT_SIZE;
                            semesterTb.Text = csvData[r][1];
                            semesterTb.Height = 25;
                            semesterTb.Width = 120;
                            semesterTb.Padding = new Thickness(5, 5, 5, 5);
                            csvDataCanvas.Children.Add(semesterTb);
                            Canvas.SetLeft(semesterTb, 50);
                            Canvas.SetTop(semesterTb, canvastop);

                            var unitTitleTxt = new TextBox();
                            unitTitleTxt.FontFamily = new FontFamily(IMP_RECORD_FONT_FAMILY);
                            unitTitleTxt.FontSize = IMP_RECORD_FONT_SIZE;
                            unitTitleTxt.Text = csvData[r][2];
                            unitTitleTxt.Height = 25;
                            unitTitleTxt.Width = 300;
                            unitTitleTxt.Padding = new Thickness(5, 5, 5, 5);
                            unitTitleTxt.IsReadOnly = true;
                            unitTitleTxt.BorderThickness = new Thickness(0, 0, 0, 0);
                            unitTitleTxt.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                            csvDataCanvas.Children.Add(unitTitleTxt);
                            Canvas.SetLeft(unitTitleTxt, 120);
                            Canvas.SetTop(unitTitleTxt, canvastop);

                            currentDueDate = csvData[r][3];

                            var submissionDateTb = new TextBlock();
                            submissionDateTb.FontFamily = new FontFamily(IMP_RECORD_FONT_FAMILY);
                            submissionDateTb.FontSize = IMP_RECORD_FONT_SIZE;
                            submissionDateTb.Text = currentDueDate;
                            submissionDateTb.Height = 25;
                            submissionDateTb.Width = 120;
                            submissionDateTb.Padding = new Thickness(5, 5, 5, 5);
                            csvDataCanvas.Children.Add(submissionDateTb);
                            Canvas.SetLeft(submissionDateTb, 420);
                            Canvas.SetTop(submissionDateTb, canvastop);

                            //Only show the "Select date" button if there is a date that can be selected
                            if (currentDueDate.Length > 0) {
                                Button selectSubmissionDateBtn = new Button();
                                selectSubmissionDateBtn.FontFamily = new FontFamily(IMP_RECORD_FONT_FAMILY);
                                selectSubmissionDateBtn.FontSize = IMP_RECORD_FONT_SIZE;
                                selectSubmissionDateBtn.Content = "Select";
                                selectSubmissionDateBtn.Height = 25;
                                selectSubmissionDateBtn.Width = 70;
                                selectSubmissionDateBtn.Name = "selectSubmissionDateBtn_" + r;
                                selectSubmissionDateBtn.Tag = currentDueDate;
                                selectSubmissionDateBtn.AddHandler(Button.ClickEvent, new RoutedEventHandler(selectSubmissionDateBtn_Click));
                                csvDataCanvas.Children.Add(selectSubmissionDateBtn);
                                Canvas.SetLeft(selectSubmissionDateBtn, 540);
                                Canvas.SetTop(selectSubmissionDateBtn, canvastop);
                            }

                            //Subtly alternating row colours for visual clarity
                            if (r % 2 == 0) {
                                levelTb.Background = new SolidColorBrush(Color.FromRgb(55, 55, 55));
                                semesterTb.Background = new SolidColorBrush(Color.FromRgb(55, 55, 55));
                                unitTitleTxt.Background = new SolidColorBrush(Color.FromRgb(55, 55, 55));
                                submissionDateTb.Background = new SolidColorBrush(Color.FromRgb(55, 55, 55));
                            } else {
                                levelTb.Background = new SolidColorBrush(Color.FromRgb(85, 85, 85));
                                semesterTb.Background = new SolidColorBrush(Color.FromRgb(85, 85, 85));
                                unitTitleTxt.Background = new SolidColorBrush(Color.FromRgb(85, 85, 85));
                                submissionDateTb.Background = new SolidColorBrush(Color.FromRgb(85, 85, 85));
                            }
                            canvastop += IMP_RECORD_ROW_HEIGHT;
                        }
                    } else {
                        recordCountTb.Text = "Search returned no records";
                    }
                } catch (NullReferenceException) {
                    Console.Error.WriteLine("ERROR: Recieved no data to draw");
                }
            } else {
                this.Visibility = Visibility.Collapsed; //Don't display a window with 0 records

                currentBua.impPath = String.Empty; //Don't allow an unreadable CSV file to be stored as the IMP source
                this.callingPage.impTxt.Text = currentBua.impPath;
            }
        }

        #region Event handlers
        /// <summary>
        /// When the search button is clicked
        /// </summary>
        private void unitTitleSearchBtn_Click(object sender, RoutedEventArgs e)
        {
            createDynamicControls(getCsvDataArray(currentBua.impPath, unitTitleSearchTxt.Text));
        }

        /// <summary>
        /// When the "has date" checkbox is either checked or unchecked
        /// </summary>
        private void submissionDateChe_CheckChanged(object sender, RoutedEventArgs e)
        {
            try {
                createDynamicControls(getCsvDataArray(currentBua.impPath, unitTitleSearchTxt.Text));
            } catch (NullReferenceException) {
                Console.Error.WriteLine("WARN: Event fired before object initialisation ");
            }
        }

        /// <summary>
        /// When a dynamic "select" date buttons is clicked
        /// </summary>
        private void selectSubmissionDateBtn_Click(object sender, RoutedEventArgs e)
        {
            Button sourceBtn = (Button)e.Source;
            currentBua.submissionDueDate = DateTime.Parse(sourceBtn.Tag.ToString() + " " + DEFAULT_TIME_SUFFIX);
            callingPage.updateSddValue(true); //Indicate that this is a preset submission due date

            this.Visibility = Visibility.Collapsed;
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
