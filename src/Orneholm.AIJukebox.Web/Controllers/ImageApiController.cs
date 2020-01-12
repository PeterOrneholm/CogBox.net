using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Orneholm.AIJukebox.Web.Models;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Enums;
using SpotifyAPI.Web.Models;

namespace Orneholm.AIJukebox.Web.Controllers
{
    [ApiController]
    [Route("api/image/")]
    public class ImageApiController : Controller
    {
        private static readonly List<VisualFeatureTypes> VisualFeatures = new List<VisualFeatureTypes>()
        {
            VisualFeatureTypes.Tags,
            VisualFeatureTypes.Description,
            VisualFeatureTypes.Objects
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

            var tags = GetTags(analyzeImageResult);
            var tag = tags.First();

            var tracks = await GetTopRatedTracks(tag);
            var track = tracks.FirstOrDefault();

            var viewModel = new ImageAnalyzeResult
            {
                ImageDescription = MakeSentence(caption),
                ImageTags = tags,

                ArtistName = track?.Artists.FirstOrDefault()?.Name ?? string.Empty,

                AlbumName = track?.Album.Name ?? string.Empty,
                AlbumCoverUrl = track?.Album.Images.FirstOrDefault()?.Url ?? string.Empty,
                AlbumReleaseYear = ParseAlbumReleaseYear(track?.Album.ReleaseDate),

                TrackAudioPreviewUrl = track?.PreviewUrl ?? string.Empty,
                TrackName = track?.Name ?? string.Empty,
                TrackSpotifyUrl = track?.ExternUrls["spotify"] ?? string.Empty
            };

            return viewModel;
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

        private async Task<List<FullTrack>> GetTopRatedTracks(string tags)
        {
            var trackSearchResult = await _spotifyWebApi.SearchItemsAsync(tags, SearchType.Track, limit: 50);
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

        private static List<string> GetTags(ImageAnalysis analyzeImageResult)
        {
            var tags = new List<string>();

            var tagsWithConfidence = analyzeImageResult.Tags.Where(x => x.Confidence > 0.9)
                .OrderByDescending(x => x.Confidence)
                .SelectMany(x => new[] {x.Name, x.Hint});
            tags.AddRange(tagsWithConfidence);

            var objectsWithConfidence = analyzeImageResult.Objects
                .Where(x => x.Confidence > 0.9)
                .Select(x => x.ObjectProperty);
            tags.AddRange(objectsWithConfidence);

            return tags.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
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
