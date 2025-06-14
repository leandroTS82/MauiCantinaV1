using CantinaV1.Models;
using CantinaV1.Services.Externals;
using CantinaV1.Services.Internals;
using ClosedXML.Excel;
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

            ImportXlsxCommand = new RelayCommand(async () => await ImportXlsxAsync());

            EndDate = DateTime.Now;
            StartDate = EndDate.AddMonths(-2);
            SelectedPaymentMethod = "Todos";

            FilterCommand = new Command(async () => await LoadData());

            Task.Run(async () => await LoadData());
        }

        private async Task ImportXlsxAsync()
        {
            try
            {
                var result = await FilePicker.PickAsync(new PickOptions
                {
                    PickerTitle = "Selecione o arquivo Excel",
                    FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.Android, new[] { "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" } },
                { DevicePlatform.WinUI, new[] { ".xlsx" } },
                { DevicePlatform.iOS, new[] { "com.microsoft.excel.xlsx" } }, // opcional para iOS
                { DevicePlatform.MacCatalyst, new[] { "com.microsoft.excel.xlsx" } } // opcional para MacCatalyst
            })
                });

                if (result != null)
                {
                    using var stream = await result.OpenReadAsync();
                    using var workbook = new XLWorkbook(stream);
                    var worksheet = workbook.Worksheet(1);
                    var rows = worksheet.RowsUsed().Skip(1); // pular cabeçalho

                    foreach (var row in rows)
                    {
                        var order = new OrderHistory
                        {
                            Date = DateTime.Parse(row.Cell(1).GetString()),
                            ClientName = row.Cell(2).GetString(),
                            ProductName = row.Cell(3).GetString(),
                            Price = decimal.Parse(row.Cell(4).GetString().Replace("R$", "").Trim()),
                            Quantity = int.Parse(row.Cell(5).GetString()),
                            Total = decimal.Parse(row.Cell(6).GetString().Replace("R$", "").Trim()),
                            PaymentMethod = row.Cell(7).GetString(),
                            Observation = row.Cell(8).GetString()
                        };

                        await _ordersService.SaveItemAsync(order);
                    }

                    await Shell.Current.DisplayAlert("Sucesso", "Importação concluída.", "OK");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Erro", $"Falha na importação: {ex.Message}", "OK");
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