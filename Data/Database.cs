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
        }

        // Método genérico para obter todos os registros de qualquer tipo de tabela
        public Task<List<T>> GetAllAsync<T>() where T : new()
        {
            return _database.Table<T>().ToListAsync();
        }
        // Método genérico para excluir todos os registros de qualquer tipo de tabela
        public Task<int> DeleteAllAsync<T>() where T : new()
        {
            return _database.DeleteAllAsync<T>();
        }
        // Método genérico para excluir um registro específico de qualquer tipo de tabela
        public Task<int> DeleteAsync<T>(T item) where T : new()
        {
            return _database.DeleteAsync(item);
        }
        // Método genérico para inserir um registro em qualquer tipo de tabela
        public Task<int> InsertAsync<T>(T item) where T : new()
        {
            return _database.InsertAsync(item);
        }
        // Método genérico para atualizar um registro em qualquer tipo de tabela
        public Task<int> UpdateAsync<T>(T item) where T : new()
        {
            return _database.UpdateAsync(item);
        }       


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