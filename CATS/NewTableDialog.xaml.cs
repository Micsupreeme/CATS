using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CATS
{
    /// <summary>
    /// Interaction logic for NewTableDialog.xaml - THIS DIALOG IS NOT CURRENTLY BEING USED BECAUSE IT IS CALLED ONLY BY OBSOLETE HtmlPage
    /// </summary>
    public partial class NewTableDialog : Window
    {
        private const int COLUMNS_LIMIT = 9;
        private const int ROWS_LIMIT = 7;

        HtmlPage callingPage;
        int[] selectedCell = new int[2]{0, 0}; //The clicked cell determines the size of the grid (e.g., click [3, 3] for a 3x3 grid)

        Rectangle highlightXRect, highlightYRect; //Temporary rectangles that appear/disappear and point to the cursor to help visualise grid size

        SolidColorBrush cellUnselectedBrush = new SolidColorBrush(Color.FromRgb(255, 255, 255));
        SolidColorBrush cellSelectedBrush = new SolidColorBrush(Color.FromRgb(255, 255, 0));

        public NewTableDialog(HtmlPage caller)
        {
            InitializeComponent();
            callingPage = caller;

            //Reset everything; ensure the canvas is empty
            int leftOffset = 0;
            int topOffset = 0;
            tableSizeCanvas.Children.Clear();

            for (int columns = 0; columns < COLUMNS_LIMIT; columns++)
            {
                for (int rows = 0; rows < ROWS_LIMIT; rows++)
                { 
                    Rectangle rect = new Rectangle();
                    rect.Fill = cellUnselectedBrush;
                    rect.Width = 20;
                    rect.Height = 20;
                    rect.Tag = columns + "x" + rows;
                    rect.AddHandler(MouseEnterEvent, new RoutedEventHandler(rect_MouseEnter));
                    rect.AddHandler(MouseLeaveEvent, new RoutedEventHandler(rect_MouseLeave));
                    rect.AddHandler(MouseUpEvent, new RoutedEventHandler(rect_MouseUp));
                    tableSizeCanvas.Children.Add(rect);
                    Canvas.SetLeft(rect, leftOffset);
                    Canvas.SetTop(rect, topOffset);
                    Canvas.SetZIndex(rect, 5);
                    topOffset += 25;
                }
                topOffset = 0;
                leftOffset += 25;
            }
        }

        #region Event handlers
        /// <summary>
        /// When the mouse enters a grid rectangle
        /// </summary>
        private void rect_MouseEnter(object sender, RoutedEventArgs e)
        {
            //Select fill colour
            Rectangle sourceRect = (Rectangle)e.Source;
            sourceRect.Fill = cellSelectedBrush;

            //Table size preview
            string[] rectPosition = sourceRect.Tag.ToString().Split('x');
            selectedCell[0] = int.Parse(rectPosition[0]) + 1; //Array indexes start at 0 but people start at 1
            selectedCell[1] = int.Parse(rectPosition[1]) + 1;
            tableSizeTb.Text = (selectedCell[0] + "x" + selectedCell[1] + " Table");

            //Add temporary highlight rectangles to visualise the horizontal and vertical bounds
            tableSizeCanvas.Children.Remove(highlightXRect);
            highlightXRect = new Rectangle();
            highlightXRect.Fill = cellSelectedBrush;
            highlightXRect.Width = selectedCell[0] * 25;
            highlightXRect.Height = 20;
            tableSizeCanvas.Children.Add(highlightXRect);
            Canvas.SetLeft(highlightXRect, 0);
            Canvas.SetTop(highlightXRect, (selectedCell[1] * 25) - 25);
            Canvas.SetZIndex(highlightXRect, 0);

            tableSizeCanvas.Children.Remove(highlightYRect);
            highlightYRect = new Rectangle();
            highlightYRect.Fill = cellSelectedBrush;
            highlightYRect.Width = 20;
            highlightYRect.Height = selectedCell[1] * 25;
            tableSizeCanvas.Children.Add(highlightYRect);
            Canvas.SetLeft(highlightYRect, (selectedCell[0] * 25) - 25);
            Canvas.SetTop(highlightYRect, 0);
            Canvas.SetZIndex(highlightYRect, 1);
        }

        /// <summary>
        /// When the mouse leaves a grid rectangle
        /// </summary>
        private void rect_MouseLeave(object sender, RoutedEventArgs e)
        {
            //Unselect fill colour
            Rectangle sourceRect = (Rectangle)e.Source;
            sourceRect.Fill = cellUnselectedBrush;
        }

        /// <summary>
        /// When a grid rectangle is clicked
        /// </summary>
        private void rect_MouseUp(object sender, RoutedEventArgs e)
        {
            callingPage.insertHtmlTableWithSize(selectedCell[0], selectedCell[1], (bool)headerCellsChe.IsChecked);
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
