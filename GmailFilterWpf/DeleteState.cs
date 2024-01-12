namespace GmailFilterWpf;

public class DeleteState
{
    public const string Alive = "Alive";  // not deleted
    public const string PendingDelete = "PendingDelete"; // we will delete this
    public const string Deleted = "Deleted";  // gmail has it deleted.  we no longer see it unless we search in:Trash
}