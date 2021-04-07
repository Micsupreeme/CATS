using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CATS
{
    /// <summary>
    /// Interaction logic for ILOsPage.xaml
    /// </summary>
    public partial class ILOsPage : Page
    {
        private const int MIN_NUMBER_ILOS = 4;
        private const int MAX_NUMBER_ILOS = 10;
        
        private BUAssessment currentBua;
        StackPanel iloFullStack;
        List<StackPanel> iloRowStacks;

        int numberOfILOs; //get count of list items from bua object

        public ILOsPage(BUAssessment bua)
        {
            
            InitializeComponent();
            currentBua = bua;

            if(currentBua.ILOsList.Count <= MIN_NUMBER_ILOS) {
                numberOfILOs = MIN_NUMBER_ILOS;                 //always show at least 4 ILO controls
            } else {
                numberOfILOs = currentBua.ILOsList.Count;       //If there are more than 4 ILOs, display that number of ILO controls
            }
            createDynamicControls();
        }

        /// <summary>
        /// Create initial ILO controls to represent the current number of ILOs in the BUA object
        /// </summary>
        private void createDynamicControls()
        {
            //Ensure canvas is reset before drawing
            ilosCanvas.Children.Clear();
            ilosCanvas.Height = 0;
            int canvastop = 0; //keeps track of where to draw the next row of ILO controls ()

            iloRowStacks = new List<StackPanel>();

            //Draw records
            for (int ilo = 0; ilo < numberOfILOs; ilo++)
            {
                ilosCanvas.Height += 70;

                var iloNumberTb = new TextBlock();
                //iloNumberTb.Name = "nTb_" + ilo;
                iloNumberTb.FontFamily = new FontFamily("Arial");
                iloNumberTb.FontSize = 14;
                iloNumberTb.Text = (ilo + 1) + "."; //human numbering starts at 1 (i.e., 1-10 ILOs - not 0-9)
                iloNumberTb.Height = 35;
                iloNumberTb.Width = 50;
                iloNumberTb.Padding = new Thickness(5, 5, 5, 5);

                var iloContentTxt = new TextBox();
                //iloContentTxt.Name = "icTxt_" + ilo;
                iloContentTxt.FontFamily = new FontFamily("Arial");
                iloContentTxt.FontSize = 14;

                try {
                    iloContentTxt.Text = currentBua.ILOsList[ilo];
                } catch(ArgumentOutOfRangeException) {
                    Console.WriteLine("WARN: Drawing more ILO controls than there are ILOs");
                }

                iloContentTxt.Height = 70;
                iloContentTxt.Width = 540;
                iloContentTxt.Padding = new Thickness(5, 5, 5, 5);
                iloContentTxt.AcceptsReturn = true; //multiline textbox
                iloContentTxt.TextWrapping = TextWrapping.Wrap;
                iloContentTxt.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                iloContentTxt.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
                iloContentTxt.AddHandler(TextBox.TextChangedEvent, new RoutedEventHandler(iloContentTxt_TextChanged));

                //Only show the "delete" button if it's not the first ILO - there must always be one
                if (ilo > 0) {
                    var iloDeleteBtn = new Button();
                    //iloDeleteBtn.Name = "idBtn_" + ilo;
                    iloDeleteBtn.FontFamily = new FontFamily("Arial");
                    iloDeleteBtn.FontSize = 14;
                    iloDeleteBtn.Height = 70;
                    iloDeleteBtn.Width = 50;
                    iloDeleteBtn.Padding = new Thickness(5, 5, 5, 5);
                    iloDeleteBtn.Tag = ilo;
                    iloDeleteBtn.AddHandler(Button.ClickEvent, new RoutedEventHandler(iloDeleteBtn_Click));
                    iloDeleteBtn.Content = new Image
                    {
                        Source = new BitmapImage(new Uri("pack://application:,,,/img/icons/baseline_delete_black_18dp.png")),
                        Height = 30,
                        Width = 30,
                    };

                    StackPanel singlestack = new StackPanel();
                    singlestack.Orientation = Orientation.Horizontal;
                    singlestack.Children.Add(iloNumberTb);
                    singlestack.Children.Add(iloContentTxt);
                    singlestack.Children.Add(iloDeleteBtn);
                    iloRowStacks.Add(singlestack);
                    iloRowStacks[ilo].Name = "rowStk_" + ilo;
                } else {
                    StackPanel singlestack = new StackPanel();
                    singlestack.Orientation = Orientation.Horizontal;
                    singlestack.Children.Add(iloNumberTb);
                    singlestack.Children.Add(iloContentTxt);
                    iloRowStacks.Add(singlestack);
                    iloRowStacks[ilo].Name = "rowStk_" + ilo;
                }

                canvastop += 80;
            }
            iloFullStack = new StackPanel();
            iloFullStack.Orientation = Orientation.Vertical;
            foreach(StackPanel row in iloRowStacks)
            {
                iloFullStack.Children.Add(row);
            }

            ilosCanvas.Children.Add(iloFullStack);
            Canvas.SetLeft(iloFullStack, 0);
            Canvas.SetTop(iloFullStack, 0);
        }

        /// <summary>
        /// Update the BUA object ILO list with the contents of the dynamic ILO textboxes
        /// </summary>
        private void saveILOChanges()
        {
            try
            {
                List<string> tempILOsList = new List<string>();

                //Keep this code absolutely minimal, as this event fires every time a character changes in one of the dynamic ILO content boxes
                for (int row = 0; row < iloFullStack.Children.Count; row++)
                {
                    StackPanel rowStack = (StackPanel)iloFullStack.Children[row];
                    TextBox contentTxt = (TextBox)rowStack.Children[1]; //the second ("1") child of a single stack is always the content textbox
                    if (contentTxt.Text.Length > 0)
                    {
                        tempILOsList.Add(contentTxt.Text); //only save ILOs that aren't string empty
                    }
                }

                currentBua.ILOsList = tempILOsList;
            }
            catch (NullReferenceException)
            {
                Console.Error.WriteLine("WARN: Event fired before object initialisation ");
            }
        }

        /// <summary>
        /// Enable/disable the "add ILO" button depending on the current number of ILOs compared to the maximum
        /// </summary>
        private void updateAddButtonState()
        {
            if (iloFullStack.Children.Count == MAX_NUMBER_ILOS)
            {
                iloAddBtn.IsEnabled = false; //if max number of ILOs reached, disable add button
            }
            else
            {
                iloAddBtn.IsEnabled = true;
            }
        }

        /// <summary>
        /// Update the index numbers for the rows of ILO controls.
        /// (e.g., if you have "1, 2, 3" and you delete "2", you end up with "1, 2" instead of "1, 3" (missing number))
        /// </summary>
        private void updateILONumbering()
        {
            for (int row = 0; row < iloFullStack.Children.Count; row++)
            {
                StackPanel rowStack = (StackPanel)iloFullStack.Children[row];
                TextBlock numberTb = (TextBlock)rowStack.Children[0]; //the first child of a single stack is always the number text block
                numberTb.Text = (row + 1) + ".";
            }
        }

        #region Event handlers
        /// <summary>
        /// When the content of a dynamic ILO textbox is changed
        /// </summary>
        private void iloContentTxt_TextChanged(object sender, RoutedEventArgs e)
        {
            saveILOChanges();
        }

        /// <summary>
        /// When a dynamic "delete" ILO button is clicked
        /// </summary>
        private void iloDeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            ilosCanvas.Height -= 70;

            Button eventBtn = (Button)e.Source;
            int eventTag = (int)eventBtn.Tag; //the tag that indicates the row (containing all the controls to be deleted)
       
            for (int row = 0; row < iloFullStack.Children.Count; row++) {
                StackPanel rowStack = (StackPanel)iloFullStack.Children[row];
                if (rowStack.Name.Equals("rowStk_" + eventTag)) {
                    iloFullStack.Children.RemoveAt(row);
                }
            }

            updateILONumbering();
            saveILOChanges();
            updateAddButtonState();
        }

        /// <summary>
        /// When the "Add another ILO" button is clicked
        /// </summary>
        private void iloAddBtn_Click(object sender, RoutedEventArgs e)
        {
            ilosCanvas.Height += 70;
            
            var iloNewNumberTb = new TextBlock();
            //iloNewNumberTb.Name = "nTb_" + ilo;
            iloNewNumberTb.FontFamily = new FontFamily("Arial");
            iloNewNumberTb.FontSize = 14;
            iloNewNumberTb.Text = iloFullStack.Children.Count + 1 + ".";
            iloNewNumberTb.Height = 35;
            iloNewNumberTb.Width = 50;
            iloNewNumberTb.Padding = new Thickness(5, 5, 15, 5);

            var iloNewContentTxt = new TextBox();
            //iloNewContentTxt.Name = "icTxt_" + ilo;
            iloNewContentTxt.FontFamily = new FontFamily("Arial");
            iloNewContentTxt.FontSize = 14;
            iloNewContentTxt.Height = 70;
            iloNewContentTxt.Width = 540;
            iloNewContentTxt.Padding = new Thickness(5, 5, 5, 5);
            iloNewContentTxt.AcceptsReturn = true; //multiline textbox
            iloNewContentTxt.TextWrapping = TextWrapping.Wrap;
            iloNewContentTxt.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            iloNewContentTxt.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
            iloNewContentTxt.AddHandler(TextBox.TextChangedEvent, new RoutedEventHandler(iloContentTxt_TextChanged));

            var iloNewDeleteBtn = new Button();
            //iloNewDeleteBtn.Name = "idBtn_" + ilo;
            iloNewDeleteBtn.FontFamily = new FontFamily("Arial");
            iloNewDeleteBtn.FontSize = 14;
            iloNewDeleteBtn.Height = 70;
            iloNewDeleteBtn.Width = 50;
            iloNewDeleteBtn.Padding = new Thickness(5, 5, 5, 5);
            iloNewDeleteBtn.Tag = iloFullStack.Children.Count;
            iloNewDeleteBtn.AddHandler(Button.ClickEvent, new RoutedEventHandler(iloDeleteBtn_Click));
            iloNewDeleteBtn.Content = new Image
            {
                Source = new BitmapImage(new Uri("pack://application:,,,/img/icons/baseline_delete_black_18dp.png")),
                Height = 30,
                Width = 30,
            };

            StackPanel wrapstack = new StackPanel();
            wrapstack.Name = "rowStk_" + iloFullStack.Children.Count;
            wrapstack.Orientation = Orientation.Horizontal;
            wrapstack.Children.Add(iloNewNumberTb);
            wrapstack.Children.Add(iloNewContentTxt);
            wrapstack.Children.Add(iloNewDeleteBtn);

            iloFullStack.Children.Add(wrapstack);
            updateAddButtonState();
        }
        #endregion
    }
}
