using CantinaV1.Data;
using CantinaV1.Models;

namespace CantinaV1.Services
{
    public class GenericConfigurationServices
    {
        private readonly Database _database;

        public GenericConfigurationServices()
        {
            string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "cantina.db4");
            _database = new Database(dbPath);
        }

        internal async Task<GenericConfiguration?> GetGenericConfigurationAsync(string key)
        {
            var genericConfig = await _database.GetGenericConfigurationAsync();
            return genericConfig.FirstOrDefault(x => x.Key == key);
        }
        internal async Task SaveOrUpdateGenericConfiguration(string key, string value)
        {
            var config = await GetGenericConfigurationAsync(key);
            if (config != null)
            {
                config.Value = value;
                await _database.UpdateGenericConfigurationoAsync(config);
                return;
            }
            else
            {
                var newConfig = new GenericConfiguration
                {
                    Key = key,
                    Value = value
                };
                await _database.SaveGenericConfigurationAsync(newConfig);
            }
        }

    }
}
