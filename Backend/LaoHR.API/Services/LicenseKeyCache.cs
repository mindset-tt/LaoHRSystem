using Microsoft.Extensions.Caching.Memory;

namespace LaoHR.API.Services;

public interface ILicenseKeyCache
{
    /// <summary>
    /// Returns the cached license verification result, or null if not cached.
    /// Resolves the value through the supplied loader and caches it for 5
    /// minutes (positive results only — failures short-circuit and re-check
    /// on the next call so a transient DB outage doesn't lock the app out).
    /// </summary>
    Task<LicenseCacheEntry?> GetOrLoadAsync(Func<Task<LicenseCacheEntry?>> loader, CancellationToken ct = default);
}

public sealed record LicenseCacheEntry(bool IsValid, string Message);

public sealed class LicenseKeyCache : ILicenseKeyCache
{
    private readonly IMemoryCache _cache;
    private static readonly TimeSpan PositiveTtl = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan NegativeTtl = TimeSpan.FromSeconds(15);

    public LicenseKeyCache(IMemoryCache cache)
    {
        _cache = cache;
    }

    public async Task<LicenseCacheEntry?> GetOrLoadAsync(Func<Task<LicenseCacheEntry?>> loader, CancellationToken ct = default)
    {
        if (_cache.TryGetValue<LicenseCacheEntry>(CacheKey, out var hit) && hit is not null)
        {
            return hit;
        }

        var loaded = await loader().WaitAsync(ct);
        if (loaded is null) return null;

        _cache.Set(CacheKey, loaded, loaded.IsValid ? PositiveTtl : NegativeTtl);
        return loaded;
    }

    private const string CacheKey = "license:current";
}
