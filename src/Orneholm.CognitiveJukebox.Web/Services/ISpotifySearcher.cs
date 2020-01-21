using System.Threading.Tasks;
using SpotifyAPI.Web.Models;

namespace Orneholm.CognitiveJukebox.Web.Services
{
    public interface ISpotifySearcher
    {
        Task<SearchItem> SearchTopTracksAsync(string q);
    }
}