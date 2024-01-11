using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;

namespace GmailFilterLibrary
{
    public class GmailFilter1
    {
        static string[] Scopes = { GmailService.Scope.GmailReadonly };
        static string ApplicationName = "Sunny Gmail 2024 Filter";
        private GmailService _gmailService;

        public void Connect(string credentialFile, string credPath)
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
                    new FileDataStore(credPath, true)).Result;
                Console.WriteLine("Credential file saved to: " + credPath);
            }
            _gmailService = new GmailService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });
        }

        public List<Message> Emails { get; set; }
        
        public Action<string> Log { get; set; }
        
        public void LoadEmails(int numDaysToLoad)
        {
            Emails = new List<Message>();
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
                        var email = _gmailService.Users.Messages.Get("me", message.Id).Execute();
                        if (email != null)
                        {
                            Emails.Add(email); 
                        }
                    }
                    Log?.Invoke($"Loaded {Emails.Count} emails so far");
                }
                request.PageToken = response.NextPageToken;
            } while (!string.IsNullOrEmpty(request.PageToken));
         
        }
    }
}