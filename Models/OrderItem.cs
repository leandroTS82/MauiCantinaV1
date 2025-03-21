namespace CantinaV1.Models
{
    public class OrderItem
    {
        public int Id { get; set; }
        public string ClientName { get; set; }
        public string PaymentMethod { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal Total => Price * Quantity;
    }
}
