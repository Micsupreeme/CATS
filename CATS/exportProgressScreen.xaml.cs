using System.Windows;

namespace CATS
{
    /// <summary>
    /// Interaction logic for exportProgressScreen.xaml
    /// </summary>
    public partial class exportProgressScreen : Window
    {
        private int currentExportMode;

        /*
            Export modes:
            0 = Export to PDF (draft)
            1 = Export to PDF (final)
         */
        public exportProgressScreen(int exportMode)
        {
            InitializeComponent();

            //Set initial UI text based on the selected export mode
            currentExportMode = exportMode;
            switch(currentExportMode) {
                case 0:
                    exportTb.Text = "Export to PDF in Progress...";
                    exportStatusTb.Text = "Initialising document generator";
                    break;
                case 1:
                    exportTb.Text = "Export to Final PDF in Progress...";
                    exportStatusTb.Text = "Initialising document generator";
                    break;
            }
        }

        private void exportProg_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if(currentExportMode < 2) {
                switch (exportProg.Value) {
                    case 0:
                        exportStatusTb.Text = "Initialising document generator";
                        break;
                    case 10:
                        exportStatusTb.Text = "Generating coursework assignment brief document";
                        break;
                    case 80:
                        exportStatusTb.Text = "Saving interim coursework assignment brief";
                        break;
                    case 90:
                        exportStatusTb.Text = "Processing appendices and page numbers";
                        break;
                    case 100:
                        exportStatusTb.Text = "Export complete";
                        break;
                }
            }
        }
    }
}
