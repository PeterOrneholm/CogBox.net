namespace Orneholm.CognitiveJukebox.Web.Models
{
    public class MusicTrack
    {
        public string AlbumName { get; set; } = string.Empty;
        public int? AlbumReleaseYear { get; set; } = null;
        public string AlbumCoverUrl { get; set; } = string.Empty;

        public string ArtistName { get; set; } = string.Empty;

        public string TrackName { get; set; } = string.Empty;
        public string TrackAudioPreviewUrl { get; set; } = string.Empty;
        public string TrackSpotifyUrl { get; set; } = string.Empty;
    }
}
