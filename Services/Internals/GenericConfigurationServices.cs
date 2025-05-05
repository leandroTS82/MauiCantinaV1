using CantinaV1.Data;
using CantinaV1.Models;

namespace CantinaV1.Services.Internals
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

        internal async Task<bool> DisableExpiredCodeAppFeatures()
        {
            var disabled = false;
            var codeAppLimit = await GetGenericConfigurationAsync("CodeAppLimit");
            if (DateTime.TryParse(codeAppLimit?.Value, out var codeAppLimitDate) && codeAppLimitDate < DateTime.Now)
            {
                await SaveOrUpdateGenericConfiguration("switchSendCodeApp", "false");
                await SaveOrUpdateGenericConfiguration("switchReceiveCodeApp", "false");
                await SaveOrUpdateGenericConfiguration("CodeAppLimit", string.Empty);
                await SaveOrUpdateGenericConfiguration("CodeApp", string.Empty);
                await SaveOrUpdateGenericConfiguration("entryRegisterCodeApp", string.Empty);

                disabled = true;
            }
            return disabled;
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
