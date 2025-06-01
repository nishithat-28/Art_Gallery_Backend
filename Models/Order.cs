namespace ArtGallery.API.Models
{
    using System.ComponentModel.DataAnnotations;

    public class Order
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        [Required]
        public User User { get; set; } = null!;
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        [Required]
        public string Status { get; set; } = string.Empty;
        [Required]
        public string ShippingAddress { get; set; } = string.Empty;
        [Required]
        public string PaymentMethod { get; set; } = string.Empty;
        [Required]
        public string InvoiceNumber { get; set; } = string.Empty;
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}