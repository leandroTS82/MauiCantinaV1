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
            _database.CreateTableAsync<Order>().Wait();
            _database.CreateTableAsync<OrderItem>().Wait();
        }

        public Task<List<Product>> GetProdutosAsync()
        {
            return _database.Table<Product>().ToListAsync();
        }

        public async Task InicializarProdutosAsync()
        {
            var produtos = await GetProdutosAsync();
            _database.DeleteAllAsync<OrderItem>();
            var produtosIniciais = new List<OrderItem>
                {
                    new OrderItem { ProductName = "Café", Price = 1.00m, Quantity = 0 },
                    new OrderItem { ProductName = "Pão de Queijo", Price = 5.00m, Quantity = 0 },
                    new OrderItem { ProductName = "Pão de batata", Price = 7.00m, Quantity = 0 }
                };

            foreach (var produto in produtosIniciais)
            {
                await _database.InsertAsync(produto);
            }

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

        public Task<List<Order>> GetPedidosAsync()
        {
            return _database.Table<Order>().ToListAsync();
        }

        public Task<int> SavePedidoAsync(Order pedido)
        {
            return _database.InsertAsync(pedido);
        }
    }
}