using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;

namespace GmailFilter2024;

public class GmailAPI
{
    // This depends on a credentials.json 
    // which depends on setting up a google api cloud project
    // which needs the gmail api (read/write) scope enabled 
    // and then add who you're going to log in as as a test user for that API 

    static string[] Scopes = { GmailService.Scope.GmailReadonly };
    static string ApplicationName = "Sunny Gmail 2024 Filter";
    public static void Main(string[] args)
    {
        /*
        UserCredential credential;
        using (var stream =
               new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
        {
            string credPath = "token.json";
            credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                GoogleClientSecrets.Load(stream).Secrets,
                Scopes,
                "user",
                CancellationToken.None,
                new FileDataStore(credPath, true)).Result;
            Console.WriteLine("Credential file saved to: " + credPath);
        }
        var service = new GmailService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = ApplicationName,
        });
        */
        /*
    var labelListRequest = service.Users.Labels.List("me");
    var labelListResponse = labelListRequest.Execute();
    if (labelListResponse == null) throw new NullReferenceException("Got back a null list of labels");
    foreach (var label in labelListResponse.Labels)
    {
        Console.WriteLine($"Label: {label.Id} {label.Name} {label.ETag} {label.LabelListVisibility} {label.MessageListVisibility} {label.MessagesTotal} {label.ThreadsTotal} {label.Type}");
    }
    */
        /*        UsersResource.MessagesResource.ListRequest request = service.Users.Messages.List("me");
            ListMessagesResponse response = request.Execute();
            if (response.Messages != null && response.Messages.Count > 0)
            {
                Console.WriteLine("Messages:");
                foreach (var messageItem in response.Messages)
                {
                    Console.WriteLine("- {0}", messageItem.Id);
                }
            }
            else
            {
                Console.WriteLine("No new messages.");
            }
    */
        // I need code to get a list of email messages including From, Subject, Date from gmail in category CATEGORY_UPDATES
        /*
        UsersResource.MessagesResource.ListRequest messageListRequest = service.Users.Messages.List("me");
        messageListRequest.Q = "in:updates";  // as opposed to label:xxx
        ListMessagesResponse messageListResponse = messageListRequest.Execute();
        if (messageListResponse.Messages != null && messageListResponse.Messages.Count > 0)
        {
            Console.WriteLine("Messages:");
            foreach (var messageItem in messageListResponse.Messages)
            {
                var message = service.Users.Messages.Get("me", messageItem.Id).Execute();
                Console.WriteLine($"From: {message.Payload.Headers.FirstOrDefault(h => h.Name == "From")?.Value}");
                Console.WriteLine($"Subject: {message.Payload.Headers.FirstOrDefault(h => h.Name == "Subject")?.Value}");
                Console.WriteLine($"Date: {message.Payload.Headers.FirstOrDefault(h => h.Name == "Date")?.Value}");
            }
        }
        else
        {
            Console.WriteLine("No messages found");
        }
        */
        
    }
}