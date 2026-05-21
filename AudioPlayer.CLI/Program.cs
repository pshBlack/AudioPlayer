using AudioPlayer.Core.Repositories;
using AudioPlayer.Core.Services;
using AudioPlayer.CLI.Commands;

var storageDir = Path.Combine(AppContext.BaseDirectory, "playlists");
var repository = new PlaylistRepository(storageDir);
var metadataService = new MetadataService();
var playlistService = new PlaylistService(repository);
var playerService = new PlayerService();

var app = new AppCommands(playlistService, metadataService, playerService);
return await app.Parse(args).InvokeAsync();