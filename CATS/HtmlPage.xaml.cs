using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Interaction logic for HtmlPage.xaml
    /// </summary>
    public partial class HtmlPage : Page
    {
        private const int WEBBROWSER_MAX_SCROLL = 10000000;
        private const string HTML_STYLE_BLOCK_PREPEND = 
            //For some reason, the WPF WebBrowser control does not recognise the block-margin-auto image-centering CSS rule,
            //so the CSS for the preview is slightly inferior - using fixed side margins of 25% which provide a similar look
            "<style type=\"text/css\">" +
                "h1 {font-size: 12pt;}" +
                "h1, h2 {font-weight: bold;}" +
                "ol, ul {margin-top: 3px; margin-bottom: 3px;}" +
                "td, th {padding: 2px; border: 1px solid black;}" +
                ".caption {margin-left: 25%; margin-right: 25%; line-height: 25px;}" +
                "img {margin-left: 25%; margin-right: 25%;}" +
                "table {width: 70%; border-collapse: collapse; display: block; margin-left: 15%; margin-right: 15%;}" +
                "@page {margin: 2.54cm;}" +
                "* {font-family: Arial; font-size: 10pt;}" +
            "</style>";

        private PagedWindow callingWindow; //HTML page needs to identify its calling window so it can enable/disable navigational controls based on whether or not there are unsaved changed
        private BUAssessment currentBua;
        private string areaBeingEdited;
        private bool hasUnsavedChanges = false;
        private bool isPopulated = false;

        public HtmlPage(PagedWindow caller, BUAssessment bua, string area)
        {
            InitializeComponent();
            callingWindow = caller;
            currentBua = bua;
            areaBeingEdited = area;
            editAreaTb.Text = areaBeingEdited + ":";
            populateFields();
        }

        /// <summary>
        /// Upon page load, populate the HTML editor with the HTML body stored in the bua object
        /// </summary>
        private void populateFields()
        {
            switch (areaBeingEdited)
            {
                case "Assessment Task":
                    htmlTxt.Text = prepareHtml(currentBua.assessmentTaskHtml, false);
                    break;
                case "Submission Format":
                    htmlTxt.Text = prepareHtml(currentBua.submissionFormatHtml, false);
                    break;
                case "Marking Criteria":
                    htmlTxt.Text = prepareHtml(currentBua.markingCriteriaHtml, false);
                    break;
                default:
                    Console.Error.WriteLine("ERROR: Invalid HTML editing area specified");
                    break;
            }
            isPopulated = true;
        }

        //HTML Markup editor controls
        private void htmlTxt_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(!hasUnsavedChanges && isPopulated) { 
                //There were not previously any unsaved changes, and the HTML textbox has already been prepopulated, so this is a real unsaved change
                hasUnsavedChanges = true;
                saveBtn.Visibility = Visibility.Visible;
                callingWindow.prevBtn.Visibility = Visibility.Hidden;
                callingWindow.nextBtn.Visibility = Visibility.Hidden;
            }
            if (autoRefreshTog.IsChecked == true) {
                refreshPreview();
            }
        }

        private void toggleBoldBtn_Click(object sender, RoutedEventArgs e)
        {
            htmlTxt.SelectedText = "<b>" + htmlTxt.SelectedText + "</b>";
        }

        private void toggleItalicBtn_Click(object sender, RoutedEventArgs e)
        {
            htmlTxt.SelectedText = "<i>" + htmlTxt.SelectedText + "</i>";
        }

        private void toggleUnderlineBtn_Click(object sender, RoutedEventArgs e)
        {
            htmlTxt.SelectedText = "<u>" + htmlTxt.SelectedText + "</u>";
        }

        private void insertHyperlinkBtn_Click(object sender, RoutedEventArgs e)
        {
            NewLinkDialog newlinkdialog = new NewLinkDialog(this, htmlTxt.SelectedText);
            newlinkdialog.Visibility = Visibility.Visible;
        }

        private void toggleBulletListBtn_Click(object sender, RoutedEventArgs e)
        {
            toggleHtmlBullets(false);
        }

        private void toggleOrderedListBtn_Click(object sender, RoutedEventArgs e)
        {
            toggleHtmlBullets(true);
        }

        private void insertImageBtn_Click(object sender, RoutedEventArgs e)
        {
            NewImageDialog newimagedialog = new NewImageDialog(this);
            newimagedialog.Visibility = Visibility.Visible;
        }

        private void insertTableBtn_Click(object sender, RoutedEventArgs e)
        {
            NewTableDialog newtabledialog = new NewTableDialog(this);
            newtabledialog.Visibility = Visibility.Visible;
        }

        private void zoomSlide_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try {
                htmlTxt.FontSize = zoomSlide.Value;
            } catch (NullReferenceException) {
                Console.Error.WriteLine("WARN: Event fired before object initialisation ");
            }
        }

        private void zoomOutImg_MouseUp(object sender, MouseButtonEventArgs e)
        {
            zoomSlide.Value -= 2;
        }

        private void zoomInImg_MouseUp(object sender, MouseButtonEventArgs e)
        {
            zoomSlide.Value += 2;
        }

        private void webRefreshBtn_Click(object sender, RoutedEventArgs e)
        {
            refreshPreview();
        }

        private void saveBtn_Click(object sender, RoutedEventArgs e)
        {
            saveChanges();           
        }

        private void saveChanges()
        {
            Console.WriteLine("save");
            hasUnsavedChanges = false;
            saveBtn.Visibility = Visibility.Hidden;
            callingWindow.prevBtn.Visibility = Visibility.Visible;
            callingWindow.nextBtn.Visibility = Visibility.Visible;

            switch (areaBeingEdited)
            {
                case "Assessment Task":
                    currentBua.assessmentTaskHtml = prepareHtml(htmlTxt.Text, true);
                    break;
                case "Submission Format":
                    currentBua.submissionFormatHtml = prepareHtml(htmlTxt.Text, true);
                    break;
                case "Marking Criteria":
                    currentBua.markingCriteriaHtml = prepareHtml(htmlTxt.Text, true);
                    break;
                default:
                    Console.Error.WriteLine("ERROR: Invalid HTML editing area specified");
                    break;
            }
            currentBua.saveAsJson(callingWindow.currentFilePath);
        }

        /// <summary>
        /// Adds/removes HTML visual clutter tags (e.g. "<br>") for webbrowsers/humans
        /// </summary>
        /// <param name="html">The HTML string to transform</param>
        /// <param name="forWeb">If true, prepares the HTMl for the web (adds tags), else prepares the HTML for a human (removes tags)</param>
        /// <returns>The transformed version of the "html" input string</returns>
        private string prepareHtml(string html, bool forWeb)
        {
            string treatedHtml;
            if(forWeb)
            {
                treatedHtml = Regex.Replace(html, Environment.NewLine, "<br>"); //Prevent users from having to use <br> tags manually
                treatedHtml = Regex.Replace(treatedHtml, "</ul><br>", "</ul>"); //Automated <br> tags adds extra below lists and tables, remove them
                treatedHtml = Regex.Replace(treatedHtml, "</ol><br>", "</ol>");
                treatedHtml = Regex.Replace(treatedHtml, "</table><br>", "</table>");
                return treatedHtml;
            } else {
                treatedHtml = Regex.Replace(html, "</ul>", "</ul><br>");
                treatedHtml = Regex.Replace(treatedHtml, "</ol>", "</ol><br>");
                treatedHtml = Regex.Replace(treatedHtml, "</table>", "</table><br>");
                treatedHtml = Regex.Replace(treatedHtml, "<br>", Environment.NewLine); //Users should not have to see <br> tags as they are visual clutter, remove them
                return treatedHtml;
            }
        }

        private void refreshPreview()
        {
            try {
                htmlWb.NavigateToString(HTML_STYLE_BLOCK_PREPEND + prepareHtml(htmlTxt.Text, true));
            } catch (NullReferenceException) {
                Console.Error.WriteLine("WARN: Event fired before object initialisation ");
            }
        }

        /// <summary>
        /// Adds a specified link and display text to the HTML editor
        /// NOTE: this method is public because it's executed by NewLinkDialog, which provides the facility to specify the link address and optional display text
        /// </summary>
        /// <param name="linkUrl">The URL of the link to be inserted</param>
        /// <param name="linkText">The optional display text (reader-friendly text) for the link to be inserted</param>
        public void insertHtmlLinkWithDisplayTxt(string linkUrl, string linkText)
        {
            
            if(linkText.Length > 0) {
                htmlTxt.SelectedText = "<a href=\"" + linkUrl + "\">" + linkText + "</a>";
            } else {
                htmlTxt.SelectedText = "<a href=\"" + linkUrl + "\">" + linkUrl + "</a>";
            }
        }

        /// <summary>
        /// Adds a specified (i.e., browsed for) image to the HTML editor with a specified caption
        /// NOTE: this method is public because it's executed by NewImageDialog, which provides the facility to specify the image and optional caption
        /// </summary>
        /// <param name="imageFullFilePath">The full file path of the image to be inserted</param>
        /// <param name="imageCaption">The optional caption text that can accompany the image</param>
        public void insertHtmlImageWithCaption(string imageFullFilePath, string imageCaption)
        {
            if(imageCaption.Length > 0) {
                htmlTxt.SelectedText += "<img src=\"" + imageFullFilePath + "\" style=\"width: auto; height: 300px;\"/>" + Environment.NewLine + "<i class=\"caption\">" + imageCaption + "</i>";
            } else {
                htmlTxt.SelectedText += "<img src=\"" + imageFullFilePath + "\" style=\"width: auto; height: 300px;\"/>";
            }
        }

        /// <summary>
        /// Adds a table to the HTML editor with a specified number of columns and rows
        /// NOTE: this method is public because it's executed by NewTableDialog, which provides the facility to specify the initial table size
        /// </summary>
        /// <param name="tableColumns">The number of columns for the new table</param>
        /// <param name="tableRows">The number of rows for the new table</param>
        public void insertHtmlTableWithSize(int tableColumns, int tableRows, bool includeHeaders)
        {
            string tableBody = "<table>\n";
            for(int rows = 0; rows < tableRows; rows++)
            {
                tableBody += "<tr>";
                for(int columns = 0; columns < tableColumns; columns++)
                {
                    //Make the first row header cells if the "include headers" checkbox was checked
                    if(rows == 0 && includeHeaders) {
                        tableBody += "<th>COL " + (columns + 1) + "</th>";
                    } else {
                        tableBody += "<td>CELL</td>";
                    }
                }
                tableBody += "</tr>\n";
            }
            tableBody += "</table>";
            htmlTxt.SelectedText += tableBody;
        }

        /// <summary>
        /// Adds an ordered list or an unordered list to the HTML editor
        /// </summary>
        /// <param name="numbered">If true, adds an ordered list, else adds an unordered list</param>
        private void toggleHtmlBullets(bool numbered)
        {
            string tagName;
            if(numbered) {
                tagName = "ol";
            } else{
                tagName = "ul";
            }
            string listBody = String.Empty;
            string[] bulletItems = htmlTxt.SelectedText.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            if (bulletItems.Length > 0)
            {
                //Toggle bullets for existing list
                foreach (string bullet in bulletItems)
                {
                    Console.WriteLine(bullet);
                    listBody += "\n<li>" + bullet + "</li>";
                }
                listBody = "<" + tagName + ">" + listBody + "\n</" + tagName + ">";
            }
            else
            {
                //New list
                listBody = "<ul>\n<li></li>\n</ul>";
            }
            htmlTxt.SelectedText = listBody;
        }

        /// <summary>
        /// Every time the web browser refreshes, if the auto-scroll toggle button is checked,
        /// ensure it scrolls to the bottom of the content (so that the user doesn't have to keep scrolling back down)
        /// </summary>
        private void htmlWb_LoadCompleted(object sender, NavigationEventArgs e)
        {
            if(autoScrollTog.IsChecked == true)
            {
                var htmlDoc = htmlWb.Document as mshtml.HTMLDocument;
                if (htmlDoc != null) {
                    htmlDoc.parentWindow.scroll(0, WEBBROWSER_MAX_SCROLL);
                }
            }
        }

        private void htmlWb_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            try {
                //Prevent external sources from loading in the preview browser
                if (htmlWb.Source.ToString().Length > 0) {
                    e.Cancel = true;
                }
            } catch (NullReferenceException) {
                Console.Error.WriteLine("WARN: Event fired before object initialisation ");
            }
        }
    }
}
