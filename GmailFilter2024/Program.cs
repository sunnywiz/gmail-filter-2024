using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.IO;
using System.Threading;

public class GmailAPI
{
    static string[] Scopes = { GmailService.Scope.GmailReadonly };
    static string ApplicationName = "Sunny Gmail 2024 Filter";
    public static void Main(string[] args)
    {
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

        var labelListRequest = service.Users.Labels.List("me");
        var labelListResponse = labelListRequest.Execute();
        if (labelListResponse == null) throw new NullReferenceException("Got back a null list of labels");
        foreach (var label in labelListResponse.Labels)
        {
            Console.WriteLine($"Label: {label.Id} {label.Name} {label.ETag} {label.LabelListVisibility} {label.MessageListVisibility} {label.MessagesTotal} {label.ThreadsTotal} {label.Type}");
        }
        
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
    }
}
