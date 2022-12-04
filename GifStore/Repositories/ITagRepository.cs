using GifStore.Entities;

namespace GifStore.Repositories
{
    public interface ITagRepository
    {
        public Task<Tag> Get(Guid id);
        public Task<Tag> Get(string title);
        public Task<bool> Save(Tag tag);
        public Task<ItemTag> Get(Guid itemId, Guid tagId);
        public Task<bool> TagItem(ItemTag tag);
        public Task<bool> UntagItem(ItemTag tag);
        public Task<bool> SaveChanges();

    }
}
