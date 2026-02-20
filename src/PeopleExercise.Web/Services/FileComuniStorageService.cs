using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using PeopleExercise.Web.Configuration;
using PeopleExercise.Web.Models;



namespace PeopleExercise.Web.Services
{
    public sealed class FileComuniStorageService : IComuniStorageService
    {

        private static readonly JsonSerializerOptions JsonSerializerOptions = new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        private readonly string _peopleDirectoryPath;
        private readonly ILogger<FilePersonStorageService> _logger;
        private readonly IUserNotificationStore _notificationStore;

        public FileComuniStorageService(
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
    }
}
