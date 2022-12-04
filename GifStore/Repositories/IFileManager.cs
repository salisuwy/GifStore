using GifStore.Data.ViewModels;
using GifStore.Entities;

namespace GifStore.Repositories
{
    public interface IFileManager
    {
        public Task<ResponseVM> Upload(User user, IFormFile file, CancellationToken token);

        public Task<ResponseVM> Download(string filename);

        public Task<bool> Delete(string filename);
    }
}
