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
        }

        public Task<List<Product>> GetProdutosAsync()
        {
            return _database.Table<Product>().ToListAsync();
        }

        public async Task InicializarProdutosAsync()
        {
            var produtos = await GetProdutosAsync();
            _database.DeleteAllAsync<Product>();
            var produtosIniciais = new List<Product>
                {
                    new Product { Nome = "Café", Preco = 1.00m, Quantidade = 0 },
                    new Product { Nome = "Pão de Queijo", Preco = 5.00m, Quantidade = 0 },
                    new Product { Nome = "Pão de batata", Preco = 7.00m, Quantidade = 0 }
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