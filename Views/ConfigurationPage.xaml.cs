using CantinaV1.Data;
using CantinaV1.Models;

namespace CantinaV1.Views;

public partial class ConfigurationPage : ContentPage
{
    private readonly Database _database;

    public ConfigurationPage()
    {
        InitializeComponent();

        string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
            "cantina.db4");
        _database = new Database(dbPath);

        CarregarConfiguracao();
    }

    private async void CarregarConfiguracao()
    {
        var config = await _database.GetConfiguracaoAsync();
        if (config != null)
        {
            entryDDD.Text = config.DDD;
            entryTelefone.Text = config.Telefone;
            switchHabilitado.IsToggled = config.ReceberPedidos;
        }
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

        var config = new Configuration
        {
            DDD = entryDDD.Text,
            Telefone = entryTelefone.Text,
            ReceberPedidos = switchHabilitado.IsToggled
        };

        await _database.SaveConfiguracaoAsync(config);
        await DisplayAlert("Sucesso", "Configuração salva com sucesso!", "OK");
    }
}
