namespace AudioPlayer.Core.Models
{
    public class Track
    {
        public string FilePath { get; set; } = string.Empty;
        public TrackMetadata Metadata { get; set; } = new();
        public TimeSpan Duration { get; set; }

        public override string ToString()
        {
            return $"{Metadata.Title} - {Metadata.Artist} ({Duration:mm\\:ss})";
        }
    }
}