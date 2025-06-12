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
            AddItemsTemp();
            LoadHistory();
        }

        private async void AddItemsTemp()
        {
            // Limpa os itens existentes antes de adicionar novos
           // await _historyService.DeleteAllAsync();
            // Adiciona itens temporários para teste
            var historyItems = new List<OrderHistory>{
                                                        new OrderHistory
                                                        {
                                                            Date = DateTime.Now,
                                                            ClientName = "Diogo Souza - Louvor",
                                                            ProductName = "Mistinho",
                                                            Price = 6.00m,
                                                            Quantity = 1,
                                                            Total = 6.00m,
                                                            PaymentMethod = "Pagar depois",
                                                            Observation = ""
                                                        },
                                                        new OrderHistory
                                                        {
                                                            Date = DateTime.Now,
                                                            ClientName = "Maiara",
                                                            ProductName = "Bolo de Prestígio",
                                                            Price = 5.00m,
                                                            Quantity = 1,
                                                            Total = 5.00m,
                                                            PaymentMethod = "Pix",
                                                            Observation = ""
                                                        }
                                                    };
            foreach (var item in historyItems)
            {
                await _historyService.SaveItemAsync(item); 
            }
        }

        private async void LoadHistory()
        {
            var history = await _historyService.GetAllAsync();
            HistoryCollectionView.ItemsSource = history;
        }
    }
}
