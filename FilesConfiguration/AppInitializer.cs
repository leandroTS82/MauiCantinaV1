using CantinaV1.Services.Internals;

namespace CantinaV1.FilesConfiguration
{
    public class AppInitializer
    {
        private readonly GenericConfigurationServices _configService;

        public AppInitializer()
        {
            _configService = new GenericConfigurationServices();
        }

        public async Task InitializeDefaultsAsync()
        {
            await EnsureKeyAsync("FirebaseAuthDomain", "https://pedidoscantina-7b0d6-default-rtdb.firebaseio.com");

            /*O trecho de código abaixo serve para garantir que as propriedades de acesso ao firebase sejam desabilitadas após o tempo limite*/
            await _configService.DisableExpiredCodeAppFeatures();
        }

        private async Task EnsureKeyAsync(string key, string defaultValue)
        {
            var existing = await _configService.GetGenericConfigurationAsync(key);
            if (existing == null)
            {
                await _configService.SaveOrUpdateGenericConfiguration(key, defaultValue);
            }
        }
    }

}
