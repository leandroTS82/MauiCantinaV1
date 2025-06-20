using CantinaV1.Models;
using CantinaV1.Services.Internals;
using System.Collections.ObjectModel;

namespace CantinaV1.ViewModels
{
    public class PendingOrdersViewModel : BindableObject
    {
        private readonly OrderHistoryService _historyService;
        public ObservableCollection<OrderHistoryGroup> GroupedPendingOrders { get; set; }

        public PendingOrdersViewModel()
        {
            _historyService = new OrderHistoryService();
            GroupedPendingOrders = new ObservableCollection<OrderHistoryGroup>();

            Task.Run(async () => await LoadPendingOrders());
        }

        private async Task LoadPendingOrders()
        {
            var allItems = await _historyService.GetAllAsync();

            var pendingItems = allItems
                .Where(item => item.Status == "Pendente")
                .GroupBy(item => item.ClientName)
                .Select(g => new OrderHistory
                {
                    ClientName = g.Key,
                    Total = g.Sum(x => x.Total),
                    Status = "Pendente",
                    Date = g.First().Date,
                    PaymentMethod = g.First().PaymentMethod
                })
                .OrderBy(x => x.ClientName)
                .ToList();

            var finalGroups = pendingItems
                .GroupBy(item => item.ClientName)
                .Select(g => new OrderHistoryGroup(g.Key, g))
                .ToList();

            MainThread.BeginInvokeOnMainThread(() =>
            {
                GroupedPendingOrders.Clear();
                foreach (var group in finalGroups)
                    GroupedPendingOrders.Add(group);
            });
        }
    }
}
