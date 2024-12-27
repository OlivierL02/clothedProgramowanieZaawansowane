namespace clothed.Models
{
    public class Cart
    {
        public int Id { get; set; }
        public string UserEmail { get; set; }
        public DateTime CreatedAt { get; set; }

        public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
    }
}
