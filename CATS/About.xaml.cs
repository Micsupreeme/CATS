using System.Diagnostics;
using System.Reflection;
using System.Windows;

namespace CATS
{
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class About : Window
    {
        public About()
        {
            //Get publish details for the application
            string APPLICATION_NAME = Assembly.GetExecutingAssembly().GetName().Name.ToString();
            string APPLICATION_VERSION = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            InitializeComponent();

            //Write application details to UI
            appNameTb.Text = "Coursework Assessment Transmogrification System (" + APPLICATION_NAME + ")";
            appVersionTb.Text = "Version 1.0.0";
            appCopyrightTb.Text = "Copyright \u00A9 Michael Whatley 2021";
            appPublisherTb.Text = "Micsupreeme";
            appDescriptionTxt.Text = 
                "This is a bespoke application for the Computing and Informatics department at Bournemouth University. " +
                "It is designed to dynamically generate PDF coursework assignment briefs and minimise the room for formatting errors " +
                "by utilising a diverse set of intuitive form-based controls.\n\nSimply complete the fields like an online survey, " +
                "navigating with the \"Next\" and \"Previous\" buttons. Finally, click the \"Export to PDF\" button to generate " +
                "a format-compliant coursework assignment brief for review with a single click.";
            appIconsTb.Text = "Generic icons by ";
            appIconsLinkTb.Text = "Material Design";
        }

        #region Event handlers
        /// <summary>
        /// When the application publisher is clicked (which appears like a hyperlink)
        /// </summary>
        private void appPublisherTb_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Process.Start("https://github.com/Micsupreeme");
        }

        /// <summary>
        /// When the icons link is clicked (which appears like a hyperlink)
        /// </summary>
        private void appIconsLinkTb_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Process.Start("https://material.io/resources/icons");
        }

        /// <summary>
        /// When the Dependencies button is clicked
        /// </summary>
        private void dependenciesBtn_Click(object sender, RoutedEventArgs e)
        {
            Dependencies dependencies = new Dependencies();
            dependencies.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// When the OK button is clicked
        /// </summary>
        private void okBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
        }
        #endregion
    }
}
