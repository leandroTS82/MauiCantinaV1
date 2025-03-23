using CantinaV1.Data;
using CantinaV1.Models;

namespace CantinaV1.Views;

public partial class ProductsPage : ContentPage
{
    private readonly Database _database;
    private List<Product> _produtos;
    public ProductsPage()
    {
        InitializeComponent();
        string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
            "cantina.db3");
        _database = new Database(dbPath);
        Inicializar();
    }
    private async void Inicializar()
    {
        await CarregarProdutos();
    }

    private async Task CarregarProdutos()
    {
        _produtos = await _database.GetProdutosAsync();
        listProdutos.ItemsSource = _produtos;
    }

    private async void OnSalvarProdutoClicked(object sender, EventArgs e)
    {
        // Pega os valores dos campos de entrada
        string nomeProduto = entryNome.Text;
        string precoProduto = entryPreco.Text;

        // Valida se os campos não estão vazios
        if (string.IsNullOrEmpty(nomeProduto) || string.IsNullOrEmpty(precoProduto))
        {
            // Aqui você pode mostrar uma mensagem de erro para o usuário, caso necessário
            await DisplayAlert("Erro", "Preencha todos os campos", "OK");
            return;
        }

        // Cria um novo objeto Product
        var produto = new Product
        {
            Name = nomeProduto,
            Price = decimal.Parse(precoProduto) // Converte o preço para decimal
        };

        // Salva o produto no banco de dados
        await _database.SaveProdutoAsync(produto);

        // Limpa os campos do formulário
        entryNome.Text = string.Empty;
        entryPreco.Text = string.Empty;

        // Carrega os produtos atualizados
        await CarregarProdutos();
    }
    private async void OnLimparProdutosClicked(object sender, EventArgs e)
    {
        await _database.DeleteAllProdutosAsync(); // Limpa a tabela
        listProdutos.ItemsSource = await _database.GetProdutosAsync(); // Atualiza a lista

    }
}