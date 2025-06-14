using CantinaV1.Models;
using CantinaV1.Services.Internals;
using System.Collections.ObjectModel;

namespace CantinaV1.ViewModels
{
    public class OrderHistoryViewModel
    {
        private readonly OrderHistoryService _historyService;

        public ObservableCollection<OrderHistoryGroup> GroupedOrderHistories { get; set; }

        public OrderHistoryViewModel()
        {
            _historyService = new OrderHistoryService();
            GroupedOrderHistories = new ObservableCollection<OrderHistoryGroup>();
            LoadData();
        }

        private async void LoadData()
        {
            var allItems = await _historyService.GetAllAsync();

            // Agrupar por Data, Cliente, Forma de Pagamento
            var grouped = allItems
                .GroupBy(item => new { Date = item.Date.Date, item.ClientName, item.PaymentMethod })
                .Select(g => new OrderHistory
                {
                    Date = g.Key.Date,
                    ClientName = g.Key.ClientName,
                    PaymentMethod = g.Key.PaymentMethod,
                    Total = g.Sum(x => x.Total)
                })
                .OrderByDescending(x => x.Date) // Data mais recente primeiro
                .ThenBy(x => x.ClientName)
                .ToList();

            // Subgrupar por Data para exibir na CollectionView agrupada
            var finalGroups = grouped
                .GroupBy(item => item.Date)
                .OrderByDescending(g => g.Key) // Data mais recente primeiro
                .Select(g => new OrderHistoryGroup(g.Key.ToString("dd/MM/yyyy"), g))
                .ToList();

            GroupedOrderHistories.Clear();
            foreach (var group in finalGroups)
                GroupedOrderHistories.Add(group);
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
