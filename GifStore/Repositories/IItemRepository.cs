using GifStore.Data.ViewModels;
using GifStore.Entities;

namespace GifStore.Repositories
{
    public interface IItemRepository
    {
        public Task<ItemViewModel> Get(Guid id);
        public Task<Item> GetRaw(Guid id);
        public Task<Item> GetRawByFilename(string filename);
        public Task<IEnumerable<ItemViewModel>> GetAll(int skip, int take, User user);
        public Task<IEnumerable<ItemViewModel>> Search(int skip, int take, User user, string keyword);
        public Task<int> Count(User user);
        public Task<int> Count(User user, string keyword);
        public Task<ItemViewModel> Save(Item item);
        public Task<bool> Update(Item item);
        public Task<bool> Delete(Item item);

        public Task<bool> SaveChanges();
    }
}
