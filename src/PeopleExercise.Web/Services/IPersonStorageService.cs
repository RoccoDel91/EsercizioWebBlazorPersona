using PeopleExercise.Web.Models;

namespace PeopleExercise.Web.Services;

public interface IPersonStorageService
{
    Task<IReadOnlyList<Persona>> GetAllAsync();

    Task<Persona?> GetByIdAsync(Guid id);

    Task SaveAsync(Persona person);

    Task DeleteAsync(Guid id);

    Task OpenFilePath(Guid id);
}
    