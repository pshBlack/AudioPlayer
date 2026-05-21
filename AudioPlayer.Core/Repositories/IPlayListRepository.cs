using AudioPlayer.Core.Models;

namespace AudioPlayer.Core.Repositories
{
    public interface IPlayListRepository
    {
        void Save(Playlist playlist);
        Playlist? Load(string name);
        void Delete(string name);
        IEnumerable<string> GetAllPlaylistNames();
    }
}