using System.Collections.Generic;

namespace Orneholm.CognitiveJukebox.Web.Models
{
    public class JukeboxOptions
    {
        public string ApiBaseUrl { get; set; } = string.Empty;
        public string ShutterSoundUrl { get; set; } = string.Empty;
        public List<string> IrrelevantTags { get; set; } = new List<string>();
        public List<string> Quotes { get; set; } = new List<string>();
    }
}
