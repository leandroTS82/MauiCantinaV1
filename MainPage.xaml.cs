using CantinaV1.Models;
using CantinaV1.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace CantinaV1
{
    public partial class MainPage : ContentPage
    {
        private readonly Database _database;
        private List<Produto> _produtos;

        public MainPage()
        {
            InitializeComponent();
            string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "cantina.db3");
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

        private void AtualizarTotal()
        {
            decimal total = _produtos.Sum(p => p.Total);
            labelTotal.Text = $"Total: R${total:N2}";
        }
    }
}
