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
using System.Windows.Shapes;
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
        private const int IMP_RECORDS_LIMIT = 100; //Only show up to 100 records at a time
        private const int IMP_RECORD_ROW_HEIGHT = 35; //Components are 25px with 10px bottom padding = 35px

        WeightDatePage callingPage; //Storing this enables this window to call for the WeightDate page to update its sdd value according to the newly change bua object
        private BUAssessment currentBua;
        CachedCsvReader csvReader;

        public SelectSubmissionDueDateDialog(WeightDatePage caller, BUAssessment bua)
        {
            InitializeComponent();
            callingPage = caller;
            currentBua = bua;

            //If a unit title has been set, use that in the initial search, otherwise accept any unit title ("")
            if(currentBua.unitTitle.Length > 3)
            {
                string firstCharsOfUnitTitle = currentBua.unitTitle.Substring(0, 3);
                unitTitleSearchTxt.Text = firstCharsOfUnitTitle;
                createDynamicControls(getCsvDataArray("D:\\University\\Documents\\IMP.csv", firstCharsOfUnitTitle));
            } else {
                createDynamicControls(getCsvDataArray("D:\\University\\Documents\\IMP.csv", ""));
            }
        }

        void createDynamicControls(List<string[]> csvData)
        {
            try {
                //Check to see if there is data to draw
                if (csvData.Count > 0) {

                    //Ensure canvas is reset before drawing
                    csvDataCanvas.Children.Clear();
                    csvDataCanvas.Height = 100;
                    int canvastop = 0; //keeps track of where to draw the next row of csvData controls
                    string currentDueDate;

                    //Draw components
                    for (int r = 0; r < csvData.Count; r++) {
                        csvDataCanvas.Height += IMP_RECORD_ROW_HEIGHT;

                        var tb = new TextBlock();
                        tb.Text = csvData[r][0];
                        tb.Height = 25;
                        tb.Width = 100;
                        tb.Padding = new Thickness(0, 0, 0, 10);
                        csvDataCanvas.Children.Add(tb);
                        Canvas.SetLeft(tb, 0);
                        Canvas.SetTop(tb, canvastop);

                        var tb2 = new TextBlock();
                        tb2.Text = csvData[r][1];
                        tb2.Height = 25;
                        tb2.Width = 100;
                        csvDataCanvas.Children.Add(tb2);
                        Canvas.SetLeft(tb2, 50);
                        Canvas.SetTop(tb2, canvastop);

                        var tb3 = new TextBlock();
                        tb3.Text = csvData[r][2];
                        tb3.Height = 25;
                        tb3.Width = 100;
                        csvDataCanvas.Children.Add(tb3);
                        Canvas.SetLeft(tb3, 100);
                        Canvas.SetTop(tb3, canvastop);

                        currentDueDate = csvData[r][3];

                        var tb4 = new TextBlock();
                        tb4.Text = currentDueDate;
                        tb4.Height = 25;
                        tb4.Width = 100;
                        csvDataCanvas.Children.Add(tb4);
                        Canvas.SetLeft(tb4, 250);
                        Canvas.SetTop(tb4, canvastop);

                        Button b1 = new Button();
                        b1.Content = "Select";
                        b1.Height = 25;
                        b1.Width = 50;
                        b1.Name = "coolbtn" + r + "4";
                        b1.Tag = currentDueDate;
                        b1.AddHandler(Button.ClickEvent, new RoutedEventHandler(b1_Click));
                        csvDataCanvas.Children.Add(b1);
                        Canvas.SetLeft(b1, 300);
                        Canvas.SetTop(b1, canvastop);

                        canvastop += IMP_RECORD_ROW_HEIGHT;
                    }
                }
            } catch(NullReferenceException) {
                Console.Error.WriteLine("ERROR: Recieved no data to draw");
            }
        }

        private void unitTitleSearchBtn_Click(object sender, RoutedEventArgs e)
        {
            createDynamicControls(getCsvDataArray("D:\\University\\Documents\\IMP.csv", unitTitleSearchTxt.Text));
        }

        //TODO: send the tag value as the selected date
        private void b1_Click(object sender, RoutedEventArgs e)
        {
            Button sourcebtn = (Button)e.Source;
            Console.WriteLine("test" + sourcebtn.Tag);
        }

        List<string[]> getCsvDataArray(string csvPath, string searchTerm)
        {
            try
            {
                using (csvReader = new CachedCsvReader(new StreamReader(csvPath), true))
                {
                    List<string[]> filteredRecords = new List<string[]>();
                    csvReader.ReadToEnd();

                    //i = 1 because the first record is not a real record, it's the headers
                    for (int i = 1; i < csvReader.Records.Count; i++) {

                        //Only add the record if it matches the filters
                        if (submissionDateChe.IsChecked == true) {

                            //Search term + Has submission due date
                            if(filteredRecords.Count < IMP_RECORDS_LIMIT) {
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
                                    string[] trimmedRecord = new string[4] { csvReader.Records[i][7], csvReader.Records[i][6], csvReader.Records[i][4], csvReader.Records[i][25] };
                                    filteredRecords.Add(trimmedRecord);
                                }
                            }

                        }
                    }
                    return filteredRecords;
                }
            } catch(Exception)
            {
                //File cannot be accessed (e.g. used by another process)
                Console.WriteLine("ERROR: Unable to write to " + csvPath + " - used by another process?");
                MessageBox.Show("Unable to open " + csvPath + ". It might be locked by another process.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }
    }
}
