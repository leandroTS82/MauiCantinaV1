using CantinaV1.Data;
using CantinaV1.Models;

namespace CantinaV1.Services.Internals
{
    public class ContactsService
    {
        private readonly Database _database;
        public ContactsService()
        {
            string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
                "cantina.db4");
            _database = new Database(dbPath);
        }

        public async Task<List<ContactModel>> GetAllAsync() => await _database.GetAllAsync<ContactModel>();

        public async Task SaveItemAsync(ContactModel item)
        {
            if (item.Id == 0)
                await _database.InsertAsync(item);
            else
                await _database.UpdateAsync(item);
        }

        public async Task DeleteAllAsync() => await _database.DeleteAllAsync<ContactModel>();

        internal async Task UpdateAsync(ContactModel item)
        {
            throw new NotImplementedException();
        }

        internal async Task DeleteAsync(ContactModel contact)
        {
            var contacts = await _database.GetAllAsync<ContactModel>();
            if (contacts.Contains(contact))
            {
                await _database.DeleteAsync(contact);
            }
            else
            {
                throw new InvalidOperationException("Contact not found in the database.");
            }
        }
    }
}