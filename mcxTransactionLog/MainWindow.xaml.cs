using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using MySql.Data;
using MySql.Data.MySqlClient;
using mcxNOW;
using WebCookies;

namespace mcxTrans
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Api tradeAPI = new Api();
        private TransactionData transactionData = null;

        public MainWindow()
        {
            InitializeComponent();
            
            rbBrowserCookie.ToolTip = new ToolTip { Content = "Use the stored browser cookies as login. Assumes a previous valid login with the selected browser" };
            rbUserPass.ToolTip = new ToolTip { Content = "User your mcx user name and password to login" };
            btnStart.ToolTip = new ToolTip { Content = "Start storing the transactions in the Database" };
            btnExport.ToolTip = new ToolTip { Content = "Export the transactions to a csv file" };

            lbPassword.Visibility = Visibility.Hidden;
            lbUserName.Visibility = Visibility.Hidden;
            tbUserName.Visibility = Visibility.Hidden;
            pbPassword.Visibility = Visibility.Hidden;
            lbAuth.Visibility = Visibility.Hidden;
            tbAuth.Visibility = Visibility.Hidden;

            lbDBPassword.IsEnabled = false;
            lbDBUser.IsEnabled = false;
            lbDBHost.IsEnabled = false;

            tbHost.IsEnabled = false;
            tbUser.IsEnabled = false;
            tbPassword.IsEnabled = false;

        }

        private void Login()
        {
            if (rbBrowserCookie.IsChecked == true)
            {
                string secret = "";
                string session = "";
                Cookie.Browsers browser = Cookie.Browsers.CHROME; ;
                if (rbChrome.IsChecked == true) browser = Cookie.Browsers.CHROME;
                else if (rbFF.IsChecked == true) browser = Cookie.Browsers.FIREFOX;
                else if (rbIE.IsChecked == true) browser = Cookie.Browsers.IE;


                Cookie.GetCookie("mcxnow.com", "mcx_sess", ref session, browser);
                Cookie.GetCookie("mcxnow.com", "mcx_key", ref secret, browser);
                tradeAPI.Login(secret, session);
            }
            else
            {
                tradeAPI.Login(tbUserName.Text, pbPassword.Password, tbAuth.Text, true);
            }

            Database.DatabaseInitialiseData did;
            if (cbMySQL.IsChecked == true)
            {
                did = new Database.DatabaseInitialiseData
                {
                    Type = Database.DatabaseTypes.MYSQL,
                    DBName = @"mcxTransactionDB",
                    Host = tbHost.Text,
                    User = tbUser.Text,
                    Password = tbPassword.Text
                };
            }
            else
            {
                did = new Database.DatabaseInitialiseData
                {
                    Type = Database.DatabaseTypes.SQLITE,
                    DBName = @"mcxTransactionDB",
                    Host = "",
                    User = "",
                    Password = ""
                };
            }
            transactionData = new TransactionData(tradeAPI, did);
        }

        private void ButtonExport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Login();
                SaveFileDialog fd = new SaveFileDialog();
                fd.FileName = "Transactions";
                fd.DefaultExt = ".csv";
                fd.Filter = "CSV documents (.csv)|*.csv";
                Nullable<bool> result = fd.ShowDialog();

                if (result == true) transactionData.Export(fd.FileName);
            }
            catch (Exception ex)
            {
                if (ex is MySqlException) MessageBox.Show(ex.Message, "MySQL error");
                else MessageBox.Show(ex.Message, "Unknown error");
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Login();
                transactionData.StartFetchingPrice();
            }
            catch (Exception ex)
            {
                if (ex is MySqlException) MessageBox.Show(ex.Message, "MySQL error");
                else MessageBox.Show(ex.Message, "Unknown error");
            }
        }

        private void rbBrowserCookie_Click(object sender, RoutedEventArgs e)
        {
            rbChrome.Visibility = Visibility.Visible;
            rbIE.Visibility = Visibility.Visible;
            rbFF.Visibility = Visibility.Visible;
            rbChrome.IsChecked = true;

            lbPassword.Visibility = Visibility.Hidden;
            lbUserName.Visibility = Visibility.Hidden;
            tbUserName.Visibility = Visibility.Hidden;
            pbPassword.Visibility = Visibility.Hidden;
            lbAuth.Visibility = Visibility.Hidden;
            tbAuth.Visibility = Visibility.Hidden;
        }

        private void rbUserPass_Click(object sender, RoutedEventArgs e)
        {
            rbChrome.Visibility = Visibility.Hidden;
            rbIE.Visibility = Visibility.Hidden;
            rbFF.Visibility = Visibility.Hidden;

            lbPassword.Visibility = Visibility.Visible;
            lbUserName.Visibility = Visibility.Visible;
            tbUserName.Visibility = Visibility.Visible;
            pbPassword.Visibility = Visibility.Visible;
            lbAuth.Visibility = Visibility.Visible;
            tbAuth.Visibility = Visibility.Visible;
            lbPassword.Content = "Password:";
            lbUserName.Content = "Username:";

        }

        private void rbMcxNow_Click(object sender, RoutedEventArgs e)
        {
            brdLoginMethod.Visibility = Visibility.Visible;
            rbBrowserCookie.Visibility = Visibility.Visible;
            rbUserPass.Visibility = Visibility.Visible;
            rbBrowserCookie.IsChecked = true;
            rbChrome.Visibility = Visibility.Visible;
            rbIE.Visibility = Visibility.Visible;
            rbFF.Visibility = Visibility.Visible;
            rbChrome.IsChecked = true;

            lbPassword.Visibility = Visibility.Hidden;
            lbUserName.Visibility = Visibility.Hidden;
            tbUserName.Visibility = Visibility.Hidden;
            pbPassword.Visibility = Visibility.Hidden;

        }

        private void cbMySQL_Checked(object sender, RoutedEventArgs e)
        {
            lbDBPassword.IsEnabled = true;
            lbDBUser.IsEnabled = true;
            lbDBHost.IsEnabled = true;

            tbHost.IsEnabled = true;
            tbUser.IsEnabled = true;
            tbPassword.IsEnabled = true;
        }

        private void cbMySQL_Unchecked(object sender, RoutedEventArgs e)
        {
            lbDBPassword.IsEnabled = false;
            lbDBUser.IsEnabled = false;
            lbDBHost.IsEnabled = false;

            tbHost.IsEnabled = false;
            tbUser.IsEnabled = false;
            tbPassword.IsEnabled = false;
        }
    }
}
