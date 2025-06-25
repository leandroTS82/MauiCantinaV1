using CantinaV1.Models;
using CantinaV1.Services.Externals;
using CantinaV1.Services.Internals;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace CantinaV1.ViewModels
{
    public class OrderHistoryViewModel : BindableObject
    {
        private readonly OrderHistoryService _ordersService = new OrderHistoryService();
        private readonly XlsxService _xlsxService = new();

        public IRelayCommand ImportXlsxCommand { get; }
        public IRelayCommand ExportXlsxCommand { get; }
        public ICommand RegisterPaymentCommand { get; }

        private readonly OrderHistoryService _historyService;
        public ObservableCollection<OrderHistoryGroup> GroupedOrderHistories { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public List<string> PaymentMethods { get; } = new List<string> { "Todos", "Dinheiro", "Pix", "Pagar depois", "Cartão" };
        private string selectedPaymentMethod;
        public string SelectedPaymentMethod
        {
            get => selectedPaymentMethod;
            set
            {
                selectedPaymentMethod = value;
                OnPropertyChanged();
            }
        }
        public List<string> StatusList { get; } = new List<string> { "Todos", "Pendente", "Pago" };

        private string selectedStatus;
        public string SelectedStatus
        {
            get => selectedStatus;
            set
            {
                selectedStatus = value;
                OnPropertyChanged();
            }
        }

        public ICommand FilterCommand { get; }

        public OrderHistoryViewModel()
        {
            _historyService = new OrderHistoryService();
            GroupedOrderHistories = new ObservableCollection<OrderHistoryGroup>();

            ImportXlsxCommand = new RelayCommand(async () => await ImportXlsxAsync());
            async Task ImportXlsxAsync()
            {
                var orders = await _xlsxService.ImportOrderHistoryXlsxAsync();

                if (orders.Count > 0)
                {
                    foreach (var order in orders)
                    {
                        await _ordersService.SaveItemAsync(order);
                    }

                    await LoadData();
                }
            }
            ExportXlsxCommand = new RelayCommand(async () => await ExportXlsxAsync());

            RegisterPaymentCommand = new RelayCommand<OrderHistory>(async (order) => await RegisterPayment(order));

            EndDate = DateTime.Now;
            StartDate = EndDate.AddMonths(-2);
            SelectedPaymentMethod = "Todos";
            SelectedStatus = "Todos";

            FilterCommand = new Command(async () => await LoadData());

            Task.Run(async () => await LoadData());
        }

        private async Task ExportXlsxAsync()
        {
            var allItems = await _historyService.GetAllAsync();

            var filtered = allItems
                .Where(item => item.Date.Date >= StartDate.Date && item.Date.Date <= EndDate.Date);

            if (SelectedPaymentMethod != "Todos")
            {
                filtered = filtered.Where(item => item.PaymentMethod == SelectedPaymentMethod);
            }

            if (SelectedStatus != "Todos")
            {
                filtered = filtered.Where(item => item.Status == SelectedStatus);
            }

            var listToExport = filtered.OrderByDescending(x => x.Date).ToList();

            await _xlsxService.ExportOrdersToXlsxAsync(orderItems: null, orderHistory: listToExport);
        }


        private async Task RegisterPayment(OrderHistory order)
        {
            if (order.PaymentMethod == "Pagar depois" && order.Status == "Pendente")
            {
                order.Status = "Pago";
                await _ordersService.UpdateAsync(order);
                await LoadData();
            }
        }

        private async Task LoadData()
        {
            var allItems = await _historyService.GetAllAsync();

            var filtered = allItems
                .Where(item => item.Date.Date >= StartDate.Date && item.Date.Date <= EndDate.Date);

            if (SelectedPaymentMethod != "Todos")
            {
                filtered = filtered.Where(item => item.PaymentMethod == SelectedPaymentMethod);
            }

            if (SelectedStatus != "Todos")
            {
                filtered = filtered.Where(item => item.Status == SelectedStatus);
            }

            var grouped = filtered
                .GroupBy(item => new { Date = item.Date.Date, item.ClientName, item.PaymentMethod })
                .Select(g => new OrderHistory
                {
                    Date = g.Key.Date,
                    ClientName = g.Key.ClientName,
                    PaymentMethod = g.Key.PaymentMethod,
                    Status = g.First().Status,
                    Total = g.Sum(x => x.Total)
                })
                .OrderByDescending(x => x.Date)
                .ThenBy(x => x.ClientName)
                .ToList();

            var finalGroups = grouped
                .GroupBy(item => item.Date)
                .OrderByDescending(g => g.Key)
                .Select(g => new OrderHistoryGroup(g.Key.ToString("dd/MM/yyyy"), g))
                .ToList();

            MainThread.BeginInvokeOnMainThread(() =>
            {
                GroupedOrderHistories.Clear();
                foreach (var group in finalGroups)
                    GroupedOrderHistories.Add(group);
            });
        }

    }

    public class OrderHistoryGroup : ObservableCollection<OrderHistory>
    {
        public string Date { get; set; }

        public OrderHistoryGroup(string date, IEnumerable<OrderHistory> items) : base(items)
        {
            Date = date;
        }
    }
}
