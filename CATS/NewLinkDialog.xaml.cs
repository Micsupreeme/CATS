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

namespace CATS
{
    /// <summary>
    /// Interaction logic for NewLinkDialog.xaml
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
