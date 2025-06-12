using CantinaV1.Models;
using CantinaV1.Services.Internals;

namespace CantinaV1.Views
{
    public partial class OrderHistoryPage : ContentPage
    {
        private readonly OrderHistoryService _historyService;
        public OrderHistoryPage()
        {
            InitializeComponent();
            _historyService = new OrderHistoryService();
            LoadHistory();
        }
        private async void LoadHistory()
        {
            var history = await _historyService.GetAllAsync();
            HistoryCollectionView.ItemsSource = history;
        }
    }
}
