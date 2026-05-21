using AudioPlayer.Core.Models;

namespace AudioPlayer.Core.Services;

public class MetadataService
{
    public TrackMetadata Read(string filePath)
    {
        if(!System.IO.File.Exists(filePath))
            throw new FileNotFoundException("File not found :( :", filePath);

        using var file = TagLib.File.Create(filePath); // use using to ensure proper disposal of resources
        return new TrackMetadata
        {
            Title = file.Tag.Title ?? Path.GetFileNameWithoutExtension(filePath),
            Artist = file.Tag.FirstPerformer ?? "Unknown Artist",
            Album = file.Tag.Album ?? "Unknown Album",
            Year = file.Tag.Year,
            Genre = file.Tag.FirstGenre ?? string.Empty
        };
    }

    public TimeSpan ReadDuration(string filePath)
    {
        if(!System.IO.File.Exists(filePath))
            throw new FileNotFoundException("File not found :( :", filePath);

        using var file = TagLib.File.Create(filePath);
        return file.Properties.Duration;
    }

    public Track CreateTrack(string filePath)
    {
        if(!System.IO.File.Exists(filePath))
            throw new FileNotFoundException("File not found :( :", filePath);
        
        using var file = TagLib.File.Create(filePath);

        return new Track
        {
            FilePath = filePath,
            Duration = file.Properties.Duration,
            Metadata = new TrackMetadata
            {
                Title = file.Tag.Title ?? Path.GetFileNameWithoutExtension(filePath),
                Artist = file.Tag.FirstPerformer ?? "Unknown Artist",
                Album = file.Tag.Album ?? "Unknown Album",
                Year = file.Tag.Year,
                Genre = file.Tag.FirstGenre ?? string.Empty
            },
        };
    }

    public IEnumerable<Track> CreateTracksFromDirectory(string dirPath)
    {
        if(!System.IO.Directory.Exists(dirPath))
           throw new DirectoryNotFoundException("Directory not found :( :  " + dirPath);

        string[] supportedExtensions = { ".mp3", ".flac", ".wav", ".aac", ".ogg" };

        return Directory
            .EnumerateFiles(dirPath, "*", SearchOption.AllDirectories)
            .Where(f => supportedExtensions.Contains(Path.GetExtension(f).ToLower()))
            .Select(CreateTrack);
    }
}
