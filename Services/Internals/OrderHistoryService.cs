using CantinaV1.Data;
using CantinaV1.Models;

namespace CantinaV1.Services.Internals
{
    public class OrderHistoryService
    {
        private readonly Database _database;
        public OrderHistoryService()
        {
            string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "cantina.db4");
            _database = new Database(dbPath);
        }
        public async Task<List<OrderHistory>> GetAllAsync()
        {
            return await _database.GetAllAsync<OrderHistory>();
        }

        internal List<OrderHistory> ConvertOrderItemsToOrderHistoryItems(string observation, List<OrderItem> orderItems)
        {
            if (orderItems == null || !orderItems.Any())
                return new List<OrderHistory>();

            var orderHistoryList = new List<OrderHistory>();

            foreach (var item in orderItems)
            {
                var historyItem = new OrderHistory
                {
                    Date = item.Created,
                    ClientName = item.ClientName,
                    ProductName = item.ProductName,
                    Price = item.Price,
                    Quantity = item.Quantity,
                    Total = item.Total,
                    PaymentMethod = item.PaymentMethod,
                    Observation = $"{item.OrderNotes} |Obs. Limpeza:{observation}"
                };

                orderHistoryList.Add(historyItem);
            }

            return orderHistoryList;
        }


        internal async Task DeleteAllAsync()
        {
            await _database.DeleteAllAsync<OrderHistory>();
        }

        internal async Task SaveItemAsync(OrderHistory item)
        {
            await _database.InsertAsync<OrderHistory>(item);
        }
    }
}
