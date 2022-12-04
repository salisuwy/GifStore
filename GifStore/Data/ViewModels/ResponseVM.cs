namespace GifStore.Data.ViewModels
{
    public class ResponseVM
    {
        public ResponseStatus Status { get; set; }
        public string Message { get; set; }
        public dynamic? Data { get; set; }
    }
}
