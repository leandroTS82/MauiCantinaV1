using CantinaV1.Models;
using CantinaV1.Services.Externals;

public class PollingFirebaseOrderService
{
    private readonly FireBaseOrderServices _firebaseService;
    private readonly string _orderKey;
    private CancellationTokenSource _cts;
    private List<string> _lastKnownIds = new();

    public event EventHandler<List<OrderToSend>> updatedsOrders;

    public PollingFirebaseOrderService(string orderKey)
    {
        _firebaseService = new FireBaseOrderServices();
        _orderKey = orderKey;
    }

    public void StartPolling(int intervalInSeconds = 5)
    {
        _cts = new CancellationTokenSource();

        Task.Run(async () =>
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                try
                {
                    var orders = await _firebaseService.BuscarPedidosPorOrderKeyAsync(_orderKey);

                    // Só atualiza se houve mudanças
                    var novosIds = orders.Select(p => p.Id).ToList();
                    if (!_lastKnownIds.SequenceEqual(novosIds))
                    {
                        _lastKnownIds = novosIds;
                        updatedsOrders?.Invoke(this, orders);
                    }
                }
                catch (Exception ex)
                {
                    // logar erro se quiser
                }

                await Task.Delay(TimeSpan.FromSeconds(intervalInSeconds));
            }
        }, _cts.Token);
    }

    public void StopPolling()
    {
        _cts?.Cancel();
    }
}

/*
 O código abaixo é um modelo de como utilizar o PollingFirebaseOrderService
adicione esse código no OnAppearing do seu ....xaml.cs
 */

//protected override async void OnAppearing()
//{
//    base.OnAppearing();

//    var switchReceiveCodeApp = await _configService.GetGenericConfigurationAsync("switchReceiveCodeApp");
//    if (switchReceiveCodeApp == null || !bool.TryParse(switchReceiveCodeApp.Value, out bool isEnabled) || !isEnabled)
//        return;

//    var orderKey = await _configService.GetGenericConfigurationAsync("entryRegisterCodeApp") is { } key && !string.IsNullOrEmpty(key.Value)
//        ? key.Value
//        : null;

//    if (string.IsNullOrEmpty(orderKey))
//        return;

//    _pollingService = new PollingFirebaseOrderService(orderKey);
//    _pollingService.updatedsOrders += (sender, orders) =>
//    {
//        if (orders == null) return;

//        MainThread.BeginInvokeOnMainThread(() =>
//        {
//            Orders.Clear();
//            foreach (var order in orders)
//                Orders.Add(order);
//        });
//    };
//    _pollingService.StartPolling(5);
//}