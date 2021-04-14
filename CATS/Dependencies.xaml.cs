using System.Diagnostics;
using System.Windows;

namespace CATS
{
    /// <summary>
    /// Interaction logic for Dependencies.xaml
    /// </summary>
    public partial class Dependencies : Window
    {
        public Dependencies()
        {
            InitializeComponent();
        }

        #region Event handlers
        /// <summary>
        /// When the OK button is clicked
        /// </summary>
        private void okBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// When the HtmlAgilityPack URL is clicked
        /// </summary>
        private void htmlAgilityUrlTb_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Process.Start("https://html-agility-pack.net/");
        }

        /// <summary>
        /// When the HtmlAgilityPack licence is clicked
        /// </summary>
        private void htmlAgilityLicenceTb_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Process.Start("licences\\HtmlAgilityPack LICENSE.txt");
        }

        /// <summary>
        /// When the LumenWorksCsvReader URL is clicked
        /// </summary>
        private void lumenUrlTb_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Process.Start("https://github.com/phatcher/CsvReader");
        }

        /// <summary>
        /// When the LumenWorksCsvReader licence is clicked
        /// </summary>
        private void lumenLicenceTb_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Process.Start("licences\\LumenWorksCsvReader LICENSE.txt");
        }

        /// <summary>
        /// When the Nancy URL is clicked
        /// </summary>
        private void nancyUrlTb_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Process.Start("http://nancyfx.org/");
        }

        /// <summary>
        /// When the Nancy licence is clicked
        /// </summary>
        private void nancyLicenceTb_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Process.Start("licences\\Nancy LICENSE.txt");
        }

        /// <summary>
        /// When the Newtonsoft.Json URL is clicked
        /// </summary>
        private void newtonsoftUrlTb_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Process.Start("https://www.newtonsoft.com/json");
        }

        /// <summary>
        /// When the NewtonSoft.Json licence is clicked
        /// </summary>
        private void newtonsoftLicenceTb_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Process.Start("licences\\Newtonsoft.Json LICENSE.txt");
        }

        /// <summary>
        /// When the PDFsharp-MigraDoc URL is clicked
        /// </summary>
        private void migraDocUrlTb_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Process.Start("http://www.pdfsharp.net/");
        }

        /// <summary>
        /// When the PDFsharp-MigraDoc licence is clicked
        /// </summary>
        private void migraDocLicenceTb_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Process.Start("licences\\PDFsharp-MigraDoc LICENSE.txt");
        }

        /// <summary>
        /// When the Select.HtmlToPdf URL is clicked
        /// </summary>
        private void selectHtmlPdfUrlTb_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Process.Start("https://selectpdf.com/community-edition/");
        }

        /// <summary>
        /// When the Select.HtmlToPdf licence is clicked
        /// </summary>
        private void selectHtmlPdfLicenceTb_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Process.Start("licences\\Select.HtmlToPdf End User License Agreement.html");
        }

        /// <summary>
        /// When the Smith.WPF.HtmlEditor URL is clicked
        /// </summary>
        private void smithEditorUrlTb_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Process.Start("https://github.com/adambarath/SmithHtmlEditor");
        }

        /// <summary>
        /// When the Smith.WPF.HtmlEditor licence is clicked
        /// </summary>
        private void smithEditorLicenceTb_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Process.Start("licences\\Smith.WPF.HtmlEditor LICENSE.txt");
        }

        /// <summary>
        /// When the WindowsAPICodePack-Core URL is clicked
        /// </summary>
        private void windowsApiCoreUrlTb_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Process.Start("https://github.com/aybe/Windows-API-Code-Pack-1.1");
        }

        /// <summary>
        /// When the WindowsAPICodePack-Core licence is clicked
        /// </summary>
        private void windowsApiCoreLicenceTb_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Process.Start("licences\\WindowsAPICodePack-Core LICENCE.txt");
        }

        /// <summary>
        /// When the WindowsAPICodePack-Shell URL is clicked
        /// </summary>
        private void windowsApiShellUrlTb_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Process.Start("https://github.com/aybe/Windows-API-Code-Pack-1.1");
        }

        /// <summary>
        /// When the WindowsAPICodePack-Shell licence is clicked
        /// </summary>
        private void windowsApiShellLicenceTb_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Process.Start("licences\\WindowsAPICodePack-Shell LICENCE.txt");
        }
#endregion
    }
}
