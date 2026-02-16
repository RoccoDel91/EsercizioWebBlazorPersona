namespace PeopleExercise.Web.Configuration;

public sealed class StorageOptions
{
    public const string SectionName = "Storage";

    public string PeopleDirectoryPath { get; set; } = "Data/People";
}
