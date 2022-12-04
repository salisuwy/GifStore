namespace GifStore.Entities
{
    public class User : Entity
    {
        public string Fullname { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
    }
}
