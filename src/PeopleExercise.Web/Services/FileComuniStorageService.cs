using Microsoft.Extensions.Options;
using PeopleExercise.Web.Configuration;
using System.Text;
using System.Text.Json;

namespace PeopleExercise.Web.Services
{

    public sealed class FileComuniStorageService : IComuniStorageService
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public readonly string _comuniFilePath;

        public FileComuniStorageService(IOptions<StorageOptions> options, IWebHostEnvironment env)
        {
            var configuredPath = options.Value.ComuniDirectoryPath;
            _comuniFilePath = Path.IsPathRooted(configuredPath)
                ? configuredPath
                : Path.GetFullPath(Path.Combine(env.ContentRootPath, configuredPath));
        }

        public async Task<IReadOnlyDictionary<string, string>> GetComuniAsync()
        {
            if (!File.Exists(_comuniFilePath))
                return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            var json = await File.ReadAllTextAsync(_comuniFilePath, Encoding.UTF8);
            var comuni = JsonSerializer.Deserialize<List<ComuneJson>>(json, JsonOptions) ?? new List<ComuneJson>();

            return comuni
                .Where(c => !string.IsNullOrWhiteSpace(c.nome) && !string.IsNullOrWhiteSpace(c.codiceCatastale))
                .GroupBy(c => c.nome.Trim(), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    g => g.Key,
                    g => g.First().codiceCatastale.Trim(),
                    StringComparer.OrdinalIgnoreCase);
             
        }

        public async Task<IReadOnlyList<string>> GetNomiComuniAsync()
        {
            var map = await GetComuniAsync();
            return map.Keys.OrderBy(k => k).ToList();
        }

        private sealed class ComuneJson
        {
            public string nome { get; set; } = string.Empty;
            public string codiceCatastale { get; set; } = string.Empty;
        }
    }

}

