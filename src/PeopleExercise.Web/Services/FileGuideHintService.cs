using System.Text;
using Microsoft.Extensions.Options;
using PeopleExercise.Web.Configuration;

namespace PeopleExercise.Web.Services;

public sealed class FileGuideHintService : IGuideHintService
{
    private readonly string _guidePath;

    public FileGuideHintService(IOptions<StorageOptions> options, IWebHostEnvironment env)
    {
        var configured = options.Value.GuideFilePath;
        _guidePath = Path.IsPathRooted(configured)
            ? configured
            : Path.GetFullPath(Path.Combine(env.ContentRootPath, configured));
    }

    public async Task<string> GetHintByHookAsync(string hook)
    {
        if (string.IsNullOrWhiteSpace(hook))
            return "Hook non valido.";

        if (!File.Exists(_guidePath))
            return $"Guida non trovata: {_guidePath}";

        var content = await File.ReadAllTextAsync(_guidePath, Encoding.UTF8);

        var startMarker = $"<!-- HOOK:{hook}:START -->";
        var endMarker = $"<!-- HOOK:{hook}:END -->";

        var start = content.IndexOf(startMarker, StringComparison.OrdinalIgnoreCase);
        if (start < 0) return $"Nessun suggerimento trovato per hook '{hook}'.";

        start += startMarker.Length;
        var end = content.IndexOf(endMarker, start, StringComparison.OrdinalIgnoreCase);
        if (end < 0) return $"Hook '{hook}' incompleto (manca END).";

        return content[start..end].Trim();
    }
}
