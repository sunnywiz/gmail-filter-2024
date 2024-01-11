using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Threading;
using GmailFilterLibrary;

namespace GmailFilterWpf;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private GmailFilter1 _gmf;
    private List<SlimEmail> SlimEmails { get; set; }

    public MainWindow()
    {
        InitializeComponent();
    }

    private async void ConnectButton_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            StatusText.Text = "Connecting...";
            StatusText.Foreground = Brushes.Black;
            _gmf = new GmailFilter1();

            _gmf.Log = (m) => { Dispatcher.Invoke(() => { StatusText.Text = m; }, DispatcherPriority.Render); };
            _gmf.Connect(CredentialsFileText.Text, TokenFileText.Text);
            _gmf.Log("Connected");

            int numDaysToLoad = int.Parse(DaysToLoadText.Text);

            _gmf.LoadEmails(numDaysToLoad);
            _gmf.Log("Done Loading");

            SlimEmails = new List<SlimEmail>();
            
            foreach (var email in _gmf.Emails)
            {
                // extract Sender's email address, subject, and date received from email
                
                string senderEmail = email.Payload.Headers.FirstOrDefault(h => h.Name == "From")?.Value;
                
                string subject = email.Payload.Headers.FirstOrDefault(h => h.Name == "Subject")?.Value;
                string dateReceived = email.Payload.Headers.FirstOrDefault(h => h.Name == "Date")?.Value;
                // if all of these are filled out and paresable, create a SlimEmail with these values
                if (!string.IsNullOrEmpty(senderEmail) && !string.IsNullOrEmpty(subject) && DateTime.TryParse(dateReceived, out DateTime receivedDate))
                {
                    SlimEmail slimEmail = new SlimEmail { Id = email.Id, ThreadId = email.ThreadId, From = senderEmail, Subject = subject, Date = receivedDate };
                    SlimEmails.Add(slimEmail);
                }
            }
            // populate ResultGrid with SlimEmails
            ResultGrid.ItemsSource = SlimEmails;
            
// TODO: save emails in a local cache
// TODO: load emails from local cache
// TODO: explicitly set the datagrid so that its grouped by sender, and doesn't show id and threadid
// TODO: will need our own status of delete, "want to delete", "we deleted it"
// TODO: will need our own status of read/not read, "mark as not read"
// TODO: local delete stuff from local cache when its too old
// TODO: datagrid row button to add to "clean up" list with # of messages to keep, and whether to mark as read
// TODO: CLEANUP button to do the cleanup 
        }
        catch (Exception ex)
        {
            StatusText.Foreground = Brushes.Red; 
            StatusText.Text = ex.Message; 
        }
    }

    public class SlimEmail
    {
        public string Id { get; set; }
        public string From { get; set; }
        public string Subject { get; set; }
        public DateTime Date { get; set; }
        public string ThreadId { get; set; }
    }
}