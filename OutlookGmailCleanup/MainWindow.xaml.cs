using System;
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

namespace OutlookGmailCleanup
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

        private void ScanButton_OnClick(object sender, RoutedEventArgs e)
        {
            // use outlook primary interop to get all mail messages from sunnywiz@gmail.com account in outlook
            var outlookApp = new Microsoft.Office.Interop.Outlook.Application();
            try
            {
                ErrorText.Text = "Working...";
                var outlookNamespace = outlookApp.GetNamespace("MAPI");

                // this top level is accounts
                var folders = outlookNamespace.Folders;
                // get the folder for sunnywiz@gmail.com from folders
                var emailAccount = folders[EmailAccountText.Text];
                // get the folder inbox
                var inboxFolder = emailAccount.Folders[InboxFolderText.Text];
                MessagesCountText.Text = inboxFolder.Items.Count.ToString(); 
            }
            catch (Exception ex)
            {
                ErrorText.Text = ex.Message;
            }
            finally
            {
                if (outlookApp != null ) { outlookApp.Quit();}
            }
        }
    }
}
