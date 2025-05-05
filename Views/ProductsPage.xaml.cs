using CantinaV1.Models;
using CantinaV1.Services.Internals;

namespace CantinaV1.Views;

public partial class ProductsPage : ContentPage
{
    private readonly ProductsService _productsService;
    private List<Product> _produtos;
    public ProductsPage()
    {
        InitializeComponent();
        _productsService = new ProductsService();
        Inicializar();
    }
    private async void Inicializar()
    {
        await CarregarProdutos();
    }

    private async Task CarregarProdutos()
    {
        _produtos = await _productsService.GetAllAsync();
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
        await _productsService.SaveItemAsync(produto);

        // Limpa os campos do formulário
        entryNome.Text = string.Empty;
        entryPreco.Text = string.Empty;

        // Carrega os produtos atualizados
        await CarregarProdutos();
    }
    private async void OnLimparProdutosClicked(object sender, EventArgs e)
    {
        await _productsService.DeleteAllAsync(); // Limpa a tabela
        listProdutos.ItemsSource = await _productsService.GetAllAsync(); // Atualiza a lista

    }
}