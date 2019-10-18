using System.IO;
using System.Threading.Tasks;
using Games.Interfaces;
using Games.Models;
using Microsoft.Extensions.FileProviders;

namespace Games.Services
{
    public class ImageService : IImageService
    {
        private readonly IFileProvider fileProvider;

        public ImageService(IFileProvider fileProvider)
        {
            this.fileProvider = fileProvider;
        }

        public async Task<string> CreateImage(Game game, Stream stream)
        {
            var imagePath = $"images/{game.Id}/image.jpg";
            var physicalPath = fileProvider.GetFileInfo(imagePath).PhysicalPath;

            Directory.CreateDirectory(Path.GetDirectoryName(physicalPath));

            using (var file = new FileStream(physicalPath, FileMode.Create))
                await stream.CopyToAsync(file);

            return imagePath;
        }

        public void DeleteImage(Game game)
        {
            var imageDir = fileProvider.GetFileInfo($"images/{game.Id}").PhysicalPath;

            if (Directory.Exists(imageDir))
                Directory.Delete(imageDir, true);
        }
    }
}