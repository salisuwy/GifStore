namespace GifStore.Utilities
{
    public class JwtConfiguration
    {
        public string?  Key { get; set; }
        public string? Issuer { get; set; }
        public string? Audience { get; set; }
        public int  Duration { get; set; }
    }
}
