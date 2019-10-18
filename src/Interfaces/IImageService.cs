using System.IO;
using System.Threading.Tasks;
using Games.Models;

namespace Games.Interfaces
{
    public interface IImageService
    {
        Task<string> CreateImage(Game game, Stream stream);
        void DeleteImage(Game game);
    }
}