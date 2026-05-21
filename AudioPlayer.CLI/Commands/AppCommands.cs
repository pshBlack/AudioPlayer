using System.CommandLine;
using AudioPlayer.Core.Services;

namespace AudioPlayer.CLI.Commands;

public class AppCommands : RootCommand
{
    public AppCommands(
        PlaylistService playlistService,
        MetadataService metadataService,
        PlayerService playerService) : base("AudioPlayer CLI — Cool Console Audio Player")
    {
        Add(new PlaylistCommands(playlistService, metadataService));
        Add(new InfoCommands(metadataService));
        Add(new PlayerCommands(playerService, playlistService));
    }
}