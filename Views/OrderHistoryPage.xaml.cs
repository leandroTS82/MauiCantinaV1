using CantinaV1.Models;
using CantinaV1.Popups;
using CantinaV1.Services.Internals;
using CommunityToolkit.Maui.Views;

namespace CantinaV1.Views
{
    public partial class OrderHistoryPage : ContentPage
    {
        private readonly OrderHistoryService _historyService;
        public OrderHistoryPage()
        {
            InitializeComponent();
            _historyService = new OrderHistoryService();
        }
        private async void OnItemTapped(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is OrderHistory selectedOrder)
            {
                var allItems = await _historyService.GetAllAsync();
                var details = allItems.Where(o => o.Date.Date == selectedOrder.Date.Date
                                               && o.ClientName == selectedOrder.ClientName
                                               && o.PaymentMethod == selectedOrder.PaymentMethod);

                var popup = new OrderDetailPopup(details);
                this.ShowPopup(popup);
            }
        }
    }
}
