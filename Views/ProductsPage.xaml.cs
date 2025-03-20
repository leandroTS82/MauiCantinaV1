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
        await _database.InicializarProdutosAsync();
        await CarregarProdutos();
    }

    private async Task CarregarProdutos()
    {
        _produtos = await _database.GetProdutosAsync();
        listProdutos.ItemsSource = _produtos;
        AtualizarTotal();
    }

    private async void OnAdicionarProdutoClicked(object sender, EventArgs e)
    {
        foreach (var produto in _produtos)
        {
            if (produto.Quantidade > 0)
            {
                await _database.UpdateProdutoAsync(produto);
            }
        }

        await CarregarProdutos();
    }
    private async void OnLimparProdutosClicked(object sender, EventArgs e)
    {
        await _database.DeleteAllProdutosAsync(); // Limpa a tabela
        listProdutos.ItemsSource = await _database.GetProdutosAsync(); // Atualiza a lista
        labelTotal.Text = "Total: R$0,00"; // Reseta o total
    }

    private void AtualizarTotal()
    {
        decimal total = _produtos.Sum(p => p.Total);
        labelTotal.Text = $"Total: R${total:N2}";
    }
}