using CantinaV1.Data;
using CantinaV1.Models;

namespace CantinaV1.Views;

public partial class OrderDetailPage : ContentPage
{
    private readonly Database _database;
    private string _clientName;

    public OrderDetailPage(string clientName)
    {
        InitializeComponent();
        string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "cantina.db4");
        _database = new Database(dbPath);
        _clientName = clientName;
        ClientNameLabel.Text = clientName;

        LoadOrderItems();
    }

    // Carregar os itens do pedido para o ClientName
    private async void LoadOrderItems()
    {
        var orders = await _database.GetOrdersByClientNameAsync(_clientName);
        
        OrderItemsListView.ItemsSource = orders;
        // Calcula a soma total dos pedidos
        decimal totalSum = orders.Sum(order => order.Total);

        // Atualiza o Label com o valor formatado
        Total.Text = $"Total: R$ {totalSum:N2}";
    }

    // Excluir todos os pedidos do cliente
    private async void OnDeleteAllOrdersClicked(object sender, EventArgs e)
    {
        var orderItems = await _database.GetOrdersByClientNameAsync(_clientName);

        foreach (var orderItem in orderItems)
        {
            await _database.DeleteOrderItemAsync(orderItem);
        }

        await DisplayAlert("Sucesso", "Todos os pedidos foram excluídos.", "OK");
        await Navigation.PushAsync(new OrderPage());
    }

    // Excluir um item específico do pedido
    private async void OnDeleteOrderItemClicked(object sender, EventArgs e)
    {
        var button = sender as Button;
        var orderItem = button?.CommandParameter as OrderItem;

        if (orderItem != null)
        {
            await _database.DeleteOrderItemAsync(orderItem);
            await DisplayAlert("Sucesso", "O item foi excluído.", "OK");
            LoadOrderItems();  // Recarrega a lista após exclusão
        }
    }
}
