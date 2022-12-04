using System.ComponentModel.DataAnnotations;

namespace GifStore.Data.Dtos
{
    public class RegisterDto
    {
        [Required]
        public string Fullname { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        
        [Required]
        [StringLength(30, MinimumLength = 6)]
        public string Password { get; set; }
    }
}
