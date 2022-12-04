using GifStore.Data;
using GifStore.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata.Ecma335;

namespace GifStore.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly GifStoreDbContext db;
        private readonly ILogger<UserRepository> logger;

        public UserRepository(GifStoreDbContext _db, ILogger<UserRepository> _logger)
        {
            db = _db;
            logger = _logger;
        }
        public async Task<User> Get(string email)
        {
            try
            {
                var user = await db.Users
                    .Where(u => u.Email == email)
                    .FirstOrDefaultAsync();
                return user;   
            }
            catch(Exception exception)
            {
                logger.LogError(exception.Message);
                return null;
            }
        }

        public async Task<User> Get(Guid Id)
        {
            try
            {
                var user = await db.Users
                    .Where(u => u.Id == Id)
                    .FirstOrDefaultAsync();
                return user;
            }
            catch (Exception exception)
            {
                logger.LogError(exception.Message);
                return null;
            }
        }

        public async Task<bool> Save(User user)
        {
            try
            {
                db.Users.Add(user);
                return await SaveChanges();
            }
            catch (Exception exception)
            {
                logger.LogError(exception.Message);
                return false;
            }
        }

        public async Task<bool> Update(User user)
        {
            try
            {
                db.Users.Update(user);
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
