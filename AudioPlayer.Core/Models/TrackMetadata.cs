namespace AudioPlayer.Core.Models
{
    public class TrackMetadata
    {
        public string Title { get; set; } = string.Empty;
        public string Artist { get; set; } = string.Empty;
        public string Album { get; set; } = string.Empty;
        public uint Year { get; set; }
        public string Genre { get; set; } = string.Empty;
    }
}