namespace GifStore.Entities
{
    public class Item : Entity
    {
        public string DisplayName { get; set; }
        public string PhysicalName { get; set; }
        public bool IsPublic { get; set; } = false;
        public virtual Guid UserId { get; set; }
        public virtual User User { get; set; }

        public virtual ICollection<ItemTag> ItemTag { get; set; }
    }
}
