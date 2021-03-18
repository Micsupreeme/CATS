﻿using System;
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
                string firstCharsOfUnitTitle = currentBua.unitTitle.Substring(0, 3);
                unitTitleSearchTxt.Text = firstCharsOfUnitTitle;
                createDynamicControls(getCsvDataArray(currentBua.impPath, firstCharsOfUnitTitle));
            } else {
                createDynamicControls(getCsvDataArray(currentBua.impPath, ""));
            }
        }

        void createDynamicControls(List<string[]> csvData)
        {
            try {
                //Check to see if there is data to draw
                if (csvData.Count > 0) {

                    //Ensure canvas is reset before drawing
                    csvDataCanvas.Children.Clear();
                    csvDataCanvas.Height = 25;
                    int canvastop = 0; //keeps track of where to draw the next row of csvData controls
                    string currentDueDate;
                    if(csvData.Count == 100) {
                        recordCountTb.Text = "Large dataset - displaying only the first 100 records";
                    } else {
                        recordCountTb.Text = "Displaying " + csvData.Count + " records";
                    }                  

                    //Draw components
                    //Headers
                    var levelHeaderTb = new TextBlock();
                    levelHeaderTb.FontFamily = new FontFamily(IMP_RECORD_FONT_FAMILY);
                    levelHeaderTb.FontSize = IMP_RECORD_FONT_SIZE;
                    levelHeaderTb.Text = "Level";
                    levelHeaderTb.Height = 25;
                    levelHeaderTb.Width = 100;
                    levelHeaderTb.Padding = new Thickness(5, 5, 5, 5);
                    levelHeaderTb.Background = new SolidColorBrush(Color.FromRgb(244, 244, 244));
                    levelHeaderTb.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
                    csvDataCanvas.Children.Add(levelHeaderTb);
                    Canvas.SetLeft(levelHeaderTb, 0);
                    Canvas.SetTop(levelHeaderTb, canvastop);

                    var semesterHeaderTb = new TextBlock();
                    semesterHeaderTb.Text = "Semester";
                    semesterHeaderTb.FontFamily = new FontFamily(IMP_RECORD_FONT_FAMILY);
                    semesterHeaderTb.FontSize = IMP_RECORD_FONT_SIZE;
                    semesterHeaderTb.Height = 25;
                    semesterHeaderTb.Width = 120;
                    semesterHeaderTb.Padding = new Thickness(5, 5, 5, 5);
                    semesterHeaderTb.Background = new SolidColorBrush(Color.FromRgb(244, 244, 244));
                    semesterHeaderTb.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
                    csvDataCanvas.Children.Add(semesterHeaderTb);
                    Canvas.SetLeft(semesterHeaderTb, 50);
                    Canvas.SetTop(semesterHeaderTb, canvastop);

                    var unitTitleHeaderTb = new TextBlock();
                    unitTitleHeaderTb.FontFamily = new FontFamily(IMP_RECORD_FONT_FAMILY);
                    unitTitleHeaderTb.FontSize = IMP_RECORD_FONT_SIZE;
                    unitTitleHeaderTb.Text = "Unit Title";
                    unitTitleHeaderTb.Height = 25;
                    unitTitleHeaderTb.Width = 300;
                    unitTitleHeaderTb.Padding = new Thickness(7, 5, 5, 5);
                    unitTitleHeaderTb.Background = new SolidColorBrush(Color.FromRgb(244, 244, 244));
                    unitTitleHeaderTb.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
                    csvDataCanvas.Children.Add(unitTitleHeaderTb);
                    Canvas.SetLeft(unitTitleHeaderTb, 120);
                    Canvas.SetTop(unitTitleHeaderTb, canvastop);

                    var submissionDateHeaderTb = new TextBlock();
                    submissionDateHeaderTb.FontFamily = new FontFamily(IMP_RECORD_FONT_FAMILY);
                    submissionDateHeaderTb.FontSize = IMP_RECORD_FONT_SIZE;
                    submissionDateHeaderTb.Text = "Submission Date";
                    submissionDateHeaderTb.Height = 25;
                    submissionDateHeaderTb.Width = 190;
                    submissionDateHeaderTb.Padding = new Thickness(5, 5, 5, 5);
                    submissionDateHeaderTb.Background = new SolidColorBrush(Color.FromRgb(244, 244, 244));
                    submissionDateHeaderTb.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
                    csvDataCanvas.Children.Add(submissionDateHeaderTb);
                    Canvas.SetLeft(submissionDateHeaderTb, 420);
                    Canvas.SetTop(submissionDateHeaderTb, canvastop);

                    canvastop += IMP_RECORD_ROW_HEIGHT;

                    //Records
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
                        if(currentDueDate.Length > 0)
                        {
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
                        if(r % 2 == 0) {
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
                }
            } catch(NullReferenceException) {
                Console.Error.WriteLine("ERROR: Recieved no data to draw");
            }
        }

        private void unitTitleSearchBtn_Click(object sender, RoutedEventArgs e)
        {
            createDynamicControls(getCsvDataArray(currentBua.impPath, unitTitleSearchTxt.Text));
        }

        private void selectSubmissionDateBtn_Click(object sender, RoutedEventArgs e)
        {
            Button sourceBtn = (Button)e.Source;
            currentBua.submissionDueDate = DateTime.Parse(sourceBtn.Tag.ToString() + " " + DEFAULT_TIME_SUFFIX);
            callingPage.updateSddValue(true); //Indicate that this is a preset submission due date

            this.Visibility = Visibility.Collapsed;
        }
        private void cancelBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
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
            } catch(IOException) {
                //File cannot be accessed (e.g. used by another process)
                Console.WriteLine("ERROR: Unable to write to " + csvPath + " - used by another process?");
                MessageBox.Show("Unable to open " + csvPath + ". It might be locked by another process.", "Open Error", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Visibility = Visibility.Collapsed;
                return null;
            } catch(IndexOutOfRangeException) {
                //Unrecognised CSV format (i.e, not a valid IMP)
                Console.WriteLine("ERROR: " + csvPath + " is not a recognised standard BU IMP file");
                MessageBox.Show("Unable to open " + csvPath + ". The file contains an unrecognised data format.", "Format Error", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Visibility = Visibility.Collapsed;
                return null;
            }
        }
    }
}