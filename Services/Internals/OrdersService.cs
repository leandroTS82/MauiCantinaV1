using CantinaV1.Data;
using CantinaV1.Models;
using System.Collections.ObjectModel;

namespace CantinaV1.Services.Internals
{
    public class OrdersService
    {
        private readonly Database _database;

        public OrdersService()
        {
            string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "cantina.db4");
            _database = new Database(dbPath);
        }

        internal void ClearListOrderInPage(ObservableCollection<OrderItem> orderItems)
        {
            while (orderItems.Any())
            {
                orderItems.RemoveAt(0);
            }
        }

        internal async Task DeleteAllAsync()
        {
            await _database.DeleteAllAsync<OrderItem>();
        }

        internal async Task<List<OrderItem>> GetAllAsync()
        {
            return await _database.GetAllAsync<OrderItem>();
        }

        internal async Task SaveItemAsync(OrderItem item)
        {
            await _database.InsertAsync<OrderItem>(item);
        }
    }
}
