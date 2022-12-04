using System.ComponentModel.DataAnnotations;

namespace GifStore.Data.ViewModels
{
    public class LoginTokenVM
    {
        public Guid Id { get; set; }
        public string Fullname { get; set; }
        public string Email { get; set; }
        public string Token { get; set; }
    }
}
