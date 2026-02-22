using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using PeopleExercise.Web.Configuration;
using PeopleExercise.Web.Models;

namespace PeopleExercise.Web.Services;

public sealed class FileTodoStorageService : ITodoStorageService
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    private readonly string _todoFilePath;
    private readonly ILogger<FileTodoStorageService> _logger;

    public FileTodoStorageService(
        IOptions<StorageOptions> storageOptions,
        IWebHostEnvironment environment,
        ILogger<FileTodoStorageService> logger)
    {
        ArgumentNullException.ThrowIfNull(storageOptions);
        ArgumentNullException.ThrowIfNull(environment);

        _logger = logger;

        if (string.IsNullOrWhiteSpace(storageOptions.Value.TodoFilePath))
        {
            throw new InvalidOperationException("Storage:TodoFilePath non configurato.");
        }

        _todoFilePath = Path.IsPathRooted(storageOptions.Value.TodoFilePath)
            ? storageOptions.Value.TodoFilePath
            : Path.GetFullPath(Path.Combine(environment.ContentRootPath, storageOptions.Value.TodoFilePath));
    }

    public async Task<IReadOnlyList<TodoTaskItem>> GetTasksAsync()
    {
        try
        {
            return await ReadAllInternalAsync();
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or IOException or JsonException)
        {
            _logger.LogError(ex, "Errore lettura TODO da {FilePath}", _todoFilePath);
            throw;
        }
    }

    public async Task UpdateTaskAsync(TodoTaskItem task)
    {
        ArgumentNullException.ThrowIfNull(task);

        try
        {
            var tasks = await ReadAllInternalAsync();
            var index = tasks.FindIndex(t => t.Id == task.Id);

            if (index < 0)
            {
                throw new InvalidOperationException($"Task con Id '{task.Id}' non trovato.");
            }

            task.TempoDiLavoro ??= new TempoDiLavoro();
            task.TempoDiLavoro.Sessioni ??= [];
            tasks[index] = task;
            await WriteAllInternalAsync(tasks);
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or IOException or JsonException or InvalidOperationException)
        {
            _logger.LogError(ex, "Errore aggiornamento task {TaskId}", task.Id);
            throw;
        }
    }

    public async Task AddNewTaskAsync(TodoTaskItem task)
    {
        ArgumentNullException.ThrowIfNull(task);

        try
        {
            var tasks = await ReadAllInternalAsync();

            if (task.Id == Guid.Empty)
            {
                task.Id = Guid.NewGuid();
            }

            task.TempoDiLavoro ??= new TempoDiLavoro();
            task.TempoDiLavoro.Sessioni ??= [];
            if (task.OnGoing && task.TempoDiLavoro.Sessioni.All(session => session.TimeStop is not null))
            {
                task.TempoDiLavoro.Sessioni.Add(new IntervalloLavoro
                {
                    TimeStart = DateTimeOffset.UtcNow
                });
            }
            tasks.Add(task);
            await WriteAllInternalAsync(tasks);
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or IOException or JsonException)
        {
            _logger.LogError(ex, "Errore aggiunta nuovo task.");
            throw;
        }
    }

    private void EnsureDirectoryExists()
    {
        var directoryPath = Path.GetDirectoryName(_todoFilePath)
            ?? throw new InvalidOperationException("Percorso Todo non valido.");
        Directory.CreateDirectory(directoryPath);
    }

    private async Task<List<TodoTaskItem>> ReadAllInternalAsync()
    {
        EnsureDirectoryExists();

        if (!File.Exists(_todoFilePath))
        {
            await File.WriteAllTextAsync(_todoFilePath, "[]", Encoding.UTF8);
            return [];
        }

        var json = await File.ReadAllTextAsync(_todoFilePath, Encoding.UTF8);
        var tasks = JsonSerializer.Deserialize<List<TodoTaskItem>>(json, JsonSerializerOptions) ?? [];

        foreach (var task in tasks)
        {
            task.TempoDiLavoro ??= new TempoDiLavoro();
            task.TempoDiLavoro.Sessioni ??= [];
        }

        return tasks;
    }

    private async Task WriteAllInternalAsync(List<TodoTaskItem> tasks)
    {
        var json = JsonSerializer.Serialize(tasks, JsonSerializerOptions);
        await File.WriteAllTextAsync(_todoFilePath, json, Encoding.UTF8);
    }
}
