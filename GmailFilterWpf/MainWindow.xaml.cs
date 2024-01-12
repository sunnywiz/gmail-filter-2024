using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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
    private List<SlimEmail> SlimEmails { get; set; }
    private ObservableCollection<GroupedEmailViewModel> _groupedEmails { get; set; }

    public MainWindow()
    {
        InitializeComponent();
        _groupedEmails = new ObservableCollection<GroupedEmailViewModel>();
        ResultGrid.ItemsSource = _groupedEmails;

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
                PopulateResults();
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

    private async void GetEmails_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // TODO 15 - Usability - Change from status bar to async filling as we go updating observable.
            // This means we'll have to start this work in a separate task and do the Invoke thing to get it on the UI thread
            // We should absorb the gmail helper class into some commands here, there's not enough to justify it being its own class
            
            StatusText.Text = "Connecting...";
            StatusText.Foreground = Brushes.Black;
            var gh = new MyGmailHelper();
            gh.Log = (m) => { Dispatcher.Invoke(() => { StatusText.Text = m; }, DispatcherPriority.Render); };
            gh.Connect(CredentialsFileText.Text, TokenFolderText.Text);
            gh.Log("Connected");

            int numDaysToLoad = int.Parse(DaysToLoadText.Text);

            var dict = SlimEmails.ToDictionary(x => x.Id);

            gh.LoadAdditionalEmails(numDaysToLoad, (m) => !dict.ContainsKey(m.Id));

            foreach (var email in gh.BareEmails)
            {
                if (dict.TryGetValue(email.Id, out var slim))
                {
                    slim.DeleteState = DeleteState.Alive;
                }
            }
            
            foreach (var email in gh.DetailedEmails)
            {
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
                        From = senderEmail,
                        Subject = subject,
                        Date = receivedDate, 
                        DeleteState = DeleteState.Alive
                    };
                    SlimEmails.Add(slimEmail);
                }
            }
            // populate ResultGrid with SlimEmails, re-sorting it
            PopulateResults(); 

            // TODO 40 - UI -  things to be deleted should look a certain way
            // TODO 10 - Core -  We will need to persist our desired delete settings to a json file as well. and load from that file.  And save to it if changed.

            // TODO 60 - Bonus - customer filters for the query to gmail?  Defaults to after xxx ?  that way you can load ranges 
            // TODO 60 - Bonus - maybe a link to open the message in gmail? 
            // TODO 50 - Feature -  will need our own status of read/not read, "mark as not read".  Use bold for unread.  Gray for we deleted it
            // TODO 20 - LongerTermCore -  local delete stuff from local cache when its too old (setting)
            // TODO 10 - Usability - should not have to manually save local store, should happen any time something updates or on a timer and always on exit.
            // TODO 10 - Core -  all in one button which goes and gets newer stuff, runs the filters, runs the "send cleanup", runs the local store cleanup, and saves the local store.
            
        }
        catch (Exception ex)
        {
            StatusText.Foreground = Brushes.Red;
            StatusText.Text = ex.Message;
        }
    }

    private void PopulateResults()
    {
        _groupedEmails.Clear();
        foreach (var email in SlimEmails
                     .GroupBy(x => x.From)
                     .Select(x => new GroupedEmailViewModel(x))
                ) _groupedEmails.Add(email); 
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

    private void PruneNowButton_OnClick(object sender, RoutedEventArgs e)
    {
        var g = ((Button)sender).DataContext as GroupedEmailViewModel;
        if (g == null) return;

        PruneGroup(g);
    }

    private void PruneGroup(GroupedEmailViewModel g)
    {
        if (!String.IsNullOrEmpty(g.NumToKeep) && int.TryParse(g.NumToKeep, out var dKeep))
        {
            var sorted = g.Emails.OrderByDescending(x => x.Date).ToList();
            foreach (var email in sorted)
            {
                if (email.DeleteState == DeleteState.Alive)
                {
                    dKeep--;
                    if (dKeep < 0)
                    {
                        email.DeleteState = DeleteState.PendingDelete;
                    }
                }
            }

            return; 
        }

        // no numtokeep specified, undelete anything that we were going to delete
        foreach (var email in g.Emails)
        {
            if (email.DeleteState == DeleteState.PendingDelete) email.DeleteState = DeleteState.Alive;
        }
    }

    private void SendDeletesButton_OnClick(object sender, RoutedEventArgs e)
    {
        // TODO 15 - Usability - Grab all the emails, we know how many, do a progress bar, async it, save local store after.
        var gh = new MyGmailHelper();
        gh.Log = (m) => { Dispatcher.Invoke(() => { StatusText.Text = m; }, DispatcherPriority.Render); };
        gh.Connect(CredentialsFileText.Text, TokenFolderText.Text);
        foreach (var g in _groupedEmails)
        {
            foreach (var email in g.Emails.Where(x => x.DeleteState == DeleteState.PendingDelete))
            {
                gh.TrashEmail(email.Id);
                email.DeleteState = DeleteState.Deleted;
            }
        }
    }

    public class GroupedEmailViewModel : INotifyPropertyChanged
    {
        private string _numToKeep;

        public GroupedEmailViewModel(IEnumerable<SlimEmail> emails)
        {
            Emails = emails.OrderByDescending(x=>x.Date).ToList();
        }
        public List<SlimEmail> Emails
        {
            get;
        }

        public string From => Emails.First().From;
        
        // TODO 20 - Usability - The count should be Active emails only 
        public int Count => Emails.Count;
        
        // TODO 21 - UI - We don't need to expose mindate 
        public DateTime MinDate => Emails.Min(x => x.Date);
        
        public DateTime MaxDate => Emails.Max(x => x.Date);

        public string NumToKeep
        {
            get => _numToKeep;
            set
            {
                if (value == _numToKeep) return;
                _numToKeep = value;
                OnPropertyChanged();
            }
        }

        // TODO 22 - Usability - Frequency does need to include historical emails
        public decimal? Frequency
        {
            get
            {
                if (Emails.Count < 2)
                {
                    return null;
                }
                else
                {
                    TimeSpan timeSpan = MaxDate - MinDate;
                    var totalDays = timeSpan.TotalDays;
                    if (totalDays < 1.0) return null; 
                    return (decimal)Emails.Count / (decimal)timeSpan.TotalDays;
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}