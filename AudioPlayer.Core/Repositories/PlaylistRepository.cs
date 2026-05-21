using System.Text.Json;
using AudioPlayer.Core.Models;

namespace AudioPlayer.Core.Repositories;

public class PlaylistRepository : IPlayListRepository
{
    private readonly string _storageDir;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public PlaylistRepository(string storageDir = "playlists")
    {
        _storageDir = storageDir;
        Directory.CreateDirectory(_storageDir);
    }

    public void Save(Playlist playlist)
    {
        var path = GetFilePath(playlist.Name);
        var json = JsonSerializer.Serialize(playlist, JsonOptions);
        File.WriteAllText(path, json);
    }

    public Playlist? Load(string name)
    {
        var path = GetFilePath(name);
        if (!File.Exists(path))
            return null;

        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<Playlist>(json);
    }

    public void Delete(string name)
    {
        var path = GetFilePath(name);
        if (File.Exists(path))
            File.Delete(path);
    }

    public IEnumerable<string> GetAllPlaylistNames()
    {
        return Directory
            .EnumerateFiles(_storageDir, "*.json")
            .Select(Path.GetFileNameWithoutExtension)
            .Where(n => n is not null)
            .Cast<string>();
    }

    private string GetFilePath(string name) =>
        Path.Combine(_storageDir, $"{name}.json");
}