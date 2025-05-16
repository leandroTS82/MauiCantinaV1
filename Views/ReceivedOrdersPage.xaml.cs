using CantinaV1.Models;
using CantinaV1.Services.Externals;
using CantinaV1.Services.Internals;
using System.Collections.ObjectModel;

namespace CantinaV1.Views
{
    public partial class ReceivedOrdersPage : ContentPage
    {
        private readonly GenericConfigurationServices _configService;
        private FirebaseOrderWorker _firebaseWorker;
        private FireBaseOrderServices _firebaseOrderService;

        public ObservableCollection<OrderToSend> Orders { get; set; } = new ObservableCollection<OrderToSend>();
        public ObservableCollection<OrderToSend> SelectedOrders { get; set; } = new ObservableCollection<OrderToSend>();

        public ReceivedOrdersPage()
        {
            InitializeComponent();
            _configService = new GenericConfigurationServices();
            _firebaseOrderService = new FireBaseOrderServices();
            BindingContext = this;
            _ = IniciaOrders();
        }

        private async Task IniciaOrders()
        {
            var switchReceiveCodeApp = await _configService.GetGenericConfigurationAsync("switchReceiveCodeApp");
            if (switchReceiveCodeApp == null || !bool.TryParse(switchReceiveCodeApp.Value, out bool isEnabled) || !isEnabled)
                return;

            var orderKey = await _configService.GetGenericConfigurationAsync("entryRegisterCodeApp");
            if (orderKey == null || string.IsNullOrEmpty(orderKey.Value))
                return;

            var result = await _firebaseOrderService.GetOrdersByOrderKeyAsync(orderKey.Value);

            MainThread.BeginInvokeOnMainThread(() =>
            {
                UpdateOrderLists(result);
            });
        }

        private async void OnOrderTapped(object sender, TappedEventArgs e)
        {
            if (sender is Frame frame && frame.BindingContext is OrderToSend tappedOrder)
            {
                tappedOrder.IsSelected = !tappedOrder.IsSelected;

                await _firebaseOrderService.UpdateOrderIsSelectedAsync(tappedOrder);

                // Atualiza as duas listas com base no estado atual
                List<OrderToSend>? combinedList = Orders.Concat(SelectedOrders)
                                         .Where(o => o.OrderKey != tappedOrder.OrderKey)
                                         .ToList();
                combinedList.Add(tappedOrder);

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    UpdateOrderLists(combinedList);
                });
            }
        }

        private void UpdateOrderLists(IEnumerable<OrderToSend> orders)
        {
            Orders.Clear();
            SelectedOrders.Clear();

            foreach (var order in orders.OrderByDescending(o => o.Created))
            {
                if (order.IsSelected)
                    SelectedOrders.Add(order);
                else
                    Orders.Add(order);
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
                        UpdateOrderLists(orders);
#if ANDROID
                        CantinaV1.Platforms.Android.NotificationHelper.PlayNotification();
#endif
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
        }
    }
}
