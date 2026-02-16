namespace PeopleExercise.Web.Services;

public sealed class PersonStorageException : Exception
{
    public PersonStorageException(string userMessage, Exception innerException)
        : base(userMessage, innerException)
    {
        UserMessage = userMessage;
    }

    public string UserMessage { get; }
}
