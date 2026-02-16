namespace PeopleExercise.Web.Services;

public sealed class UserNotificationStore : IUserNotificationStore
{
    private readonly List<string> _warnings = [];

    public void AddWarning(string warningMessage)
    {
        if (string.IsNullOrWhiteSpace(warningMessage))
        {
            return;
        }

        _warnings.Add(warningMessage);
    }

    public IReadOnlyList<string> DrainWarnings()
    {
        var drained = _warnings.ToArray();
        _warnings.Clear();
        return drained;
    }
}
