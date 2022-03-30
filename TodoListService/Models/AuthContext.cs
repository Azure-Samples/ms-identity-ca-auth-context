namespace TodoListService.Models;

/// <summary>
/// Represents an AuthContext record in DB
/// </summary>
public class AuthContext
{
    public string TenantId { get; set; }

    // the auth context Id
    public string AuthContextId { get; set; }        

    public string AuthContextDisplayName { get; set; }

    public string Operation { get; set; }
}
