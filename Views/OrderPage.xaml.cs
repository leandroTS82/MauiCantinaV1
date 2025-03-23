using CantinaV1.Data;
using CantinaV1.Models;
using System.Collections.Generic;

namespace CantinaV1.Views;

public partial class OrderPage : ContentPage
{
    private readonly Database _database;
    private List<OrderItem> _orderItem;
    private List<Product> _product;
    public OrderPage()
    {
        InitializeComponent();
        string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "cantina.db3");
        _database = new Database(dbPath);

        Inicializar();
    }
    private async void Inicializar()
    {
        await LoadOrderItemProducts();
    }

    private async Task LoadOrderItemProducts()
    {
        _product = await _database.GetProdutosAsync();
        List<OrderItem> productOrders = MappProductOrders(_product);
        listProdutos.ItemsSource = productOrders;
    }

    private List<OrderItem> MappProductOrders(List<Product> products)
    {
        List<OrderItem> orderItems = new List<OrderItem>();
        foreach (var product in products)
        {
            OrderItem orderItem = new OrderItem()
            {
                ProductName=product.Name,
                Price = product.Price
            };
            orderItems.Add(orderItem);
        }
        return orderItems;
    }
}