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
            _database.CreateTableAsync<Configuration>().Wait();
            _database.CreateTableAsync<GenericConfiguration>().Wait();
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

        public Task<int> SaveConfiguracaoAsync(Configuration config)
        {
            return _database.InsertOrReplaceAsync(config);
        }
        public Task<int> SaveGenericConfigurationAsync(GenericConfiguration config)
        {
            return _database.InsertAsync(config);
        }
        public Task<int> UpdateGenericConfigurationoAsync(GenericConfiguration genericConfiguration)
        {
            return _database.UpdateAsync(genericConfiguration);
        }
        public Task<List<GenericConfiguration>> GetGenericConfigurationAsync()
        {
            return _database.Table<GenericConfiguration>().ToListAsync();
        }
        public Task<Configuration> GetConfiguracaoAsync()
        {
            return _database.Table<Configuration>().FirstOrDefaultAsync();
        }


        public Task<List<OrderItem>> GetPedidosAsync()
        {
            return _database.Table<OrderItem>().ToListAsync();
        }

        // Recuperar pedidos por ClientName
        public Task<List<OrderItem>> GetPedidosByClientNameAsync(string clientName)
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

        public Task<int> SavePedidoAsync(OrderItem pedido)
        {
            return _database.InsertAsync(pedido);
        }

        internal async Task UpdateOrderItemAsync(OrderItem produto)
        {
            throw new NotImplementedException();
        }
    }
}