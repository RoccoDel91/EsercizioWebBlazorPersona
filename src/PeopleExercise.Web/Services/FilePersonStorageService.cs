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
        _logger = logger;
        _notificationStore = notificationStore;
        _peopleDirectoryPath = Path.IsPathRooted(storageOptions.Value.PeopleDirectoryPath)
            ? storageOptions.Value.PeopleDirectoryPath
            : Path.GetFullPath(Path.Combine(environment.ContentRootPath, storageOptions.Value.PeopleDirectoryPath));
    }

    public async Task<IReadOnlyList<Persona>> GetAllAsync()
    {
        EnsureDirectoryExists();
        var people = new List<Persona>();

        foreach (var filePath in Directory.EnumerateFiles(_peopleDirectoryPath, "*.json"))
        {
            try
            {
                var jsonContent = await File.ReadAllTextAsync(filePath, Encoding.UTF8);
                var person = JsonSerializer.Deserialize<Persona>(jsonContent, JsonSerializerOptions);
                if (person is null) continue;

                if (string.IsNullOrWhiteSpace(person.Id))
                    person.Id = Path.GetFileNameWithoutExtension(filePath);

                people.Add(person);
            }
            catch (JsonException) { RegisterWarning($"File {Path.GetFileName(filePath)} corrotto."); }
        }
        return people;
    }

    public async Task<Persona?> GetByIdAsync(string id)
    {
        var filePath = BuildFilePath(id);
        if (!File.Exists(filePath)) return null;
        var jsonContent = await File.ReadAllTextAsync(filePath, Encoding.UTF8);
        return JsonSerializer.Deserialize<Persona>(jsonContent, JsonSerializerOptions);
    }

    public async Task SaveAsync(Persona person)
    {
        // Forza l'ID come Codice Fiscale
        if (!string.IsNullOrWhiteSpace(person.codiceFiscale))
            person.Id = person.codiceFiscale;

        if (string.IsNullOrWhiteSpace(person.Id))
            throw new InvalidOperationException("ID mancante.");

        EnsureDirectoryExists();
        var filePath = BuildFilePath(person.Id);
        var serialized = JsonSerializer.Serialize(person, JsonSerializerOptions);
        await File.WriteAllTextAsync(filePath, serialized, Encoding.UTF8);
    }

    public Task DeleteAsync(string id)
    {
        var filePath = BuildFilePath(id);
        if (File.Exists(filePath)) File.Delete(filePath);
        return Task.CompletedTask;
    }

    public Task OpenFilePath(string id)
    {
        var filePath = BuildFilePath(id);
        Process.Start(new ProcessStartInfo { FileName = filePath, UseShellExecute = true });
        return Task.CompletedTask;
    }

    private string BuildFilePath(string id) => Path.Combine(_peopleDirectoryPath, $"{id}.json");
    private void EnsureDirectoryExists() => Directory.CreateDirectory(_peopleDirectoryPath);
    private void RegisterWarning(string msg) => _notificationStore.AddWarning(msg);
    private PersonStorageException BuildStorageException(string msg, Exception ex) => new(msg, ex);
}