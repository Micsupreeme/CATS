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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {           
            InitializeComponent();
        }

        private void createBtn_Click(object sender, RoutedEventArgs e)
        {
            PagedWindow pw = new PagedWindow();
            pw.Visibility = Visibility.Visible;
            this.Visibility = Visibility.Collapsed;
        }

        private void openBtn_Click(object sender, RoutedEventArgs e)
        {
            PagedWindow pw = new PagedWindow();
            pw.Visibility = Visibility.Visible;
            this.Visibility = Visibility.Collapsed;
        }
    }
}
