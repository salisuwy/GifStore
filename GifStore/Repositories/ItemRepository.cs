using GifStore.Data;
using GifStore.Data.ViewModels;
using GifStore.Entities;
using Microsoft.EntityFrameworkCore;

namespace GifStore.Repositories
{
    public class ItemRepository : IItemRepository
    {
        private readonly GifStoreDbContext db;
        private readonly ILogger<ItemRepository> logger;

        public ItemRepository(GifStoreDbContext _db, ILogger<ItemRepository> _logger)
        {
            db = _db;
            logger = _logger;
        }


        public async Task<ItemViewModel> Get(Guid id)
        {
            try
            {
                var item = await db.Items
                    .Where(i => i.Id == id)
                    .Select(i => new ItemViewModel
                    {
                        Id = i.Id,
                        DisplayName = i.DisplayName,
                        PhysicalName = i.PhysicalName,
                        IsPublic = i.IsPublic,
                        CreatedAt = i.CreatedAt,
                        User = new UserViewModel
                        {
                            Id = i.User.Id,
                            Fullname = i.User.Fullname,
                            Email = i.User.Email
                        },
                        Tags = i.ItemTag.Select(it => it.Tag.Title)
                    })
                    .FirstOrDefaultAsync();
                return item;
            }
            catch (Exception exception)
            {
                logger.LogError(exception.Message);
                return null;
            }
        }

        public async Task<Item> GetRaw(Guid id)
        {
            try
            {
                var item = await db.Items
                    .Where(i => i.Id == id)
                    .FirstOrDefaultAsync();
                return item;
            }
            catch (Exception exception)
            {
                logger.LogError(exception.Message);
                return null;
            }
        }

        public async Task<Item> GetRawByFilename(string filename)
        {
            try
            {
                var item = await db.Items
                    .Where(i => i.PhysicalName.ToLower() == filename.ToLower())
                    .FirstOrDefaultAsync();
                return item;
            }
            catch (Exception exception)
            {
                logger.LogError(exception.Message);
                return null;
            }
        }

        public async Task<IEnumerable<ItemViewModel>> GetAll(int skip, int take, User user)
        {
            try
            {
                var items = await db.Items
                    .Where(i => i.User == user)
                    .OrderByDescending(i => i.CreatedAt)
                    .Skip(skip)
                    .Take(take)
                    .Select(i => new ItemViewModel
                    {
                        Id = i.Id,
                        DisplayName = i.DisplayName,
                        PhysicalName = i.PhysicalName,
                        IsPublic = i.IsPublic,
                        CreatedAt = i.CreatedAt,
                        User = new UserViewModel
                        {
                            Id = i.User.Id,
                            Fullname = i.User.Fullname,
                            Email = i.User.Email
                        },
                        Tags = i.ItemTag.Select(it => it.Tag.Title)
                    })
                    .ToListAsync();
                return items;
            }
            catch (Exception exception)
            {
                logger.LogError(exception.Message);
                return null;
            }
        }

        public async Task<IEnumerable<ItemViewModel>> Search(int skip, int take, User user, string keyword)
        {
            try
            {
                keyword = $"%{keyword}%".ToLower();
                var items = await db.Items
                    .Where(i => i.User == user)
                    .Where(i => EF.Functions.Like(i.DisplayName.ToLower(), keyword)
                        || i.ItemTag.Any(it => EF.Functions.Like(it.Tag.Title.ToLower(), keyword))
                    )
                    .OrderByDescending(i => i.CreatedAt)
                    .Skip(skip)
                    .Take(take)
                    .Select(i => new ItemViewModel
                    {
                        Id = i.Id,
                        DisplayName = i.DisplayName,
                        PhysicalName = i.PhysicalName,
                        IsPublic = i.IsPublic,
                        CreatedAt = i.CreatedAt,
                        User = new UserViewModel
                        {
                            Id = i.User.Id,
                            Fullname = i.User.Fullname,
                            Email = i.User.Email
                        },
                        Tags = i.ItemTag.Select(it => it.Tag.Title)
                    })
                    .ToListAsync();
                return items;
            }
            catch (Exception exception)
            {
                logger.LogError(exception.Message);
                return null;
            }
        }

        public async Task<int> Count(User user)
        {
            try
            {
                var count = await db.Items
                    .Where(i => i.User == user)
                    .CountAsync();
                return count;
            }
            catch (Exception exception)
            {
                logger.LogError(exception.Message);
                return 0;
            }
        }

        public async Task<int> Count(User user, string keyword)
        {
            try
            {
                keyword = $"%{keyword}%".ToLower();
                var count = await db.Items
                    .Where(i => i.User == user)
                    .Where(i => EF.Functions.Like(i.DisplayName.ToLower(), keyword)
                                || i.ItemTag.Any(it => EF.Functions.Like(it.Tag.Title.ToLower(), keyword))
                    )
                    .CountAsync();
                return count;
            }
            catch (Exception exception)
            {
                logger.LogError(exception.Message);
                return 0;
            }
        }

        public async Task<ItemViewModel> Save(Item item)
        {
            try
            {
                db.Items.Add(item);
                var result = await SaveChanges();

                if (result)
                {
                    var itemViewModel = new ItemViewModel
                    {
                        Id = item.Id,
                        DisplayName = item.DisplayName,
                        PhysicalName = item.PhysicalName,
                        IsPublic = item.IsPublic,
                        CreatedAt = item.CreatedAt,
                        User = new UserViewModel
                        {
                            Id = item.User.Id,
                            Fullname = item.User.Fullname,
                            Email = item.User.Email
                        }
                    };
                    return itemViewModel;
                }

                return null;
            }
            catch (Exception exception)
            {
                logger.LogError(exception.Message);
                return null;
            }
        }

        public async Task<bool> Update(Item item)
        {
            try
            {
                db.Items.Update(item);
                return await SaveChanges();
            }
            catch (Exception exception)
            {
                logger.LogError(exception.Message);
                return false;
            }
        }

        public async Task<bool> Delete(Item item)
        {
            try
            {
                db.Items.Remove(item);
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
