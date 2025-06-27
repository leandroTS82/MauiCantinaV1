using CommunityToolkit.Maui.Views;
using CantinaV1.Models;

namespace CantinaV1.Popups
{
    public partial class ExportOptionsPopup : Popup
    {
        private readonly List<OrderItem> _orders;
        private readonly List<Product> _products;
        private readonly List<OrderHistory> _orderHistory;
        private readonly Func<List<OrderItem>, Task> _exportXlsxAsync;
        private readonly Func<List<OrderItem>, List<Product>, Task> _copyReportTextAsync;
        private readonly Func<List<OrderHistory>, Task> _exportHistoryXlsxAsync;
        private readonly Func<List<OrderHistory>, Task> _copyHistoryTextAsync;
        private readonly bool _isHistory = false;

        public ExportOptionsPopup(List<OrderItem> orders, List<Product> products,
                                  Func<List<OrderItem>, Task> exportXlsxAsync,
                                  Func<List<OrderItem>, List<Product>, Task> copyReportTextAsync)
        {
            InitializeComponent();
            _orders = orders;
            _products = products;
            _exportXlsxAsync = exportXlsxAsync;
            _copyReportTextAsync = copyReportTextAsync;
        }
        public ExportOptionsPopup(List<OrderHistory> orderHistory,
                          Func<List<OrderHistory>, Task> exportXlsxAsync,
                          Func<List<OrderHistory>, Task> copyTextAsync)
        {
            InitializeComponent();
            _orderHistory = orderHistory;
            _exportHistoryXlsxAsync = exportXlsxAsync;
            _copyHistoryTextAsync = copyTextAsync;
            _isHistory = true;
        }

        private async void OnExportSpreadsheetClicked(object sender, EventArgs e)
        {
            Close();

            if (_isHistory)
                await _exportHistoryXlsxAsync(_orderHistory);
            else
                await _exportXlsxAsync(_orders);
        }

        private async void OnCopyTextClicked(object sender, EventArgs e)
        {
            Close();

            if (_isHistory)
                await _copyHistoryTextAsync(_orderHistory);
            else
                await _copyReportTextAsync(_orders, _products);
        }

        private void OnCancelClicked(object sender, EventArgs e)
        {
            Close();
        }
    }
}
