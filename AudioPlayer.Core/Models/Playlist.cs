using AudioPlayer.Core.Enums;

namespace AudioPlayer.Core.Models
{
    public class Playlist
    {
        public string Name { get; set; } = string.Empty;
        public List<Track> Tracks { get; set; } = [];
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public RepeatMode RepeatMode { get; set; } = RepeatMode.Off;
        public bool IsShuffled { get; set; } = false;
    }
}