using CantinaV1.Data;
using CantinaV1.Models;
using CantinaV1.Services.Internals;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

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
    private List<string> _clients = new List<string>();
    public ObservableCollection<string> FilteredClients { get; set; } = new();


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
            "cantina.db4");
        _database = new Database(dbPath);
        // Vincula a lista de pedidos ao ListView
        OrdersListView.ItemsSource = _savedOrders;

        Inicializar();
        BindingContext = this;
        LoadClientsFromJson();
    }

    private async void LoadClientsFromJson()
    {
        try
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync("clients.json");
            using var reader = new StreamReader(stream);
            var json = await reader.ReadToEndAsync();
            _clients = JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", "Erro ao carregar clientes: " + ex.Message, "OK");
        }
    }

    private void OnClientNameTextChanged(object sender, TextChangedEventArgs e)
    {
        string query = e.NewTextValue?.ToLower() ?? string.Empty;
        FilteredClients.Clear();

        if (!string.IsNullOrEmpty(query))
        {
            var matches = _clients.Where(name => name.ToLower().Contains(query)).ToList();
            foreach (var match in matches)
                FilteredClients.Add(match);

            listSuggestions.IsVisible = FilteredClients.Count > 0;
        }
        else
        {
            listSuggestions.IsVisible = false;
        }
    }

    private void OnSuggestionSelected(object sender, SelectedItemChangedEventArgs e)
    {
        if (e.SelectedItem is string selectedClient)
        {
            entryClientName.Text = selectedClient;
            listSuggestions.IsVisible = false;

            // Limpar seleção para evitar que o mesmo item fique selecionado
            listSuggestions.SelectedItem = null;
        }
    }

    private async void OnClearOrdersClicked(object sender, EventArgs e)
    {
        var orderItems = await _database.GetPedidosAsync();

        foreach (var orderItem in orderItems)
        {
            await _database.DeleteOrderItemAsync(orderItem);
        }

        await DisplayAlert("Sucesso", "Todos os pedidos foram excluídos.", "OK");
        Inicializar();
    }
    private async void Inicializar()
    {
        await LoadOrderItemProducts();
        await LoadSavedOrders(); // Carrega os pedidos salvos
        entryClientName.Text = string.Empty;
        UnCheckRadiosPaymentMethod();

    }

    private void UnCheckRadiosPaymentMethod()
    {
        // Desmarca os RadioButtons
        radioDinheiro.IsChecked = false;
        radioPix.IsChecked = false;
        radioPagarDepois.IsChecked = false;
        radioCartao.IsChecked = false;
    }

    private async void OnExportClicked(object sender, EventArgs e)
    {
        try
        {
            string action = await DisplayActionSheet("📤 Como deseja exportar o relatório?", "❌ Cancelar", null,
                                         "📊 Relatório em Planilha",
                                         "📋 Copiar relatório em texto");


            var orders = await _database.GetPedidosAsync();
            var products = await _database.GetProdutosAsync();

            switch (action)
            {
                case "📊 Relatório em Planilha":
                    await ExportXlsxAsync(orders);
                    break;
                case "📋 Copiar relatório em texto":
                    await CopyReportTextAsync(orders, products);
                    break;
                case "❌ Cancelar":
                default:
                    break;
            }

        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro ao exportar CSV: {ex.Message}", "OK");
        }
    }

    private async Task CopyReportTextAsync(List<OrderItem> orders, List<Product> products)
    {
        CopyContentService copyContentService = new CopyContentService();
        ResponseModel response = await copyContentService.CopyReportTextAsync(orders, products);
        if (response.StatusCode == 200)
        {
            await DisplayAlert("Sucesso", response.Message, "OK");
        }
        else
            await DisplayAlert("Aviso", response.Message, "OK");
    }    

    private async Task ExportXlsxAsync(List<OrderItem> orders)
    {
        XlsxService exportXlsxService = new XlsxService();
        var response = await exportXlsxService.ExportOrdersToXlsxAsync(orders);
        if (response.StatusCode == 200)
        {
            await DisplayAlert("Sucesso", response.Message, "OK");
        }
        else
            await DisplayAlert("Aviso", response.Message, "OK");
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
            WhatsAppService whatsAppService = new WhatsAppService();
            await whatsAppService.SendOrderAsync(orderItemsToSave);

            // Mostrar uma mensagem de confirmação para o usuário
            await DisplayAlert("Pedido", "Pedido salvo com sucesso!", "OK");

            Inicializar();
        }
        else
        {
            await DisplayAlert("Erro", "Por favor, adicione produtos ao pedido.", "OK");
        }
    }

    private async Task LoadSavedOrders()
    {
        var savedOrders = await _database.GetPedidosAsync(); // Recupera os pedidos do banco de dados

        // Agrupar os pedidos por ClientName e somar os Totais
        var groupedOrders = savedOrders
            .GroupBy(order => order.ClientName)
            .Select(group => new
            {
                ClientName = group.Key,
                PaymentMethod = group.First().PaymentMethod,
                TotalSum = group.Sum(order => order.Total)
            })
            .ToList();

        foreach (var groupedOrder in groupedOrders)
        {
            // Verifica se o pedido já existe na lista
            var existingOrder = _savedOrders.FirstOrDefault(o => o.ClientName == groupedOrder.ClientName);

            if (existingOrder == null)
            {
                // Adiciona novo pedido se não existir
                _savedOrders.Add(new OrderItem
                {
                    ClientName = groupedOrder.ClientName,
                    PaymentMethod = groupedOrder.PaymentMethod,
                    TotalSum = groupedOrder.TotalSum
                });
            }
            else
            {
                // Atualiza o TotalSum se o pedido já existir
                existingOrder.TotalSum = groupedOrder.TotalSum;
            }
        }
    }


    private async void OnCardTapped(object sender, EventArgs e)
    {
        var frame = sender as Frame;

        if (frame != null)
        {
            var order = frame.BindingContext as OrderItem; // Obter o objeto Order do BindingContext

            if (order != null)
            {
                // Navegação para a página de detalhes do pedido
                await Navigation.PushAsync(new OrderDetailPage(order.ClientName));
            }
        }
    }


    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

