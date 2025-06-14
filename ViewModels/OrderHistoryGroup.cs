using CantinaV1.Models;
using CantinaV1.Services.Internals;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace CantinaV1.ViewModels
{
    public class OrderHistoryViewModel : BindableObject
    {
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

        public ICommand FilterCommand { get; }

        public OrderHistoryViewModel()
        {
            _historyService = new OrderHistoryService();
            GroupedOrderHistories = new ObservableCollection<OrderHistoryGroup>();

            EndDate = DateTime.Now;
            StartDate = EndDate.AddMonths(-2);
            SelectedPaymentMethod = "Todos";

            FilterCommand = new Command(async () => await LoadData());

            Task.Run(async () => await LoadData());
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

            var grouped = filtered
                .GroupBy(item => new { Date = item.Date.Date, item.ClientName, item.PaymentMethod })
                .Select(g => new OrderHistory
                {
                    Date = g.Key.Date,
                    ClientName = g.Key.ClientName,
                    PaymentMethod = g.Key.PaymentMethod,
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