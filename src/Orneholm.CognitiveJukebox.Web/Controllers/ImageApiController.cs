using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Orneholm.CognitiveJukebox.Web.Models;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Enums;
using SpotifyAPI.Web.Models;

namespace Orneholm.CognitiveJukebox.Web.Controllers
{
    [ApiController]
    [Route("api/image/")]
    public class ImageApiController : Controller
    {
        private static readonly List<VisualFeatureTypes> VisualFeatures = new List<VisualFeatureTypes>()
        {
            VisualFeatureTypes.Description,
            VisualFeatureTypes.Faces
        };

        private readonly TelemetryClient _telemetryClient;
        private readonly IComputerVisionClient _computerVisionClient;
        private readonly SpotifyWebAPI _spotifyWebApi;


        public ImageApiController(TelemetryClient telemetryClient, IComputerVisionClient computerVisionClient, SpotifyWebAPI spotifyWebApi)
        {
            _telemetryClient = telemetryClient;
            _computerVisionClient = computerVisionClient;
            _spotifyWebApi = spotifyWebApi;
        }

        [HttpPost("analyze")]
        public async Task<ActionResult<ImageAnalyzeResult>> Index(IFormFile file)
        {
            var analyzeImageResult = await _computerVisionClient.AnalyzeImageInStreamAsync(file.OpenReadStream(), VisualFeatures);

            var caption = GetCaption(analyzeImageResult);

            var faces = analyzeImageResult.Faces.ToList();
            var face = faces.FirstOrDefault();
            var year = GetYearFromFace(face);

            var tracks = await GetTopRatedTracks(year);
            var randomTracks = tracks.Take(10).OrderBy(a => Guid.NewGuid()).ToList();

            var viewModel = new ImageAnalyzeResult
            {
                ImageDescription = MakeSentence(caption),
                Faces = faces,

                MusicYear = year,
                MusicTracks = randomTracks.Select(FullTrackToMusicTrack).Take(5).ToList()
            };

            return viewModel;
        }

        private int GetYearFromFace(FaceDescription? face)
        {
            if (face == null)
            {
                return 1991;
            }

            return DateTime.UtcNow.Year - face.Age;
        }

        private MusicTrack FullTrackToMusicTrack(FullTrack fullTrack)
        {
            return new MusicTrack
            {
                ArtistName = fullTrack.Artists.FirstOrDefault()?.Name ?? string.Empty,

                AlbumName = fullTrack.Album.Name ?? string.Empty,
                AlbumCoverUrl = fullTrack.Album.Images.FirstOrDefault()?.Url ?? string.Empty,
                AlbumReleaseYear = ParseAlbumReleaseYear(fullTrack.Album.ReleaseDate),

                TrackAudioPreviewUrl = fullTrack.PreviewUrl ?? string.Empty,
                TrackName = fullTrack.Name ?? string.Empty,
                TrackSpotifyUrl = fullTrack.ExternUrls["spotify"] ?? string.Empty
            };
        }

        private static int? ParseAlbumReleaseYear(string? releaseDate)
        {
            if (releaseDate == null)
            {
                return null;
            }

            if (DateTime.TryParse(releaseDate, out var date))
            {
                return date.Year;
            }

            return null;
        }

        private async Task<List<FullTrack>> GetTopRatedTracks(int year)
        {
            var trackSearchResult = await _spotifyWebApi.SearchItemsAsync($"year:{year:D}", SearchType.Track, limit: 50);
            return trackSearchResult.Tracks
                .Items
                .Where(x => !x.Explicit)
                .Where(x => !string.IsNullOrEmpty(x.PreviewUrl))
                .Where(x => x.Album.Images.Any())
                .OrderBy(x => x.Popularity)
                .ToList();
        }

        private static string GetCaption(ImageAnalysis analyzeImageResult)
        {
            return analyzeImageResult.Description.Captions.FirstOrDefault()?.Text ?? string.Empty;
        }

        private static string Capitalize(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            return char.ToUpper(value[0]) + value.Substring(1);
        }

        private static string MakeSentence(string? value)
        {
            if (value == null || string.IsNullOrEmpty(value))
            {
                return value ?? string.Empty;
            }

            return Capitalize(value) + ".";
        }
    }
}
