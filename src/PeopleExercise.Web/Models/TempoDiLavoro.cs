using System.Text.Json.Serialization;

namespace PeopleExercise.Web.Models;

public sealed class TempoDiLavoro
{
    [JsonPropertyName("sessioni")]
    public List<IntervalloLavoro> Sessioni { get; set; } = [];
}

public sealed class IntervalloLavoro
{
    [JsonPropertyName("timeStart")]
    public DateTimeOffset TimeStart { get; set; }

    [JsonPropertyName("timeStop")]
    public DateTimeOffset? TimeStop { get; set; }
}
