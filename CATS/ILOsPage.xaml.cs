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
    /// Interaction logic for ILOsPage.xaml
    /// </summary>
    public partial class ILOsPage : Page
    {
        private BUAssessment currentBua;

        public ILOsPage(BUAssessment bua)
        {
            InitializeComponent();
            currentBua = bua;
            createDynamicControls();
        }

        private void createDynamicControls()
        {
            //Ensure canvas is reset before drawing
            ilosCanvas.Children.Clear();
            ilosCanvas.Height = 10;
            int canvastop = 0; //keeps track of where to draw the next row of csvData controls ()

            List<StackPanel> iloRowStacks = new List<StackPanel>();

            StackPanel iloFullStack;
            //Draw records
            for (int ilo = 0; ilo < 4; ilo++)
            {
                ilosCanvas.Height += 400;

                var numberTb = new TextBlock();
                numberTb.Name = "nTb_" + ilo;
                numberTb.FontFamily = new FontFamily("Arial");
                numberTb.FontSize = 14;
                numberTb.Text = "0.";
                numberTb.Height = 35;
                numberTb.Width = 25;
                numberTb.Padding = new Thickness(5, 5, 5, 5);
                //ilosCanvas.Children.Add(numberTb);
                //Canvas.SetLeft(numberTb, 0);
                //Canvas.SetTop(numberTb, canvastop + 17.5);

                var iloContentTxt = new TextBox();
                iloContentTxt.Name = "icTxt_" + ilo;
                iloContentTxt.FontFamily = new FontFamily("Arial");
                iloContentTxt.FontSize = 14;
                iloContentTxt.Height = 70;
                iloContentTxt.Width = 400;
                iloContentTxt.Padding = new Thickness(5, 5, 5, 5);
                iloContentTxt.AcceptsReturn = true; //multiline textbox
                iloContentTxt.TextWrapping = TextWrapping.Wrap;
                iloContentTxt.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                iloContentTxt.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
                //ilosCanvas.Children.Add(iloContentTxt);
                //Canvas.SetLeft(iloContentTxt, 25);
                //Canvas.SetTop(iloContentTxt, canvastop);

                //Only show the "delete" button if it's not the first ILO - there must always be one
                if (ilo > 0)
                {
                    var iloDeleteBtn = new Button();
                    iloDeleteBtn.Name = "idBtn_" + ilo;
                    iloDeleteBtn.FontFamily = new FontFamily("Arial");
                    iloDeleteBtn.FontSize = 14;
                    iloDeleteBtn.Height = 70;
                    iloDeleteBtn.Width = 50;
                    iloDeleteBtn.Padding = new Thickness(5, 5, 5, 5);
                    iloDeleteBtn.Tag = ilo;
                    iloDeleteBtn.AddHandler(Button.ClickEvent, new RoutedEventHandler(iloDeleteBtn_Click));
                    //ilosCanvas.Children.Add(iloDeleteBtn);
                    //Canvas.SetLeft(iloDeleteBtn, 425);
                    //Canvas.SetTop(iloDeleteBtn, canvastop);

                    StackPanel singlestack = new StackPanel();
                    singlestack.Orientation = Orientation.Horizontal;
                    singlestack.Children.Add(numberTb);
                    singlestack.Children.Add(iloContentTxt);
                    singlestack.Children.Add(iloDeleteBtn);
                    iloRowStacks.Add(singlestack);
                } else
                {
                    StackPanel singlestack = new StackPanel();
                    singlestack.Orientation = Orientation.Horizontal;
                    singlestack.Children.Add(numberTb);
                    singlestack.Children.Add(iloContentTxt);
                    iloRowStacks.Add(singlestack);
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
        /// When a dynamic "delete" ILO button is clicked
        /// </summary>
        private void iloDeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            Button eventBtn = (Button)e.Source;
            int eventTag = (int)eventBtn.Tag; //the tag that indicates the row for all the controls to be deleted - it's appended onto their names
            List<int> childrenIndexesToRemove = new List<int>();
            int indexToRemoveFrom = -1;

            for(int controlCount = 0; controlCount < ilosCanvas.Children.Count; controlCount++)
            {
                if (ilosCanvas.Children[controlCount].GetType().Name.Equals("TextBlock")) {
                    TextBlock sourceTb = (TextBlock)ilosCanvas.Children[controlCount];
                    if (sourceTb.Name.Equals("nTb_" + eventTag)) {
                        childrenIndexesToRemove.Add(controlCount);
                        indexToRemoveFrom = controlCount;
                    }
                } else if(ilosCanvas.Children[controlCount].GetType().Name.Equals("TextBox")) {
                    TextBox sourceTxt = (TextBox)ilosCanvas.Children[controlCount];
                    if (sourceTxt.Name.Equals("icTxt_" + eventTag)) {
                        childrenIndexesToRemove.Add(controlCount);
                    }
                } else { //if it's not a textblock or textbox, it must be a button
                    //Button sourceBtn = (Button)ilosCanvas.Children[controlCount];
                    //if (sourceBtn.Name.Equals("idBtn_" + eventTag)) {
                        //childrenIndexesToRemove.Add(controlCount);
                    //}
                }
            }

            if(indexToRemoveFrom > 0) //it has a meaningful value, not -1
            {
                //If the index is 2 - to delete the ILO row we remove canvas children 2, 3 and 4
                //(i.e., always the range between the index, and the index + 2)
                ilosCanvas.Children.RemoveRange(indexToRemoveFrom, 3);
            }
        }

        private void ilosTb_MouseUp(object sender, MouseButtonEventArgs e)
        {
            currentBua.convertHtmlToPdf(currentBua.getHtmlDocument());
        }

        //to get accurate number of ILOs after random additions/deletions: (children + 1) / 3
    }
}
