using System.Threading.Tasks;
using SpotifyAPI.Web.Enums;
using SpotifyAPI.Web.Models;

namespace Orneholm.CognitiveJukebox.Web.Services
{
    public class SpotifySearcher : ISpotifySearcher
    {
        private readonly SpotifyAuthenticatedWebApi _spotifyAuthenticatedWebApi;

        public SpotifySearcher(SpotifyAuthenticatedWebApi spotifyAuthenticatedWebApi)
        {
            _spotifyAuthenticatedWebApi = spotifyAuthenticatedWebApi;
        }

        public async Task<SearchItem> SearchTopTracksAsync(string q)
        {
            var api = await _spotifyAuthenticatedWebApi.GetApi();
            return api.SearchItems(q, SearchType.Track, 50);
        }
    }
}
