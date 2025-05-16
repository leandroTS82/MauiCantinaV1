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

            var orderToSend = new OrderToSend
            {
                ClientName = clientName,
                Products = productsOrder,
                Created = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };

            await _firebaseClient
                .Child(orderKey)
                .PostAsync(orderToSend);
        }

        internal async Task<List<OrderToSend>> GetOrdersByOrderKeyAsync(string orderKey)
        {
            List<OrderToSend> orders = new List<OrderToSend>();
            string fireBaseUrlDb = await GetValue("FirebaseAuthDomain");
            if (string.IsNullOrWhiteSpace(fireBaseUrlDb))
                return orders;

            FirebaseClient _firebaseClient = new FirebaseClient(fireBaseUrlDb);
            var firebaseOrders = await _firebaseClient
                .Child(orderKey)
                .OnceAsync<OrderToSend>();
            foreach (var firebaseOrder in firebaseOrders)
            {
                if (firebaseOrder.Object != null)
                {
                    var order = firebaseOrder.Object;
                    order.OrderKey = firebaseOrder.Key; // Adiciona o ID do pedido
                    orders.Add(order);
                }
            }

            return orders;
        }

        public async Task UpdateOrderIsSelectedAsync(OrderToSend order)
        {
            if (order == null || string.IsNullOrEmpty(order.OrderKey))
                return;

            string fireBaseUrlDb = await GetValue("FirebaseAuthDomain");
            string orderKey = await GetValue("entryRegisterCodeApp");

            if (string.IsNullOrWhiteSpace(fireBaseUrlDb) || string.IsNullOrWhiteSpace(orderKey))
                return;

            var firebaseClient = new FirebaseClient(fireBaseUrlDb);

            await firebaseClient
                .Child(orderKey)
                .Child(order.OrderKey)
                .PatchAsync(new { IsSelected = order.IsSelected });
        }

        #region InternalMethods
        private async Task<string> GetValue(string key)
        {
            var config = await _genericConfigurationServices.GetGenericConfigurationAsync(key);
            return config?.Value ?? string.Empty;
        }
        #endregion
    }
}
