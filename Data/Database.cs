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
            _database.CreateTableAsync<Produto>().Wait();
        }

        public Task<List<Produto>> GetProdutosAsync()
        {
            return _database.Table<Produto>().ToListAsync();
        }

        public async Task InicializarProdutosAsync()
        {
            var produtos = await GetProdutosAsync();
            _database.DeleteAllAsync<Produto>();
            var produtosIniciais = new List<Produto>
                {
                    new Produto { Nome = "Café", Preco = 1.00m, Quantidade = 0 },
                    new Produto { Nome = "Pão de Queijo", Preco = 5.00m, Quantidade = 0 },
                    new Produto { Nome = "Pão de batata", Preco = 7.00m, Quantidade = 0 }
                };

            foreach (var produto in produtosIniciais)
            {
                await _database.InsertAsync(produto);
            }

        }
        public Task<int> DeleteAllProdutosAsync()
        {
            return _database.DeleteAllAsync<Produto>();
        }

        public Task<int> SaveProdutoAsync(Produto produto)
        {
            return _database.InsertAsync(produto);
        }

        public Task<int> UpdateProdutoAsync(Produto produto)
        {
            return _database.UpdateAsync(produto);
        }

        public Task<int> DeleteProdutoAsync(Produto produto)
        {
            return _database.DeleteAsync(produto);
        }
    }
}
