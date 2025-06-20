using CantinaV1.Models;
using CantinaV1.Services.Internals;
using Firebase.Database;
using System.Reactive.Linq;

namespace CantinaV1.Services.Externals
{
    public class FirebaseOrderWorker
    {
        private readonly GenericConfigurationServices _configService;
        private readonly FireBaseOrderServices _fireBaseOrderServices;
        private FirebaseClient _firebaseClient;
        private IDisposable _subscription;

        public event EventHandler<List<OrderToSend>> OrdersUpdated;

        public FirebaseOrderWorker()
        {
            _configService = new GenericConfigurationServices();
            _fireBaseOrderServices = new FireBaseOrderServices();
        }

        public async Task StartListeningAsync()
        {
            if (_subscription != null)
                return; // já está ouvindo

            var switchReceiveCodeApp = await _configService.GetGenericConfigurationAsync("switchReceiveCodeApp");
            if (switchReceiveCodeApp == null || !bool.TryParse(switchReceiveCodeApp.Value, out bool isEnabled) || !isEnabled)
                return;

            string? firebaseUrl = await _configService.GetGenericConfigurationAsync("FirebaseAuthDomain") is { } config && !string.IsNullOrEmpty(config.Value)
                ? config.Value
                : null;

            string? orderKey = await _configService.GetGenericConfigurationAsync("entryRegisterCodeApp") is { } key && !string.IsNullOrEmpty(key.Value)
                ? key.Value
                : null;

            if (string.IsNullOrEmpty(firebaseUrl) || string.IsNullOrEmpty(orderKey))
                return;

            _firebaseClient = new FirebaseClient(firebaseUrl);

            _subscription = _firebaseClient
                            .Child(orderKey)
                            .AsObservable<OrderToSend>()
                            .Where(f => f.EventType == Firebase.Database.Streaming.FirebaseEventType.InsertOrUpdate)
                            .Subscribe(async data =>
                            {
                                if (data.Object != null)
                                {
                                    var allOrders = await _fireBaseOrderServices.GetOrdersByOrderKeyAsync(orderKey);
                                    OrdersUpdated?.Invoke(this, allOrders);
                                }
                            });
        }

        public void StopListening()
        {
            _subscription?.Dispose();
            _subscription = null; // Permite nova inscrição no futuro
        }
    }
}
