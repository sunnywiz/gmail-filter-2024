using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;

namespace GmailFilterLibrary;

public class MyGmailHelper
{
    static string[] Scopes = { GmailService.Scope.GmailModify };
    static string ApplicationName = "Sunny Gmail 2024 Filter";
    private GmailService _gmailService;

    public void Connect(string credentialFile, string credFolderPath)
    {
        UserCredential credential;
        using (var stream =
               new FileStream(credentialFile, FileMode.Open, FileAccess.Read))
        {
            credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                GoogleClientSecrets.Load(stream).Secrets,
                Scopes,
                "user",
                CancellationToken.None,
                new FileDataStore(credFolderPath, true)).Result;
            Console.WriteLine("Credential file saved to: " + credFolderPath);
        }

        _gmailService = new GmailService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = ApplicationName,
        });
    }

    public List<Message> BareEmails { get; set; }
    public List<Message> DetailedEmails { get; set; }

    public Action<string> Log { get; set; }

    public void LoadAdditionalEmails(int numDaysToLoad, Func<Message, bool> shouldILoadDetails)
    {
        BareEmails = new List<Message>();
        DetailedEmails = new List<Message>();
        // load several batches of emails from gmail inbox "me" until we get older than 30 days ago
        var currentDate = DateTime.Now;
        var oldestDate = currentDate.AddDays(-numDaysToLoad);
        var request = _gmailService.Users.Messages.List("me");
        request.Q = $"after:{oldestDate.ToString("yyyy/MM/dd")}";
        request.MaxResults = 100;
        do
        {
            var response = request.Execute();
            var messages = response.Messages;
            if (messages != null)
            {
                foreach (var message in messages)
                {
                    if (shouldILoadDetails(message))
                    {
                        var email = _gmailService.Users.Messages.Get("me", message.Id).Execute();
                        if (email != null)
                        {
                            DetailedEmails.Add(email);
                        }
                    }
                    else
                    {
                        BareEmails.Add(message);
                    }
                }

                Log?.Invoke($"Loaded {BareEmails.Count} bare and {DetailedEmails.Count} detailed emails so far");
            }

            request.PageToken = response.NextPageToken;
        } while (!string.IsNullOrEmpty(request.PageToken));
    }

    public void TrashEmail(string emailId)
    {
        _gmailService.Users.Messages.Trash("me", emailId).Execute();
    }
}