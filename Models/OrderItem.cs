namespace ArtGallery.API.Models
{
    using System.ComponentModel.DataAnnotations;

    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        [Required]
        public Order Order { get; set; } = null!;
        public int ArtWorkId { get; set; }
        [Required]
        public ArtWork ArtWork { get; set; } = null!;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
}