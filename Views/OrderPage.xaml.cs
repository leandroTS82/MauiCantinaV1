using CantinaV1.Data;
using CantinaV1.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CantinaV1.Views;

public partial class OrderPage : ContentPage, INotifyPropertyChanged
{
    private readonly Database _database;
    private List<Product> _product;
    private string _clientName;
    private string _paymentMethod;
    private List<OrderItem> _orderItems;
    // Lista para armazenar os pedidos
    private ObservableCollection<OrderItem> _savedOrders = new ObservableCollection<OrderItem>();


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
        // Vincula a lista de pedidos ao ListView
        OrdersListView.ItemsSource = _savedOrders;

        Inicializar();
    }

    private async void Inicializar()
    {
        await LoadOrderItemProducts();
        await LoadSavedOrders(); // Carrega os pedidos salvos
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
                ClientName = string.Empty,
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

    private async void OnSaveOrderClicked(object sender, EventArgs e)
    {
        // Verificar se os campos obrigatórios estão preenchidos
        if (string.IsNullOrEmpty(ClientName) || string.IsNullOrEmpty(PaymentMethod))
        {
            await DisplayAlert("Erro", "O nome do cliente e o método de pagamento são obrigatórios.", "OK");
            return;
        }

        // Criar uma lista de OrderItem com os produtos que possuem quantidade > 0
        var orderItemsToSave = _orderItems.Where(item => item.Quantity > 0).ToList();

        if (orderItemsToSave.Any())
        {
            // Salvar cada item no banco de dados
            foreach (var item in orderItemsToSave)
            {
                item.ClientName = ClientName;
                item.PaymentMethod = PaymentMethod;

                await _database.SavePedidoAsync(item);
            }

            // Mostrar uma mensagem de confirmação para o usuário
            await DisplayAlert("Pedido", "Pedido salvo com sucesso!", "OK");

            Inicializar();
        }
        else
        {
            await DisplayAlert("Erro", "Por favor, adicione produtos ao pedido.", "OK");
        }
    }

    // Adicionando o carregamento de pedidos salvos na página.
    private async Task LoadSavedOrders()
    {
        var savedOrders = await _database.GetPedidosAsync(); // Recupera os pedidos do banco de dados
        _savedOrders.Clear(); // Limpa os pedidos antigos
        foreach (var order in savedOrders)
        {
            _savedOrders.Add(order); // Adiciona os pedidos recuperados à lista observada
        }
    }

    private async void OnCardTapped(object sender, EventArgs e)
    {
        var frame = (Frame)sender;
        var clientName = (string)frame.BindingContext; // Recebe o ClientName a partir do BindingContext

        // Navega para a página de detalhes, passando o nome do cliente como parâmetro
        await Navigation.PushAsync(new OrderDetailPage(clientName));
    }


    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

