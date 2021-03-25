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
using System.Windows.Shapes;

namespace CATS
{
    /// <summary>
    /// Interaction logic for NewImageDialog.xaml
    /// </summary>
    public partial class NewImageDialog : Window
    {
        HtmlPage callingPage;

        string imagePath;
        string imageCaption;

        public NewImageDialog(HtmlPage caller)
        {
            InitializeComponent();
            callingPage = caller;
        }

        private void imageBrowseBtn_Click(object sender, RoutedEventArgs e)
        {
            string IMG_FILE_FILTER =
                "Image Files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg";

            OpenFileDialog ofd = new OpenFileDialog()
            {
                Filter = IMG_FILE_FILTER,
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)
            };
            if (ofd.ShowDialog() == true) {
                imagePath = ofd.FileName;
                imagePathTxt.Text = imagePath;
            }
        }

        private void okBtn_Click(object sender, RoutedEventArgs e)
        {
            if (captionTxt.Text.Length > 0) {
                imageCaption = captionTxt.Text;
            } else {
                imageCaption = String.Empty;
            }
            callingPage.insertHtmlImageWithCaption(imagePath, imageCaption);
            this.Visibility = Visibility.Collapsed;
        }

        private void cancelBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
        }
    }
}
