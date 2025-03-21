namespace CantinaV1.Models
{
    public class Order
    {
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal Total => Price * Quantity;
        public string ClientName { get; set; }
        public string PaymentMethod { get; set; }
    }
}
