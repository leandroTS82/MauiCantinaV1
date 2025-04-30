using CantinaV1.Models;
using CantinaV1.Services.Externals;
using CantinaV1.Services.Internals;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

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
            var result = await firebaseOrder.BuscarPedidosPorOrderKeyAsync("entryRegisterCodeApp");

            MainThread.BeginInvokeOnMainThread(() =>
            {
                Orders.Clear();
                foreach (var order in result)
                    Orders.Add(order);
            });
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
                        _orders = orders;
                        foreach (var order in orders)
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
