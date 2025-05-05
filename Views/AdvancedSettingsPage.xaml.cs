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
            LoadFirebaseConfigs();
        }

        private async void LoadFirebaseConfigs()
        {
            entryFirebaseAuthDomain.Text = await GetValue("FirebaseAuthDomain");
        }

        private async Task<string> GetValue(string key)
        {
            var config = await _configService.GetGenericConfigurationAsync(key);
            return config?.Value ?? string.Empty;
        }

        private async void OnSalvarFirebaseClicked(object sender, EventArgs e)
        {
            await Salvar("FirebaseAuthDomain", entryFirebaseAuthDomain.Text);
            await DisplayAlert("Sucesso", "Configurações Firebase salvas.", "OK");
        }

        private async Task Salvar(string key, string value)
        {
            await _configService.SaveOrUpdateGenericConfiguration(key, value ?? string.Empty);
        }
    }
}
