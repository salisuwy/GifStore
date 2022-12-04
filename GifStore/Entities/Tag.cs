namespace GifStore.Entities
{
    public class Tag : Entity
    {
        public string Title { get; set; }

        public virtual ICollection<ItemTag> ItemTag { get; set; }
    }
}
