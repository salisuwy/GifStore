using GifStore.Data;
using GifStore.Entities;
using Microsoft.EntityFrameworkCore;

namespace GifStore.Repositories
{
    public class TagRepository : ITagRepository
    {
        private readonly GifStoreDbContext db;
        private readonly ILogger<TagRepository> logger;

        public TagRepository(GifStoreDbContext _db, ILogger<TagRepository> _logger)
        {
            db = _db;
            logger = _logger;
        }

        public async Task<Tag> Get(Guid id)
        {
            try
            {
                var tag = await db.Tags
                    .Where(t => t.Id == id)
                    .FirstOrDefaultAsync();
                return tag;
            }
            catch (Exception exception)
            {
                logger.LogError(exception.Message);
                return null;
            }
        }

        public async Task<Tag> Get(string title)
        {
            try
            {
                var tag = await db.Tags
                    .Where(t => t.Title == title)
                    .FirstOrDefaultAsync();
                return tag;
            }
            catch (Exception exception)
            {
                logger.LogError(exception.Message);
                return null;
            }
        }

        public async Task<ItemTag> Get(Guid itemId, Guid tagId)
        {
            try
            {
                var tag = await db.ItemTags
                    .Where(it => it.ItemId == itemId && it.TagId == tagId)
                    .FirstOrDefaultAsync();
                return tag;
            }
            catch (Exception exception)
            {
                logger.LogError(exception.Message);
                return null;
            }
        }

        public async Task<bool> TagItem(ItemTag itemTag)
        {
            try
            {
                db.ItemTags.Add(itemTag);
                return await SaveChanges();
            }
            catch (Exception exception)
            {
                logger.LogError(exception.Message);
                return false;
            }
        }

        public async Task<bool> UntagItem(ItemTag itemTag)
        {
            try
            {
                db.ItemTags.Remove(itemTag);
                return await SaveChanges();
            }
            catch (Exception exception)
            {
                logger.LogError(exception.Message);
                return false;
            }
        }

        public async Task<bool> Save(Tag tag)
        {
            try
            {
                db.Tags.Add(tag);
                return await SaveChanges();
            }
            catch (Exception exception)
            {
                logger.LogError(exception.Message);
                return false;
            }
        }

        public async Task<bool> SaveChanges()
        {
            var affectedRows = await db.SaveChangesAsync();

            if (affectedRows > 0) return true;

            return false;
        }
    }
}
