using System.Collections.Generic;

namespace Orneholm.AIJukebox.Web.Models
{
    public class ImageAnalyzeResult
    {
        public string Description { get; set; } = string.Empty;
        public List<string> RelevantTags { get; set; } = new List<string>();
    }
}
