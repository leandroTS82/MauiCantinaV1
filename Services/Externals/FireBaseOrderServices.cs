using CantinaV1.Models;
using CantinaV1.Services.Internals;
using Firebase.Database;
using Firebase.Database.Query;

namespace CantinaV1.Services.Externals
{
    public class FireBaseOrderServices
    {
        private readonly GenericConfigurationServices _genericConfigurationServices;

        public FireBaseOrderServices()
        {
            _genericConfigurationServices = new GenericConfigurationServices();
        }

        private async Task<string> GetValue(string key)
        {
            var config = await _genericConfigurationServices.GetGenericConfigurationAsync(key);
            return config?.Value ?? string.Empty;
        }

        public async Task SendOrderAsync(List<OrderItem> orderItemsToSave)
        {
            if (orderItemsToSave == null || !orderItemsToSave.Any())
                return;

            string fireBaseUrlDb = await GetValue("FirebaseAuthDomain");
            if (string.IsNullOrWhiteSpace(fireBaseUrlDb))
                return;

            FirebaseClient _firebaseClient = new FirebaseClient(fireBaseUrlDb);
            string orderKey = await GetValue("CodeApp");
            if (string.IsNullOrWhiteSpace(orderKey))
                return;

            string clientName = orderItemsToSave.First().ClientName?.Trim();
            if (string.IsNullOrWhiteSpace(clientName))
                return;

            var productsOrder = orderItemsToSave
                .Select(item => new ProductToOrder
                {
                    ProductName = item.ProductName,
                    Quantity = item.Quantity
                })
                .ToList();

            var pedido = new OrderToSend
            {
                ClientName = clientName,
                Products = productsOrder,
                Created = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };

            await _firebaseClient
                .Child(orderKey)
                .PostAsync(pedido);
        }
    }

    internal class ProductToOrder
    {
        public string ProductName { get; set; }
        public int Quantity { get; set; }
    }

    internal class OrderToSend
    {
        public string ClientName { get; set; }
        public List<ProductToOrder> Products { get; set; }
        public string Created { get; set; }
    }
}
