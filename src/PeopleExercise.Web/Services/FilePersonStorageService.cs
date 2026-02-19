using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using PeopleExercise.Web.Configuration;
using PeopleExercise.Web.Models;

namespace PeopleExercise.Web.Services;

public sealed class FilePersonStorageService : IPersonStorageService
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    private readonly string _peopleDirectoryPath;
    private readonly ILogger<FilePersonStorageService> _logger;
    private readonly IUserNotificationStore _notificationStore;

    public FilePersonStorageService(
        IOptions<StorageOptions> storageOptions,
        IWebHostEnvironment environment,
        ILogger<FilePersonStorageService> logger,
        IUserNotificationStore notificationStore)
    {
        ArgumentNullException.ThrowIfNull(storageOptions);
        ArgumentNullException.ThrowIfNull(environment);

        _logger = logger;
        _notificationStore = notificationStore;

        if (string.IsNullOrWhiteSpace(storageOptions.Value.PeopleDirectoryPath))
        {
            throw new InvalidOperationException("Storage:PeopleDirectoryPath non configurato.");
        }

        _peopleDirectoryPath = Path.IsPathRooted(storageOptions.Value.PeopleDirectoryPath)
            ? storageOptions.Value.PeopleDirectoryPath
            : Path.GetFullPath(Path.Combine(environment.ContentRootPath, storageOptions.Value.PeopleDirectoryPath));
       
    }

    public async Task<IReadOnlyList<Persona>> GetAllAsync()
    {
        try
        {
            EnsureDirectoryExists();

            var people = new List<Persona>();

            foreach (var filePath in Directory.EnumerateFiles(_peopleDirectoryPath, "*.json", SearchOption.TopDirectoryOnly))
            {
                try
                {
                    var jsonContent = await File.ReadAllTextAsync(filePath, Encoding.UTF8);
                    var person = JsonSerializer.Deserialize<Persona>(jsonContent, JsonSerializerOptions);

                    if (person is null)
                    {
                        RegisterWarning($"File '{Path.GetFileName(filePath)}' non valido: record ignorato.");
                        continue;
                    }

                    if (person.Id == Guid.Empty && Guid.TryParse(Path.GetFileNameWithoutExtension(filePath), out var parsedId))
                    {
                        person.Id = parsedId;
                    }

                    people.Add(person);
                }
                catch (JsonException jsonException)
                {
                    _logger.LogWarning(jsonException, "JSON corrotto in {FilePath}", filePath);
                    RegisterWarning($"File '{Path.GetFileName(filePath)}' corrotto: record ignorato.");
                }
            }

            return people;
        }
        catch (UnauthorizedAccessException unauthorizedAccessException)
        {
            throw BuildStorageException(
                "Permessi insufficienti per leggere la cartella persone.",
                unauthorizedAccessException);
        }
        catch (IOException ioException)
        {
            throw BuildStorageException(
                "Errore I/O durante la lettura dei file persona.",
                ioException);
        }
    }

    public async Task<Persona?> GetByIdAsync(Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Id non valido.", nameof(id));
        }

        try
        {
            EnsureDirectoryExists();

            var filePath = BuildFilePath(id);
            if (!File.Exists(filePath))
            {
                return null;
            }

            var jsonContent = await File.ReadAllTextAsync(filePath, Encoding.UTF8);
            return JsonSerializer.Deserialize<Persona>(jsonContent, JsonSerializerOptions);
        }
        catch (JsonException jsonException)
        {
            throw BuildStorageException(
                "Il file della persona contiene JSON non valido.",
                jsonException);
        }
        catch (UnauthorizedAccessException unauthorizedAccessException)
        {
            throw BuildStorageException(
                "Permessi insufficienti per leggere il file persona.",
                unauthorizedAccessException);
        }
        catch (IOException ioException)
        {
            throw BuildStorageException(
                "Errore I/O durante la lettura del file persona.",
                ioException);
        }
    }

    public async Task SaveAsync(Persona person)
    {
        ArgumentNullException.ThrowIfNull(person);

        if (person.Id == Guid.Empty)
        {
            person.Id = Guid.NewGuid();
        }

        try
        {
            EnsureDirectoryExists();

            var filePath = BuildFilePath(person.Id);

            // TODO (Exercise): Implementare serializzazione in SaveAsync.
            var serializedPerson = JsonSerializer.Serialize(person, JsonSerializerOptions);
            await File.WriteAllTextAsync(filePath, serializedPerson, Encoding.UTF8);
        }
        catch (JsonException jsonException)
        {
            throw BuildStorageException(
                "Errore di serializzazione JSON durante il salvataggio.",
                jsonException);
        }
        catch (UnauthorizedAccessException unauthorizedAccessException)
        {
            throw BuildStorageException(
                "Permessi insufficienti per salvare il file persona.",
                unauthorizedAccessException);
        }
        catch (IOException ioException)
        {
            throw BuildStorageException(
                "Errore I/O durante il salvataggio del file persona.",
                ioException);
        }
    }

    public Task DeleteAsync(Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Id non valido.", nameof(id));
        }

        try
        {
            var filePath = BuildFilePath(id);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            return Task.CompletedTask;
        }
        catch (UnauthorizedAccessException unauthorizedAccessException)
        {
            throw BuildStorageException(
                "Permessi insufficienti per eliminare il file persona.",
                unauthorizedAccessException);
        }
        catch (IOException ioException)
        {
            throw BuildStorageException(
                "Errore I/O durante l eliminazione del file persona.",
                ioException);
        }
    }

    public Task OpenFilePath(Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Id non valido.", nameof(id));
        }

        try
        {
            var filePath = BuildFilePath(id);

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("File persona non trovato.", filePath);
            }

            // Apri il file con l'app predefinita del sistema operativo, senza tenere handle aperti.
            var startInfo = new ProcessStartInfo
            {
                FileName = filePath,
                UseShellExecute = true
            };

            Process.Start(startInfo);

            return Task.CompletedTask;
        }
        catch (UnauthorizedAccessException unauthorizedAccessException)
        {
            throw BuildStorageException(
                "Permessi insufficienti per aprire il file persona.",
                unauthorizedAccessException);
        }
        catch (IOException ioException)
        {
            throw BuildStorageException(
                "Errore I/O durante l'apertura del file persona.",
                ioException);
        }
        catch (Exception exception) when (exception is InvalidOperationException or System.ComponentModel.Win32Exception)
        {
            throw BuildStorageException(
                "Impossibile aprire il file con l'app predefinita.",
                exception);
        }
    }

    private string BuildFilePath(Guid id)
    {
        return Path.Combine(_peopleDirectoryPath, $"{id}.json");
    }

    private void EnsureDirectoryExists()
    {
        Directory.CreateDirectory(_peopleDirectoryPath);
    }

    private void RegisterWarning(string warningMessage)
    {
        _notificationStore.AddWarning(warningMessage);
    }

    private PersonStorageException BuildStorageException(string userMessage, Exception exception)
    {
        _logger.LogError(exception, userMessage);
        return new PersonStorageException(userMessage, exception);
    }
}
