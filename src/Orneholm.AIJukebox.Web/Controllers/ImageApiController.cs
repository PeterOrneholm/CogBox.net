using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Orneholm.AIJukebox.Web.Models;

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

        public ImageApiController(TelemetryClient telemetryClient, IComputerVisionClient computerVisionClient)
        {
            _telemetryClient = telemetryClient;
            _computerVisionClient = computerVisionClient;
        }

        [HttpPost("analyze")]
        public async Task<ActionResult<ImageAnalyzeResult>> Index(IFormFile file)
        {
            var analyzeImageResult = await _computerVisionClient.AnalyzeImageInStreamAsync(file.OpenReadStream(), VisualFeatures);

            var firstCaption = analyzeImageResult.Description.Captions.FirstOrDefault();

            var tags = new List<string>();

            var tagsWithConfidence = analyzeImageResult.Tags.Where(x => x.Confidence > 0.9)
                                                            .OrderByDescending(x => x.Confidence)
                                                            .SelectMany(x => new[] { x.Name, x.Hint });
            tags.AddRange(tagsWithConfidence);

            var objectsWithConfidence = analyzeImageResult.Objects
                                                            .Where(x => x.Confidence > 0.9)
                                                            .Select(x => x.ObjectProperty);
            tags.AddRange(objectsWithConfidence);

            var viewModel = new ImageAnalyzeResult
            {
                Description = firstCaption?.Text ?? string.Empty,
                RelevantTags = tags.Where(x => !string.IsNullOrWhiteSpace(x)).ToList()
            };

            return viewModel;
        }
    }
}
