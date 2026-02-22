using PeopleExercise.Web.Models;

namespace PeopleExercise.Web.Services;

public interface ITodoStorageService
{
    Task<IReadOnlyList<TodoTaskItem>> GetTasksAsync();
    Task UpdateTaskAsync(TodoTaskItem task);
    Task AddNewTaskAsync(TodoTaskItem task);
}