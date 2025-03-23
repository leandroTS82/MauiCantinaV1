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
        }

        public Task<List<Product>> GetProdutosAsync()
        {
            return _database.Table<Product>().ToListAsync();
        }

        public Task<int> SaveProdutoAsync(Product produto)
        {
            return _database.InsertAsync(produto);
        }

        public Task<int> DeleteAllProdutosAsync()
        {
            return _database.DeleteAllAsync<Product>();
        }

        public Task<int> UpdateProdutoAsync(Product produto)
        {
            return _database.UpdateAsync(produto);
        }

        public Task<List<OrderItem>> GetPedidosAsync()
        {
            return _database.Table<OrderItem>().ToListAsync();
        }

        public Task<int> SavePedidoAsync(OrderItem pedido)
        {
            return _database.InsertAsync(pedido);
        }

        internal async Task<List<OrderItem>> GetOrderItemsAsync()
        {
            throw new NotImplementedException();
        }

        internal async Task UpdateOrderItemAsync(OrderItem produto)
        {
            throw new NotImplementedException();
        }
    }
}