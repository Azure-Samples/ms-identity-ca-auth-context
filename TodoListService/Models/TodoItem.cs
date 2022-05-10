using System.Text.Json.Serialization;

namespace TodoListService.Models;

public class Todo
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; }

    [JsonPropertyName("owner")]
    public string Owner { get; set; }
}
