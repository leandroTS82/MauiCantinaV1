using CantinaV1.Models;
using CantinaV1.Data;

namespace CantinaV1
{
    public partial class MainPage : ContentPage
    {
        private Database _database;

        public MainPage()
        {
            InitializeComponent();
            string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "cantina.db3");
            _database = new Database(dbPath);
            CarregarProdutos();
        }

        private async void CarregarProdutos()
        {
            var produtos = await _database.GetProdutosAsync();
            listProdutos.ItemsSource = produtos;
            AtualizarTotal(produtos);
        }

        private async void OnAdicionarProdutoClicked(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(entryNome.Text) &&
                decimal.TryParse(entryPreco.Text, out decimal preco) &&
                int.TryParse(entryQuantidade.Text, out int quantidade))
            {
                var produto = new Produto { Nome = entryNome.Text, Preco = preco, Quantidade = quantidade };
                await _database.SaveProdutoAsync(produto);
                entryNome.Text = entryPreco.Text = entryQuantidade.Text = "";
                CarregarProdutos();
            }
        }

        private void AtualizarTotal(List<Produto> produtos)
        {
            decimal total = produtos.Sum(p => p.Total);
            labelTotal.Text = $"Total: R${total:N2}";
        }
    }
}
