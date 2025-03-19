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

        public Task<int> SaveProdutoAsync(Produto produto)
        {
            return _database.InsertAsync(produto);
        }

        public Task<int> DeleteProdutoAsync(Produto produto)
        {
            return _database.DeleteAsync(produto);
        }
    }
}
