using System.Collections.Generic;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

namespace Orneholm.CognitiveJukebox.Web.Models
{
    public class ImageAnalyzeResult
    {
        public string ImageDescription { get; set; } = string.Empty;
        public List<FaceDescription> Faces { get; set; } = new List<FaceDescription>();

        public int MusicYear { get; set; }
        public List<MusicTrack> MusicTracks { get; set; } = new List<MusicTrack>();
    }
}
