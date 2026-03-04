using System.Text.Json.Serialization;

namespace PeopleExercise.Web.Models;

public sealed class TodoTaskItem
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("obbiettivo")]
    public string Obbiettivo { get; set; } = string.Empty;

    [JsonPropertyName("completato")]
    public bool Completato { get; set; }

    [JsonPropertyName("onGoing")]
    public bool OnGoing { get; set; }

    [JsonPropertyName("noTimer")]
    public bool NoTimer { get; set; }

    [JsonPropertyName("tempoDiLavoro")]
    public TempoDiLavoro TempoDiLavoro { get; set; } = new();

    [JsonPropertyName("hookToGuide")]
    public string HookToGuide { get; set; } = string.Empty;
}
