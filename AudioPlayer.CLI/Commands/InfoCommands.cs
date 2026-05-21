using System.CommandLine;
using AudioPlayer.Core.Services;

namespace AudioPlayer.CLI.Commands;

public class InfoCommands : Command
{
    public InfoCommands(MetadataService metadataService) : base("info", "Information about audio file")
    {
        Add(BuildShow(metadataService));
    }

    private static Command BuildShow(MetadataService metadataService)
    {
        var pathArg = new Argument<string>("path") { Description = "Path to audio file" };
        var command = new Command("show", "Show audio file metadata") { pathArg };

        command.SetAction(parseResult =>
        {
            var path = parseResult.GetValue(pathArg)!;

            if (!System.IO.File.Exists(path))
            {
                Console.WriteLine($"File not found: {path}");
                return;
            }

            var track = metadataService.CreateTrack(path);
            Console.WriteLine($"File:       {path}");
            Console.WriteLine($"Title:      {track.Metadata.Title}");
            Console.WriteLine($"Artist:     {track.Metadata.Artist}");
            Console.WriteLine($"Album:      {track.Metadata.Album}");
            Console.WriteLine($"Year:       {track.Metadata.Year}");
            Console.WriteLine($"Genre:      {track.Metadata.Genre}");
            Console.WriteLine($"Duration:   {track.Duration:mm\\:ss}");
        });

        return command;
    }
}