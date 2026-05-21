using System.CommandLine;
using AudioPlayer.Core.Services;

namespace AudioPlayer.CLI.Commands;

public class PlaylistCommands : Command
{
    public PlaylistCommands(
        PlaylistService playlistService,
        MetadataService metadataService) : base("playlist", "Manage playlists")
    {
        Add(BuildCreate(playlistService));
        Add(BuildList(playlistService));
        Add(BuildAdd(playlistService, metadataService));
        Add(BuildRemove(playlistService));
        Add(BuildShow(playlistService));
        Add(BuildDelete(playlistService));
    }

    private static Command BuildCreate(PlaylistService playlistService)
    {
        var nameArg = new Argument<string>("name") { Description = "Playlist name" };
        var command = new Command("create", "Create a new playlist") { nameArg };

        command.SetAction(parseResult =>
        {
            var name = parseResult.GetValue(nameArg)!;
            var playlist = playlistService.Create(name);
            Console.WriteLine($"Playlist '{playlist.Name}' created.");
        });

        return command;
    }

    private static Command BuildList(PlaylistService playlistService)
    {
        var command = new Command("list", "Show all playlists");

        command.SetAction(_ =>
        {
            var playlists = playlistService.GetAllPlaylistNames().ToList();
            if (playlists.Count == 0)
            {
                Console.WriteLine("No playlists found.");
                return;
            }

            foreach (var name in playlists)
                Console.WriteLine($"  • {name}");
        });

        return command;
    }

    private static Command BuildAdd(PlaylistService playlistService, MetadataService metadataService)
    {
        var nameArg = new Argument<string>("name") { Description = "Playlist name" };
        var pathArg = new Argument<string>("path") { Description = "Path to file or directory" };
        var command = new Command("add", "Add track or directory to playlist") { nameArg, pathArg };

        command.SetAction(parseResult =>
        {
            var name = parseResult.GetValue(nameArg)!;
            var path = parseResult.GetValue(pathArg)!;

            if (System.IO.Directory.Exists(path))
            {
                var tracks = metadataService.CreateTracksFromDirectory(path).ToList();
                foreach (var track in tracks)
                    playlistService.AddTrack(name, track);

                Console.WriteLine($"Added {tracks.Count} tracks to '{name}'.");
            }
            else if (System.IO.File.Exists(path))
            {
                var track = metadataService.CreateTrack(path);
                playlistService.AddTrack(name, track);
                Console.WriteLine($"Додано: {track}");
            }
            else
            {
                Console.WriteLine($"File or directory not found: {path}");
            }
        });

        return command;
    }

    private static Command BuildRemove(PlaylistService playlistService)
    {
        var nameArg = new Argument<string>("name") { Description = "Playlist name" };
        var pathArg = new Argument<string>("path") { Description = "Path to file" };
        var command = new Command("remove", "Remove track from playlist") { nameArg, pathArg };

        command.SetAction(parseResult =>
        {
            var name = parseResult.GetValue(nameArg)!;
            var path = parseResult.GetValue(pathArg)!;
            playlistService.RemoveTrack(name, path);
            Console.WriteLine($"Track removed from '{name}'.");
        });

        return command;
    }

    private static Command BuildShow(PlaylistService playlistService)
    {
        var nameArg = new Argument<string>("name") { Description = "Playlist name" };
        var command = new Command("show", "Show playlist tracks") { nameArg };

        command.SetAction(parseResult =>
        {
            var name = parseResult.GetValue(nameArg)!;
            var playlist = playlistService.Get(name);

            if (playlist is null)
            {
                Console.WriteLine($"Playlist '{name}' not found.");
                return;
            }

            Console.WriteLine($"Playlist: {playlist.Name} ({playlist.Tracks.Count} tracks)");
            for (int i = 0; i < playlist.Tracks.Count; i++)
                Console.WriteLine($"  {i + 1}. {playlist.Tracks[i]}");
        });

        return command;
    }

    private static Command BuildDelete(PlaylistService playlistService)
    {
        var nameArg = new Argument<string>("name") { Description = "Playlist name" };
        var command = new Command("delete", "Delete playlist") { nameArg };

        command.SetAction(parseResult =>
        {
            var name = parseResult.GetValue(nameArg)!;
            playlistService.Delete(name);
            Console.WriteLine($"Playlist '{name}' deleted.");
        });

        return command;
    }
}