using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using SpotifyAPI.Web.Models;

namespace Orneholm.CognitiveJukebox.Web.Services
{
    public class CachedSpotifySearcher : ISpotifySearcher
    {
        private readonly ISpotifySearcher _implementation;
        private readonly IMemoryCache _memoryCache;

        public CachedSpotifySearcher(ISpotifySearcher implementation, IMemoryCache memoryCache)
        {
            _implementation = implementation;
            _memoryCache = memoryCache;
        }

        public async Task<SearchItem> SearchTopTracksAsync(string q)
        {
            if (!_memoryCache.TryGetValue<SearchItem>(q, out var result))
            {
                result = await _implementation.SearchTopTracksAsync(q);

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromDays(1));

                _memoryCache.Set(q, result, cacheEntryOptions);
            }

            return result;
        }
    }
}