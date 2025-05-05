using CantinaV1.Data;
using CantinaV1.Models;

namespace CantinaV1.Services.Internals
{
    public class ProductsService
    {
        private readonly Database _database;
        public ProductsService()
        {
            string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "cantina.db4");
            _database = new Database(dbPath);
        }

        internal async Task DeleteAllAsync()
        {
            await _database.DeleteAllAsync<Product>();
        }

        internal async Task<List<Product>> GetAllAsync()
        {
            return await _database.GetAllAsync<Product>();
        }

        internal async Task SaveItemAsync(Product item)
        {
            await _database.InsertAsync<Product>(item);
        }
    }
}
