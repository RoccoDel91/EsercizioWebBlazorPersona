namespace PeopleExercise.Web.Configuration;

public sealed class StorageOptions
{
    public const string SectionName = "Storage";

    public string PeopleDirectoryPath { get; set; } = "Data/People";
    public string TodoFilePath { get; set; } = "Data/ToDo/tasks.json";
    public string GuideFilePath { get; set; } = "../../docs/Exercise_Guide.md";
}
