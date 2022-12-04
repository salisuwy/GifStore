using GifStore.Entities;

namespace GifStore.Utilities
{
    public interface ITokenProvider
    {
        public string GenerateToken(User user);
    }
}
