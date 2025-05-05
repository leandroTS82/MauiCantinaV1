using CantinaV1.Data;
using CantinaV1.Models;
using CantinaV1.Services.Internals;

namespace CantinaV1.Views;

public partial class ConfigurationPage : ContentPage
{
    private readonly Database _database;
    private readonly GenericConfigurationServices _genericConfigurationServices;
    public ConfigurationPage()
    {
        InitializeComponent();
        _genericConfigurationServices = new GenericConfigurationServices();

        string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "cantina.db4");
        _database = new Database(dbPath);

        CarregarConfiguracao();
    }

    private async void CarregarConfiguracao()
    {
        var ddd = await _genericConfigurationServices.GetGenericConfigurationAsync("entryDDD");
        if (ddd == null) return;
        entryDDD.Text = ddd.Value;

        var telefone = await _genericConfigurationServices.GetGenericConfigurationAsync("entryTelefone");
        if (telefone == null) return;
        entryTelefone.Text = telefone.Value;

        var switchWhatsHabilitado = await _genericConfigurationServices.GetGenericConfigurationAsync("switchHabilitado");
        if (switchHabilitado != null)
            switchHabilitado.IsToggled = Convert.ToBoolean(switchWhatsHabilitado.Value);       

        LoadCodeAppInit();
    }

    private async void LoadCodeAppInit()
    {

        var disabled = await _genericConfigurationServices.DisableExpiredCodeAppFeatures();
        if (disabled)
        {
            entryCodeApp.Text = $"{RandomNumber()}{RandomNumber()}";
            switchReceiveCodeApp.IsToggled = false;
            switchSendCodeApp.IsToggled = false;
            await _genericConfigurationServices.SaveOrUpdateGenericConfiguration("switchSendCodeApp", switchSendCodeApp.IsToggled.ToString());
            await _genericConfigurationServices.SaveOrUpdateGenericConfiguration("switchReceiveCodeApp", switchReceiveCodeApp.IsToggled.ToString());
        }
        else
        {
            var codeApp = await _genericConfigurationServices.GetGenericConfigurationAsync("CodeApp");
            var registerCodeApp = await _genericConfigurationServices.GetGenericConfigurationAsync("entryRegisterCodeApp");
            var switchSendCodeAppConfig = await _genericConfigurationServices.GetGenericConfigurationAsync("switchSendCodeApp");
            var switchReceiveCodeAppConfig = await _genericConfigurationServices.GetGenericConfigurationAsync("switchReceiveCodeApp");

            if (codeApp != null)
                entryCodeApp.Text = codeApp.Value;
            if (registerCodeApp != null)
                entryRegisterCodeApp.Text = registerCodeApp.Value;
            if (switchSendCodeAppConfig != null)
                switchSendCodeApp.IsToggled = Convert.ToBoolean(switchSendCodeAppConfig.Value);
            if (switchReceiveCodeAppConfig != null)
                switchReceiveCodeApp.IsToggled = Convert.ToBoolean(switchReceiveCodeAppConfig.Value);
        }           
    }

    private int RandomNumber()
    {
        Random random = new Random();
        return random.Next(100, 1000);
    }

    private void entryDDD_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (e.NewTextValue.Length > 2)
        {
            entryDDD.Text = e.NewTextValue.Substring(0, 2);
        }
    }
    private async void OnSalvarConfiguracaoClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(entryDDD.Text) || string.IsNullOrWhiteSpace(entryTelefone.Text))
        {
            await DisplayAlert("Erro", "Preencha DDD e Telefone", "OK");
            return;
        }

        await _genericConfigurationServices.SaveOrUpdateGenericConfiguration("entryDDD", entryDDD.Text);
        await _genericConfigurationServices.SaveOrUpdateGenericConfiguration("entryTelefone", entryTelefone.Text);
        await _genericConfigurationServices.SaveOrUpdateGenericConfiguration("switchHabilitado", switchHabilitado.IsToggled.ToString());

        //await _database.SaveConfiguracaoAsync(config);
        await DisplayAlert("Sucesso", "Configuração salva com sucesso!", "OK");
    }

    private void OnNewCodeAppClicked(object sender, EventArgs e)
    {
        entryCodeApp.Text = $"{RandomNumber()}{RandomNumber()}";
        switchSendCodeApp.IsToggled = false;
    }

    private async void OnSaveSendCodeAppClicked(object sender, EventArgs e)
    {
        var flagSendCode = switchSendCodeApp.IsToggled;
        var sendCode = entryCodeApp.Text;

        await _genericConfigurationServices.SaveOrUpdateGenericConfiguration("CodeAppLimit", DateTime.Now.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss"));
        await _genericConfigurationServices.SaveOrUpdateGenericConfiguration("CodeApp", sendCode);
        await _genericConfigurationServices.SaveOrUpdateGenericConfiguration("switchSendCodeApp", flagSendCode.ToString());

        await DisplayAlert("Sucesso", "Configuração salva com sucesso!", "OK");
    }

    private async void OnSaveReceiveCodeAppClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(entryRegisterCodeApp.Text))
        {
            await DisplayAlert("Erro", "Preencha o código numérico", "OK");
            return;
        }

        var flagReceiveCode = switchReceiveCodeApp.IsToggled;
        if (flagReceiveCode)
            await _genericConfigurationServices.SaveOrUpdateGenericConfiguration("entryRegisterCodeApp", entryRegisterCodeApp.Text);
        await _genericConfigurationServices.SaveOrUpdateGenericConfiguration("switchReceiveCodeApp", flagReceiveCode.ToString());

        await DisplayAlert("Sucesso", "Configuração salva com sucesso!", "OK");
    }

    private async void OnRedirectReceivedOrdersPageClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new ReceivedOrdersPage());
    }
}
