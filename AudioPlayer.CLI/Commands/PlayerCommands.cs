using System.CommandLine;
using AudioPlayer.Core.Enums;
using AudioPlayer.Core.Services;

namespace AudioPlayer.CLI.Commands;

public class PlayerCommands : Command
{
    public PlayerCommands(
        PlayerService playerService,
        PlaylistService playlistService) : base("player", "Manage audio playback")
    {
        Add(BuildPlay(playerService, playlistService));
        Add(BuildShuffle(playlistService));
        Add(BuildRepeat(playlistService));
        Add(BuildSearch(playlistService));
    }

    private static Command BuildPlay(PlayerService playerService, PlaylistService playlistService)
    {
        var nameArg = new Argument<string>("playlist") { Description = "Playlist name" };
        var indexOpt = new Option<int>("--index") { Description = "Track index (starting from 0)", DefaultValueFactory = _ => 0 };
        var command = new Command("play", "Play track from playlist") { nameArg, indexOpt };

        command.SetAction(parseResult =>
        {
            var name = parseResult.GetValue(nameArg)!;
            var index = parseResult.GetValue(indexOpt);

            var playlist = playlistService.Get(name);
            if (playlist is null)
            {
                Console.WriteLine($"Playlist '{name}' not found.");
                return;
            }

            if (index < 0 || index >= playlist.Tracks.Count)
            {
                Console.WriteLine($"Invalid index. Playlist has {playlist.Tracks.Count} tracks.");
                return;
            }

            var track = playlist.Tracks[index];
            playerService.Play(track);

            Console.WriteLine($"Playing: {track}");
            Console.WriteLine("Press Enter to stop...");
            Console.ReadLine();
            playerService.Stop();
        });

        return command;
    }

    private static Command BuildShuffle(PlaylistService playlistService)
    {
        var nameArg = new Argument<string>("playlist") { Description = "Playlist name" };
        var command = new Command("shuffle", "Shuffle playlist tracks") { nameArg };

        command.SetAction(parseResult =>
        {
            var name = parseResult.GetValue(nameArg)!;
            playlistService.Shuffle(name);
            Console.WriteLine($"Playlist '{name}' shuffled.");
        });

        return command;
    }

    private static Command BuildRepeat(PlaylistService playlistService)
    {
        var nameArg = new Argument<string>("playlist") { Description = "Playlist name" };
        var modeArg = new Argument<RepeatMode>("mode") { Description = "Repeat mode: None, One, All" };
        var command = new Command("repeat", "Set repeat mode") { nameArg, modeArg };

        command.SetAction(parseResult =>
        {
            var name = parseResult.GetValue(nameArg)!;
            var mode = parseResult.GetValue(modeArg);
            playlistService.SetRepeatMode(name, mode);
            Console.WriteLine($"Repeat mode for '{name}': {mode}.");
        });

        return command;
    }

    private static Command BuildSearch(PlaylistService playlistService)
    {
        var nameArg = new Argument<string>("playlist") { Description = "Playlist name" };
        var queryArg = new Argument<string>("query") { Description = "Search query" };
        var command = new Command("search", "Search tracks in playlist") { nameArg, queryArg };

        command.SetAction(parseResult =>
        {
            var name = parseResult.GetValue(nameArg)!;
            var query = parseResult.GetValue(queryArg)!;

            var results = playlistService.Search(name, query).ToList();
            if (results.Count == 0)
            {
                Console.WriteLine("No tracks found.");
                return;
            }

            Console.WriteLine($"Found {results.Count} tracks:");
            foreach (var track in results)
                Console.WriteLine($"  • {track}");
        });

        return command;
    }
}