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
    /// Interaction logic for TitleLevelPage.xaml
    /// </summary>
    public partial class TitleLevelPage : Page
    {
        public TitleLevelPage()
        {
            InitializeComponent();
        }

        private void setSubState(int stateNumber)
        {
            try {
                if (stateNumber == 0) {
                    //Submission state
                    asmtNoXCmb.SelectedIndex = 0;
                    asmtNoYCmb.SelectedIndex = 1;
                    asmtNoXCmb.IsEnabled = true;
                    asmtNoYCmb.IsEnabled = true;
                } else {
                    //Resubmission state
                    asmtNoXCmb.SelectedIndex = 0;
                    asmtNoYCmb.SelectedIndex = 0;
                    asmtNoXCmb.IsEnabled = false;
                    asmtNoYCmb.IsEnabled = false; //Resubs are always "1 of 1"
                }
            } catch(NullReferenceException nrEx) {
                Console.Error.WriteLine("ERROR: Attempted to access uninstantiated control");
            }
        }

        private void subResubCmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (subResubCmb.SelectedValue.Equals("Submission")) {
                setSubState(0);
            } else {
                setSubState(1);
            }
        }
    }
}
