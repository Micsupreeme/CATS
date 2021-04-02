using System.Text;
using System.Windows;
using System.Security.Cryptography;
using System.Windows.Media;

namespace CATS
{
    /// <summary>
    /// Interaction logic for AuthenticateDialog.xaml
    /// </summary>
    public partial class AuthenticateDialog : Window
    {
        private const string MAGIC_CODE = "4BB481F5364E957525D7FF7DF36DEEAE2FEDD4C010CA0EFCBDC7670DBAB520F8";
        private const int MAX_ATTEMPTS = 5;

        private MainWindow callingWindow;
        private int authenticationAttempts = 0;

        public AuthenticateDialog(MainWindow caller)
        {
            InitializeComponent();
            callingWindow = caller;
        }

        /// <summary>
        /// Gets the raw hash value from the specified string
        /// </summary>
        /// <param name="inputString">The password to translate to SHA-256</param>
        /// <returns>The password as a raw SHA-256 hash</returns>
        public static byte[] getSHA256Hash(string inputString)
        {
            using (HashAlgorithm sha256 = SHA256.Create())
                return sha256.ComputeHash(Encoding.UTF8.GetBytes(inputString));
        }

        /// <summary>
        /// Gets the readable hash value from the raw SHA-256 hash
        /// </summary>
        /// <param name="inputString">The password to translate to SHA-256</param>
        /// <returns>The password as a SHA-256 string</returns>
        public static string getSHA256HashString(string inputString)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in getSHA256Hash(inputString))
                sb.Append(b.ToString("X2"));

            return sb.ToString();
        }

        #region Event handlers
        private void submitBtn_Click(object sender, RoutedEventArgs e)
        {
            //Visual confirmation that the user has been elevated
            SolidColorBrush buRed1Brush = new SolidColorBrush(Color.FromRgb(247, 108, 89)); //#F48886

            //If the hashed content of the passwordbox equals the magic code, toggle elevated user mode
            if (getSHA256HashString(passPb.Password).Equals(MAGIC_CODE)) {
                callingWindow.toggleAuthenticated(true);
                callingWindow.OptionsAuthMi.Header = "_Authenticated";
                callingWindow.OptionsAuthMi.Background = buRed1Brush;
                this.Visibility = Visibility.Collapsed;
            } else { //Upon a failed attempt, count it
                authenticationAttempts++;
            }

            //If max number of attempts reached, disable this dialog
            if(authenticationAttempts == MAX_ATTEMPTS) {
                callingWindow.OptionsAuthMi.IsEnabled = false;
                this.Visibility = Visibility.Collapsed;
            }
        }

        private void cancelBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
        }
        #endregion
    }
}
