using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
using Newtonsoft.Json;

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

        string localStoreFileName = LocalStoreText.Text;
        try
        {
            // load SlimEmails from local store
            if (File.Exists(localStoreFileName))
            {
                // Read the JSON string from a file
                string json = File.ReadAllText(localStoreFileName);
                // Deserialize the JSON string to a List<SlimEmails>
                SlimEmails = JsonConvert.DeserializeObject<List<SlimEmail>>(json);
                EmailCountText.Text = SlimEmails.Count.ToString();
                // populate EmailEarliestText and EmailLatestText from slimEmails
                EmailEarliestText.Text = SlimEmails.Min(e => e.Date).ToString("d");
                EmailLatestText.Text = SlimEmails.Max(e => e.Date).ToString("d");
                
                // populate ResultGrid with SlimEmails
                ResultGrid.ItemsSource = SlimEmails;
            }
            else throw new FileNotFoundException("file does not exist");
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Could not load from {localStoreFileName}: {ex.Message}";
            SlimEmails = new List<SlimEmail>();
            EmailEarliestText.Text = "N/A";
            EmailLatestText.Text = "N/A";
            EmailCountText.Text = "0";
        }
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

            var dict = SlimEmails.ToDictionary(x => x.Id);
            
            foreach (var email in _gmf.Emails)
            {
                if (dict.TryGetValue(email.Id, out var foundEmail))
                {
                    // we already have it, do we need to update it? based on etag i think. 
                    Trace.WriteLine("Check this");
                    continue; 
                }
                // Check if we already have this email. 
                
                // extract Sender's email address, subject, and date received from email

                string senderEmail = email.Payload.Headers.FirstOrDefault(h => h.Name == "From")?.Value;
                string subject = email.Payload.Headers.FirstOrDefault(h => h.Name == "Subject")?.Value;
                string dateReceived = email.Payload.Headers.FirstOrDefault(h => h.Name == "Date")?.Value;

                // if all of these are filled out and paresable, create a SlimEmail with these values
                if (!string.IsNullOrEmpty(senderEmail) &&
                    !string.IsNullOrEmpty(subject) &&
                    DateTime.TryParse(dateReceived, out DateTime receivedDate))
                {
                    SlimEmail slimEmail = new SlimEmail
                    {
                        Id = email.Id,
                        ThreadId = email.ThreadId,
                        Etag = email.ETag,
                        From = senderEmail,
                        Subject = subject,
                        Date = receivedDate
                    };
                    SlimEmails.Add(slimEmail);
                }
            }
            // populate ResultGrid with SlimEmails
            ResultGrid.ItemsSource = SlimEmails;

            // TODO: have LoadEmails skip loading details of the ones we already have loaded.
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
        public string Etag { get; set; }
        public string From { get; set; }
        public string Subject { get; set; }
        public DateTime Date { get; set; }
        public string ThreadId { get; set; }
    }

    private void SaveLocalCacheButton_OnClick(object sender, RoutedEventArgs e)
    {
        string localStoreFileName = LocalStoreText.Text;
        try
        {
            string json = JsonConvert.SerializeObject(SlimEmails, Formatting.Indented);
// Write the JSON string to a file
            File.WriteAllText(localStoreFileName, json);
            StatusText.Text = $"Saved to {localStoreFileName}";
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Could not load from {localStoreFileName}: {ex.Message}";
            SlimEmails = new List<SlimEmail>();
            EmailEarliestText.Text = "N/A";
            EmailLatestText.Text = "N/A";
            EmailCountText.Text = "0";
        }
    }
}