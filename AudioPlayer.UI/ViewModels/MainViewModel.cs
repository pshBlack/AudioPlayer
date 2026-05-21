using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using AudioPlayer.Core.Models;
using AudioPlayer.Core.Repositories;
using AudioPlayer.Core.Services;

namespace AudioPlayer.UI.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private readonly PlaylistService _playlistService;
    private readonly MetadataService _metadataService;
    private readonly PlayerService _playerService;
    private Track? _activeTrack;
    private List<Track> _activeTracks = [];

    public ObservableCollection<string> Playlists { get; } = [];
    public ObservableCollection<Track> Tracks { get; } = [];

    private string _selectedPlaylist = string.Empty;
    public string SelectedPlaylist
    {
        get => _selectedPlaylist;
        set
        {
            _selectedPlaylist = value;
            OnPropertyChanged();
            LoadTracks();
        }
    }

    private Track? _selectedTrack;
    public Track? SelectedTrack
    {
        get => _selectedTrack;
        set
        {
            if (_selectedTrack == value) return;
            _selectedTrack = value;
            OnPropertyChanged();
            if (_selectedTrack is not null)
            {
                _activeTrack = _selectedTrack;
                _activeTracks = Tracks.ToList();
                _playerService.Play(_selectedTrack);
                CurrentTrackInfo = _selectedTrack.ToString();
            }
        }
    }

    private string _currentTrackInfo = "No active track";
    public string CurrentTrackInfo
    {
        get => _currentTrackInfo;
        set { _currentTrackInfo = value; OnPropertyChanged(); }
    }

    private string _newPlaylistName = string.Empty;
    public string NewPlaylistName
    {
        get => _newPlaylistName;
        set { _newPlaylistName = value; OnPropertyChanged(); }
    }

    private string _searchQuery = string.Empty;
    public string SearchQuery
    {
        get => _searchQuery;
        set { _searchQuery = value; OnPropertyChanged(); FilterTracks(); }
    }

    private float _volume = 0.5f;
    public float Volume
    {
        get => _volume;
        set { _volume = value; OnPropertyChanged(); _playerService.Volume = value; }
    }

    public ICommand CreatePlaylistCommand { get; }
    public ICommand DeletePlaylistCommand { get; }
    public ICommand PlayPauseCommand { get; }
    public ICommand NextCommand { get; }
    public ICommand PreviousCommand { get; }
    public ICommand ShuffleCommand { get; }
    public ICommand AddFileCommand { get; }
    public ICommand AddFolderCommand { get; }

    public MainViewModel()
    {
        var storageDir = System.IO.Path.Combine(AppContext.BaseDirectory, "playlists");
        var repository = new PlaylistRepository(storageDir);
        _metadataService = new MetadataService();
        _playlistService = new PlaylistService(repository);
        _playerService = new PlayerService();
        _playerService.Volume = _volume;
        _playerService.TrackFinished += (_, _) =>
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(() => Next());
        };

        CreatePlaylistCommand = new RelayCommand(CreatePlaylist);
        DeletePlaylistCommand = new RelayCommand(DeletePlaylist);
        PlayPauseCommand = new RelayCommand(PlayPause);
        NextCommand = new RelayCommand(Next);
        PreviousCommand = new RelayCommand(Previous);
        ShuffleCommand = new RelayCommand(Shuffle);
        AddFileCommand = new AsyncRelayCommand(AddFile);
        AddFolderCommand = new AsyncRelayCommand(AddFolder);

        LoadPlaylists();
        var timer = new System.Timers.Timer(500);
        timer.Elapsed += (_, _) =>
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                if (!_playerService.IsPlaying) return;

                var duration = _activeTrack?.Duration.TotalSeconds ?? 0;
                if (duration <= 0) return;

                var pos = _playerService.Position.TotalSeconds;
                if (pos > duration) return;

                TrackDuration = duration;
                _position = pos;
                OnPropertyChanged(nameof(Position));
                OnPropertyChanged(nameof(TrackDuration));
            });
        };
        timer.Start();
    }

    private void CreatePlaylist()
    {
        if (string.IsNullOrWhiteSpace(NewPlaylistName)) return;
        _playlistService.Create(NewPlaylistName);
        NewPlaylistName = string.Empty;
        LoadPlaylists();
    }

    private void DeletePlaylist()
    {
        if (string.IsNullOrWhiteSpace(SelectedPlaylist)) return;
        _playlistService.Delete(SelectedPlaylist);
        Tracks.Clear();
        LoadPlaylists();
    }

    private void PlayPause()
    {
        if (_playerService.IsPlaying)
        {
            _playerService.Pause();
            return;
        }

        _playerService.Resume();
    }


    private void Next()
    {
        if (_activeTrack is null || _activeTracks.Count == 0) return;
        var index = _activeTracks.IndexOf(_activeTrack);
        if (index == -1) index = 0;
        var next = index < _activeTracks.Count - 1 ? _activeTracks[index + 1] : _activeTracks[0];
        _activeTrack = next;
        _selectedTrack = next;
        OnPropertyChanged(nameof(SelectedTrack));
        _playerService.Play(next);
        CurrentTrackInfo = next.ToString();
    }

    private void Previous()
    {
        if (_activeTrack is null || _activeTracks.Count == 0) return;
        var index = _activeTracks.IndexOf(_activeTrack);
        if (index == -1) index = 0;
        var prev = index > 0 ? _activeTracks[index - 1] : _activeTracks[^1];
        _activeTrack = prev;
        _selectedTrack = prev;
        OnPropertyChanged(nameof(SelectedTrack));
        _playerService.Play(prev);
        CurrentTrackInfo = prev.ToString();
    }

    private void Shuffle()
    {
        if (string.IsNullOrWhiteSpace(SelectedPlaylist)) return;
        _playlistService.Shuffle(SelectedPlaylist);
        LoadTracks();
    }

    private async Task AddFile()
    {
        if (string.IsNullOrWhiteSpace(SelectedPlaylist)) return;

        var window = Avalonia.Application.Current?.ApplicationLifetime is
            Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
            ? desktop.MainWindow : null;

        if (window is null) return;

        var files = await window.StorageProvider.OpenFilePickerAsync(
            new Avalonia.Platform.Storage.FilePickerOpenOptions
            {
                AllowMultiple = true,
                FileTypeFilter = [new Avalonia.Platform.Storage.FilePickerFileType("Audio")
                {
                    Patterns = ["*.mp3", "*.flac", "*.ogg", "*.wav", "*.m4a"]
                }]
            });

        foreach (var file in files)
        {
            var track = _metadataService.CreateTrack(file.Path.LocalPath);
            _playlistService.AddTrack(SelectedPlaylist, track);
        }

        LoadTracks();
    }

    private async Task AddFolder()
    {
        if (string.IsNullOrWhiteSpace(SelectedPlaylist)) return;

        var window = Avalonia.Application.Current?.ApplicationLifetime is
            Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
            ? desktop.MainWindow : null;

        if (window is null) return;

        var folders = await window.StorageProvider.OpenFolderPickerAsync(
            new Avalonia.Platform.Storage.FolderPickerOpenOptions
            {
                AllowMultiple = false
            });

        if (folders.Count == 0) return;

        var folder = folders[0].Path.LocalPath;
        var tracks = _metadataService.CreateTracksFromDirectory(folder).ToList();
        foreach (var track in tracks)
            _playlistService.AddTrack(SelectedPlaylist, track);

        LoadTracks();
    }

    private void LoadPlaylists()
    {
        Playlists.Clear();
        foreach (var name in _playlistService.GetAllPlaylistNames())
            Playlists.Add(name);
    }

    private double _position;
    public double Position
    {
        get => _position;
        set
        {
            _position = value;
            OnPropertyChanged();
            _playerService.Position = TimeSpan.FromSeconds(value);
        }
    }

public double TrackDuration { get; private set; }

    private void LoadTracks()
    {
        Tracks.Clear();
        if (string.IsNullOrWhiteSpace(SelectedPlaylist)) return;

        var playlist = _playlistService.Get(SelectedPlaylist);
        if (playlist is null) return;

        foreach (var track in playlist.Tracks)
            Tracks.Add(track);
    }

    private void FilterTracks()
    {
        Tracks.Clear();
        if (string.IsNullOrWhiteSpace(SelectedPlaylist)) return;

        var results = string.IsNullOrWhiteSpace(SearchQuery)
            ? _playlistService.Get(SelectedPlaylist)?.Tracks ?? []
            : _playlistService.Search(SelectedPlaylist, SearchQuery).ToList();

        foreach (var track in results)
            Tracks.Add(track);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}