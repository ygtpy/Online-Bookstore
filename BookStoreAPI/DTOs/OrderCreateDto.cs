namespace BookStoreAPI.DTOs
{
    public class OrderCreateDto
    {
        public int UserId { get; set; }
        public string? ShippingAddress { get; set; }
        public List<OrderItemCreateDto> OrderItems { get; set; } = new List<OrderItemCreateDto>();
    }

    public class OrderItemCreateDto
    {
        public int BookId { get; set; }
        public int Quantity { get; set; }
    }
}