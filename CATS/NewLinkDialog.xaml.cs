using System;
using System.Windows;

namespace CATS
{
    /// <summary>
    /// Interaction logic for NewLinkDialog.xaml - THIS DIALOG IS NOT CURRENTLY BEING USED BECAUSE IT IS CALLED ONLY BY OBSOLETE HtmlPage
    /// </summary>
    public partial class NewLinkDialog : Window
    {
        HtmlPage callingPage;

        string linkUrl;
        string linkDisplayText;

        public NewLinkDialog(HtmlPage caller, string selectedText)
        {
            InitializeComponent();
            callingPage = caller;
            if(selectedText.Length > 0) {
                linkTextTxt.Text = selectedText;
            }
        }

        #region Event handlers
        /// <summary>
        /// When the OK button is clicked
        /// </summary>
        private void okBtn_Click(object sender, RoutedEventArgs e)
        {
            linkUrl = linkUrlTxt.Text;
            if (linkTextTxt.Text.Length > 0) {
                linkDisplayText = linkTextTxt.Text;
            } else {
                linkDisplayText = String.Empty;
            }
            callingPage.insertHtmlLinkWithDisplayTxt(linkUrl, linkDisplayText);
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
