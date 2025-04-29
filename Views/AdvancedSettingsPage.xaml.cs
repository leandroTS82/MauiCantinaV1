using CantinaV1.Services.Internals;

namespace CantinaV1.Views
{
    public partial class AdvancedSettingsPage : ContentPage
    {
        private readonly GenericConfigurationServices _configService;

        public AdvancedSettingsPage()
        {
            InitializeComponent();
            _configService = new GenericConfigurationServices();
            CarregarFirebaseConfigs();
        }

        private async void CarregarFirebaseConfigs()
        {
            entryFirebaseApiKey.Text = await ObterValor("FirebaseApiKey");
            entryFirebaseAuthDomain.Text = await ObterValor("FirebaseAuthDomain");
            entryFirebaseProjectId.Text = await ObterValor("FirebaseProjectId");
            entryFirebaseStorageBucket.Text = await ObterValor("FirebaseStorageBucket");
            entryFirebaseMessagingSenderId.Text = await ObterValor("FirebaseMessagingSenderId");
            entryFirebaseAppId.Text = await ObterValor("FirebaseAppId");
        }

        private async Task<string> ObterValor(string key)
        {
            var config = await _configService.GetGenericConfigurationAsync(key);
            return config?.Value ?? string.Empty;
        }

        private async void OnSalvarFirebaseClicked(object sender, EventArgs e)
        {
            await Salvar("FirebaseApiKey", entryFirebaseApiKey.Text);
            await Salvar("FirebaseAuthDomain", entryFirebaseAuthDomain.Text);
            await Salvar("FirebaseProjectId", entryFirebaseProjectId.Text);
            await Salvar("FirebaseStorageBucket", entryFirebaseStorageBucket.Text);
            await Salvar("FirebaseMessagingSenderId", entryFirebaseMessagingSenderId.Text);
            await Salvar("FirebaseAppId", entryFirebaseAppId.Text);

            await DisplayAlert("Sucesso", "Configurações Firebase salvas.", "OK");
        }

        private async Task Salvar(string key, string value)
        {
            await _configService.SaveOrUpdateGenericConfiguration(key, value ?? string.Empty);
        }
    }
}
