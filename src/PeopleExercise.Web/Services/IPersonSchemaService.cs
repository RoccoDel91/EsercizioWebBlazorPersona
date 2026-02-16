namespace PeopleExercise.Web.Services;

public interface IPersonSchemaService
{
    IReadOnlyList<PersonPropertyMetadata> GetPersonProperties();
}
