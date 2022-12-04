using GifStore.Entities;

namespace GifStore.Repositories
{
    public interface IUserRepository
    {
        public Task<User> Get(string email);
        public Task<User> Get(Guid Id);
        public Task<bool> Save(User user);
        public Task<bool> Update(User user);

        public Task<bool> SaveChanges();
    }
}
