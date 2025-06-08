namespace ArtGallery.API.DTOs
{
    public class OrderCreateDto
    {
        public required string ShippingAddress { get; set; }
        public required string PaymentMethod { get; set; }
        public required List<OrderItemDto> OrderItems { get; set; }
    }

    public class OrderItemDto
    {
        public int ArtWorkId { get; set; }
        public int Quantity { get; set; }
    }

    public class OrderResponseDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public required string Username { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public required string Status { get; set; }
        public required string ShippingAddress { get; set; }
        public required string PaymentMethod { get; set; }
        public required string InvoiceNumber { get; set; }
        public required List<OrderItemResponseDto> OrderItems { get; set; }
    }

    public class OrderItemResponseDto
    {
        public int Id { get; set; }
        public int ArtWorkId { get; set; }
        public required string ArtWorkTitle { get; set; }
        public required string ArtWorkArtist { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal Subtotal { get; set; }
    }

    public class InvoiceDto
    {
        public required string InvoiceNumber { get; set; }
        public DateTime InvoiceDate { get; set; }
        public required string CustomerName { get; set; }
        public required string CustomerEmail { get; set; }
        public required string ShippingAddress { get; set; }
        public required string PaymentMethod { get; set; }
        public required List<OrderItemResponseDto> Items { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Tax { get; set; }
        public decimal Total { get; set; }
    }
}