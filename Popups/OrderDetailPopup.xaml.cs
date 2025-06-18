using CantinaV1.Models;
using CantinaV1.Services.Externals;
using CantinaV1.Services.Internals;
using CommunityToolkit.Maui.Views;


namespace CantinaV1.Popups;

public partial class OrderDetailPopup : Popup
{
    private readonly List<OrderHistory> _orderList;

    public OrderDetailPopup(IEnumerable<OrderHistory> orderHistories)
    {
        InitializeComponent();

        _orderList = orderHistories.ToList();
        var firstOrder = _orderList.FirstOrDefault();

        BindingContext = new
        {
            ClientName = firstOrder?.ClientName,
            Date = firstOrder?.Date,
            PaymentMethod = firstOrder?.PaymentMethod,
            Observation = firstOrder?.Observation,
            Status = firstOrder?.Status,
            PaymentDate = firstOrder?.PaymentDate,
            Orders = _orderList,
            TotalSum = _orderList.Sum(o => o.Total)
        };

        // Show action buttons only if PaymentMethod is "Pagar Depois"
        if (firstOrder?.PaymentMethod == "Pagar depois")
        {
            ActionButtons.IsVisible = true;
        }
    }

    private async void OnSendChargeClicked(object sender, EventArgs e)
    {
        var whatsappService = new WhatsAppService();
        string message = "Olá! Há um pagamento pendente na Cantina. Por favor, verifique."; // generic message
        await whatsappService.SendMessage("5511987823588", message); // temporary number
        await Shell.Current.DisplayAlert("WhatsApp", "Charge message sent.", "OK");
    }

    private async void OnRegisterPaymentClicked(object sender, EventArgs e)
    {
        foreach (var order in _orderList)
        {
            order.Status = "Pago";
            order.PaymentDate = DateTime.Now;
            order.PaymentMethod = "Pix";

            var orderHistoryService = new OrderHistoryService();
            await orderHistoryService.UpdateAsync(order);
        }
        Close(true);
    }

    private void OnCloseClicked(object sender, EventArgs e)
    {
        Close();
    }
}
