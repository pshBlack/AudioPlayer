using AudioPlayer.Core.Enums;
using AudioPlayer.Core.Models;
using LibVLCSharp.Shared;

namespace AudioPlayer.Core.Services;

public class PlayerService : IDisposable
{
    private readonly LibVLC _libVlc;
    private readonly MediaPlayer _mediaPlayer;
    private bool _disposed;

    public float Volume
    {
        get => _mediaPlayer.Volume / 100f;
        set => _mediaPlayer.Volume = (int)Math.Clamp(value * 100, 0, 200);
    }

    public bool IsPlaying => _mediaPlayer.IsPlaying;

    public TimeSpan Position
    {
        get => TimeSpan.FromMilliseconds(_mediaPlayer.Time);
        set => _mediaPlayer.Time = (long)value.TotalMilliseconds;
    }

    public event EventHandler? TrackFinished;

    public PlayerService()
    {
        LibVLCSharp.Shared.Core.Initialize();
        _libVlc = new LibVLC();
        _mediaPlayer = new MediaPlayer(_libVlc);
        _mediaPlayer.EndReached += (_, _) => TrackFinished?.Invoke(this, EventArgs.Empty);
    }

    public void Play(Track track)
    {
        if (!System.IO.File.Exists(track.FilePath))
            throw new FileNotFoundException($"File not found: {track.FilePath}");

        var media = new Media(_libVlc, track.FilePath, FromType.FromPath);
        _mediaPlayer.Media = media;
        _mediaPlayer.Play();
        media.Dispose();
    }

    public void Pause()
    {
        if (_mediaPlayer.CanPause)
            _mediaPlayer.Pause();
    }

    public void Resume()
    {
        if (!_mediaPlayer.IsPlaying)
            _mediaPlayer.Play();
    }

    public void Stop()
    {
        _mediaPlayer.Stop();
    }

    public void Dispose()
    {
        if (_disposed) return;
        _mediaPlayer.Stop();
        _mediaPlayer.Dispose();
        _libVlc.Dispose();
        _disposed = true;
    }
}