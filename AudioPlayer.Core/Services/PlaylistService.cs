using AudioPlayer.Core.Enums;
using AudioPlayer.Core.Models;
using AudioPlayer.Core.Repositories;

namespace AudioPlayer.Core.Services;

public class PlaylistService
{
    private readonly IPlayListRepository _repository;

    public PlaylistService(IPlayListRepository repository)
    {
        _repository = repository;
    }

    public Playlist Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Playlist name cannot be empty.", nameof(name));

        var playlist = new Playlist { Name = name };
        _repository.Save(playlist);
        return playlist;
    }

    public void AddTrack(string playlistName, Track track)
    {
        var playlist = GetOrThrow(playlistName);
        playlist.Tracks.Add(track);
        _repository.Save(playlist);
    }

    public void RemoveTrack(string playlistName, string filePath)
    {
        var playlist = GetOrThrow(playlistName);
        playlist.Tracks.RemoveAll(t => t.FilePath == filePath);
        _repository.Save(playlist);
    }

    public void Shuffle(string playlistName)
    {
        var playlist = GetOrThrow(playlistName);
        playlist.Tracks = playlist.Tracks.OrderBy(_ => Random.Shared.Next()).ToList();
        playlist.IsShuffled = true;
        _repository.Save(playlist);
    }

    public void SetRepeatMode(string playlistName, RepeatMode mode)
    {
        var playlist = GetOrThrow(playlistName);
        playlist.RepeatMode = mode;
        _repository.Save(playlist);
    }

    public Playlist? Get(string name) => _repository.Load(name);

    public IEnumerable<string> GetAllPlaylistNames() => _repository.GetAllPlaylistNames();

    public void Delete(string name) => _repository.Delete(name);

    public IEnumerable<Track> Search(string playlistName, string query)
    {
        var playlist = GetOrThrow(playlistName);
        var q = query.ToLower();

        return playlist.Tracks.Where(t =>
            t.Metadata.Title.ToLower().Contains(q) ||
            t.Metadata.Artist.ToLower().Contains(q) ||
            t.Metadata.Album.ToLower().Contains(q));
    }

    private Playlist GetOrThrow(string name) =>
        _repository.Load(name) ?? throw new InvalidOperationException($"Playlist '{name}' not found.");
}