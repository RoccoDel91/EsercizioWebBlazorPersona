namespace PeopleExercise.Web.Services;

public interface IUserNotificationStore
{
    void AddWarning(string warningMessage);

    IReadOnlyList<string> DrainWarnings();
}
