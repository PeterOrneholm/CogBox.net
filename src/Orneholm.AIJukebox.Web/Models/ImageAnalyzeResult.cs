using System.Collections.Generic;

namespace Orneholm.AIJukebox.Web.Models
{
    public class ImageAnalyzeResult
    {
        public string ImageDescription { get; set; } = string.Empty;

        public List<MusicTrack> MusicTracks { get; set; } = new List<MusicTrack>();
    }
}
