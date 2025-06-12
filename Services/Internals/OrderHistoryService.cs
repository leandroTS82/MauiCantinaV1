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
