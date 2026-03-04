namespace PeopleExercise.Web.Services;

public interface IGuideHintService
{
    Task<string> GetHintByHookAsync(string hook);
}
