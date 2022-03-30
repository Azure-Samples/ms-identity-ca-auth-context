using System.Text.Json.Serialization;

namespace TodoListService.Models;

/// <summary>
/// Represents an AuthContext record in DB
/// </summary>
public class AuthContext
{
    [JsonPropertyName("tenantId")]
    public string TenantId { get; set; }

    // the auth context Id
    [JsonPropertyName("authContextId")]
    public string AuthContextId { get; set; }

    [JsonPropertyName("authContextDisplayName")]
    public string AuthContextDisplayName { get; set; }

    [JsonPropertyName("operation")]
    public string Operation { get; set; }
}
