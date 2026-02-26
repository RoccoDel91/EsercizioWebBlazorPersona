using PeopleExercise.Web.Models;

namespace PeopleExercise.Web.Services;

public interface IPersonStorageService
{
    Task<IReadOnlyList<Persona>> GetAllAsync();

    // CAMBIATO: da Guid a string
    Task<Persona?> GetByIdAsync(string id);

    Task SaveAsync(Persona person);

    // CAMBIATO: da Guid a string
    Task DeleteAsync(string id);

    // CAMBIATO: da Guid a string
    Task OpenFilePath(string id);
}