namespace GifStore.Data.ViewModels
{
    public class ItemViewModel  
    {
        public Guid Id { get; set; }
        public string DisplayName { get; set; }
        public string PhysicalName { get; set; }
        public bool IsPublic { get; set; }
        public DateTime CreatedAt { get; set; }
        public virtual UserViewModel User { get; set; }
        public IEnumerable<string> Tags { get; set; }
    }
}
