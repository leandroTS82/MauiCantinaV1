// Database.cs
using SQLite;
using CantinaV1.Models;

namespace CantinaV1.Data
{
    public class Database
    {
        private readonly SQLiteAsyncConnection _database;

        public Database(string dbPath)
        {
            _database = new SQLiteAsyncConnection(dbPath);
            _database.CreateTableAsync<Product>().Wait();
            _database.CreateTableAsync<OrderItem>().Wait();
            _database.CreateTableAsync<GenericConfiguration>().Wait();
            _database.CreateTableAsync<OrderHistory>().Wait();
            _database.CreateTableAsync<ContactModel>().Wait(); // Novo
        }

        public Task<List<T>> GetAllAsync<T>() where T : new() => _database.Table<T>().ToListAsync();
        public Task<int> DeleteAllAsync<T>() where T : new() => _database.DeleteAllAsync<T>();
        public Task<int> DeleteAsync<T>(T item) where T : new() => _database.DeleteAsync(item);
        public Task<int> InsertAsync<T>(T item) where T : new() => _database.InsertAsync(item);
        public Task<int> UpdateAsync<T>(T item) where T : new() => _database.UpdateAsync(item);

        // refatoração de detalhe de pedido pendente

        // Recuperar pedidos por ClientName da página order item
        public Task<List<OrderItem>> GetOrdersByClientNameAsync(string clientName)
        {
            return _database.Table<OrderItem>()
                            .Where(orderItem => orderItem.ClientName.ToLower().Equals(clientName.ToLower()))
                            .ToListAsync();
        }

        // Excluir um item de pedido
        public Task<int> DeleteOrderItemAsync(OrderItem orderItem)
        {
            return _database.DeleteAsync(orderItem);
        }
    }
}