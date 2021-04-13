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
        #endregion
    }
}
