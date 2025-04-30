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

        internal async Task<List<OrderToSend>> BuscarPedidosPorOrderKeyAsync(string orderKey)
        {
            List<OrderToSend> orders = new List<OrderToSend>();
            string fireBaseUrlDb = await GetValue("FirebaseAuthDomain");
            fireBaseUrlDb = "https://pedidoscantina-7b0d6-default-rtdb.firebaseio.com";
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
                    order.Id = firebaseOrder.Key; // Adiciona o ID do pedido
                    orders.Add(order);
                }
            }

            return orders;
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
