using CantinaV1.Models;
using CantinaV1.Services.Externals;
using CantinaV1.Services.Internals;
using System.Collections.ObjectModel;

namespace CantinaV1.Views
{
    public partial class ReceivedOrdersPage : ContentPage
    {
        private PollingFirebaseOrderService _pollingService;
        private readonly GenericConfigurationServices _configService;


        private FirebaseOrderWorker _firebaseWorker;
        List<OrderToSend> _orders = new List<OrderToSend>();
        public ObservableCollection<OrderToSend> Orders { get; set; } = new ObservableCollection<OrderToSend>();

        public ReceivedOrdersPage()
        {
            InitializeComponent();
            _configService = new GenericConfigurationServices();
            BindingContext = this;
            IniciaOrders();
        }

        private async Task IniciaOrders()
        {
            var firebaseOrder = new FireBaseOrderServices();
            var switchReceiveCodeApp = await _configService.GetGenericConfigurationAsync("switchReceiveCodeApp");
            if (switchReceiveCodeApp == null || !bool.TryParse(switchReceiveCodeApp.Value, out bool isEnabled) || !isEnabled)
                return;
            var orderKey = await _configService.GetGenericConfigurationAsync("entryRegisterCodeApp");
            if (orderKey == null || string.IsNullOrEmpty(orderKey.Value))
                return;
            var result = await firebaseOrder.BuscarPedidosPorOrderKeyAsync(orderKey.Value);

            MainThread.BeginInvokeOnMainThread(() =>
            {
                Orders.Clear();
                foreach (var order in result)
                    Orders.Add(order);
            });
        }

        private void OnOrderTapped(object sender, TappedEventArgs e)
        {
            if (sender is Frame frame && frame.BindingContext is OrderToSend tappedOrder)
            {
                // Alterna o estado
                tappedOrder.IsSelected = !tappedOrder.IsSelected;

                // Remove da posição atual
                Orders.Remove(tappedOrder);

                // Adiciona no topo ou fim
                if (tappedOrder.IsSelected)
                    Orders.Insert(0, tappedOrder); // Verde => vai pro topo
                else
                    Orders.Add(tappedOrder); // Amarelo => vai pro fim
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            _firebaseWorker = new FirebaseOrderWorker();
            _firebaseWorker.OrdersUpdated += (sender, orders) =>
            {
                if (orders == null) return;

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    try
                    {
                        Orders.Clear();
                        // Inverte a lista para mostrar o último pedido no topo
                        foreach (var order in orders.AsEnumerable().Reverse())
                            Orders.Add(order);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Erro no UI Thread: " + ex.Message);
                    }
                });
            };

            await _firebaseWorker.StartListeningAsync();
        }       

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _pollingService?.StopPolling();
        }
    }
}
