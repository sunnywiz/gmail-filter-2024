using System;

namespace GmailFilterWpf;

public class SlimEmail
{
    public string Id { get; set; }
    public string Etag { get; set; }
    public string From { get; set; }
    public string Subject { get; set; }
    public DateTime Date { get; set; }
    public string ThreadId { get; set; }
}