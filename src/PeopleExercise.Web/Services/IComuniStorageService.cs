public interface IComuniStorageService
{
    Task<IReadOnlyDictionary<string, string>> GetComuniAsync();
    Task<IReadOnlyList<string>> GetNomiComuniAsync();
}