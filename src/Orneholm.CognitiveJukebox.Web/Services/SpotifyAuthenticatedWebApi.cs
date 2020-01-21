using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;

namespace Orneholm.CognitiveJukebox.Web.Services
{
    public class SpotifyAuthenticatedWebApi
    {
        private const string CacheEntryKey = "SpotifyWebAPI";
        private readonly TimeSpan _cacheReset = TimeSpan.FromMinutes(5);

        private readonly CredentialsAuth _spotifyAuth;
        private readonly IMemoryCache _memoryCache;

        public SpotifyAuthenticatedWebApi(CredentialsAuth spotifyAuth, IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
            _spotifyAuth = spotifyAuth;
        }

        public async Task<SpotifyWebAPI> GetApi()
        {
            if (!_memoryCache.TryGetValue<SpotifyWebAPI>(CacheEntryKey, out var spotifyWebApi))
            {
                var token = await _spotifyAuth.GetToken();
                spotifyWebApi = new SpotifyWebAPI
                {
                    AccessToken = token.AccessToken,
                    TokenType = token.TokenType
                };

                var expiresAt = token.CreateDate.Add(TimeSpan.FromSeconds(token.ExpiresIn)).Subtract(_cacheReset);
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(expiresAt);

                _memoryCache.Set(CacheEntryKey, spotifyWebApi, cacheEntryOptions);
            }

            return spotifyWebApi;
        }
    }
}