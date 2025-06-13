﻿namespace CantinaV1.Models
{
    public class OrderToSend
    {
        public string ClientName { get; set; }
        public List<ProductToOrder> Products { get; set; }
        public string Created { get; set; }
        public string Id { get; internal set; }
        public string OrderKey { get; set; }
        public bool IsSelected { get; set; }
    }
    public class ProductToOrder
    {
        public string ProductName { get; set; }
        public int Quantity { get; set; }
    }
}
