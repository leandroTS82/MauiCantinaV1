using CantinaV1.Models;
using CantinaV1.Popups;
using CantinaV1.Services.Internals;
using CommunityToolkit.Maui.Views;

namespace CantinaV1.Views
{
    public partial class PendingOrdersPage : ContentPage
    {
        private readonly OrderHistoryService _historyService;

        public PendingOrdersPage()
        {
            InitializeComponent();
            _historyService = new OrderHistoryService();
        }

        private async void OnItemTapped(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is OrderHistory selectedOrder)
            {
                var allItems = await _historyService.GetAllAsync();
                var details = allItems.Where(o => o.ClientName == selectedOrder.ClientName && o.Status == "Pendente");

                var popup = new OrderDetailPopup(details);
                this.ShowPopup(popup);
            }
        }
    }
}
