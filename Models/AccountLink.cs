namespace systemforstudent.Models;

// Credentials and tokens belong to Security API; this table only links its username to study data.
public class AccountLink
{
    public string Username { get; set; } = "";
    public string Role { get; set; } = "";
    public int EntityId { get; set; }
}
