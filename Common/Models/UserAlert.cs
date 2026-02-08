using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;
using Common.Enums;
using Common.Models.Interface;

namespace Common.Models;

public class UserAlert : IEntityId
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public UserAlertType AlertType { get; set; }

    // Store as JSON string in database
    public string OptionsJson { get; set; }

    // Not mapped property for easy access
    [NotMapped]
    [JsonIgnore]
    public Dictionary<string, string> Options
    {
        get => string.IsNullOrEmpty(OptionsJson)
            ? new Dictionary<string, string>()
            : JsonSerializer.Deserialize<Dictionary<string, string>>(OptionsJson);
        set => OptionsJson = JsonSerializer.Serialize(value);
    }
}