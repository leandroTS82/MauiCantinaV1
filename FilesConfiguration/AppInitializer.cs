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
            //await EnsureKeyAsync("FirebaseApiKey", "AIzaSyD09BLyB3nV_029H4Pioc9pFVQFR4VufTA");
            //await EnsureKeyAsync("FirebaseProjectId", "pedidoscantina-7b0d6");
            //await EnsureKeyAsync("FirebaseStorageBucket", "pedidoscantina-7b0d6.firebasestorage.app");
            //await EnsureKeyAsync("FirebaseMessagingSenderId", "299966173514");
            //await EnsureKeyAsync("FirebaseAppId", "1:299966173514:web:602194bd86dcef9c31f1b3");
            await EnsureKeyAsync("FirebaseAuthDomain", "https://pedidoscantina-7b0d6-default-rtdb.firebaseio.com");

            /*O trecho de código abaixo serve para garantir que as propriedades de acesso ao firebase sejam desabilitadas após o tempo limite*/
            var codeAppLimit = await _configService.GetGenericConfigurationAsync("CodeAppLimit");
            if (codeAppLimit != null)
            {
                var codeAppLimitDate = Convert.ToDateTime(codeAppLimit.Value);
                if (codeAppLimitDate < DateTime.Now)
                {
                    await _configService.SaveOrUpdateGenericConfiguration("switchSendCodeApp", "false");
                    await _configService.SaveOrUpdateGenericConfiguration("switchReceiveCodeApp", "false");
                    await _configService.SaveOrUpdateGenericConfiguration("CodeAppLimit", string.Empty);
                    await _configService.SaveOrUpdateGenericConfiguration("CodeApp", string.Empty);
                    await _configService.SaveOrUpdateGenericConfiguration("entryRegisterCodeApp", string.Empty);
                }
            }
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
