using CantinaV1.Data;
using CantinaV1.Models;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.IO;

namespace CantinaV1.Views;

public partial class OrderPage : ContentPage, INotifyPropertyChanged
{
    private readonly Database _database;
    private List<Product> _product;
    private string _clientName;
    private string _paymentMethod;
    private List<OrderItem> _orderItems;

    public string ClientName
    {
        get => _clientName;
        set
        {
            _clientName = value;
            OnPropertyChanged();
        }
    }

    public bool IsDinheiro
    {
        get => _paymentMethod == "Dinheiro";
        set { if (value) PaymentMethod = "Dinheiro"; }
    }

    public bool IsPix
    {
        get => _paymentMethod == "Pix";
        set { if (value) PaymentMethod = "Pix"; }
    }

    public bool IsPagarDepois
    {
        get => _paymentMethod == "Pagar depois";
        set { if (value) PaymentMethod = "Pagar depois"; }
    }

    public bool IsCartao
    {
        get => _paymentMethod == "Cartão";
        set { if (value) PaymentMethod = "Cartão"; }
    }

    public string PaymentMethod
    {
        get => _paymentMethod;
        set
        {
            _paymentMethod = value;
            OnPropertyChanged();
        }
    }

    public OrderPage()
    {
        InitializeComponent();
        BindingContext = this;

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
        _orderItems = MappProductOrders(_product);
        listProdutos.ItemsSource = _orderItems;

        // Calcular o total do pedido quando os produtos forem carregados
        UpdateTotalPedido();
    }

    private List<OrderItem> MappProductOrders(List<Product> products)
    {
        List<OrderItem> orderItems = new List<OrderItem>();
        foreach (var product in products)
        {
            OrderItem orderItem = new OrderItem()
            {
                ProductName = product.Name,
                Price = product.Price,
                Quantity = 0
            };

            // Associar o evento de alteração de quantidade
            orderItem.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(OrderItem.Quantity))
                {
                    UpdateTotalPedido();
                }
            };

            orderItems.Add(orderItem);
        }
        return orderItems;
    }

    private void UpdateTotalPedido()
    {
        decimal totalPedido = 0;
        foreach (var item in _orderItems)
        {
            totalPedido += item.Total;
        }

        // Atualizar o texto do label com o total do pedido formatado
        labelTotalPedido.Text = $"Total do Pedido: R${totalPedido:N2}";
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
