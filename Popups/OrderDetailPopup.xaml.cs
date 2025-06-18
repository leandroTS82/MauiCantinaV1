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

        if (firstOrder?.Status != "Pago")
        {
            ActionButtons.IsVisible = true;
        }
    }

    private async void OnSendChargeClicked(object sender, EventArgs e)
    {
        try
        {
            var whatsappService = new WhatsAppService();
            string message = "Olá! Há um pagamento pendente na Cantina. Por favor, verifique.";
            await whatsappService.SendMessage("5511987823588", message);
            FeedbackLabel.Text = "Mensagem de cobrança enviada com sucesso!";
            FeedbackLabel.TextColor = Colors.Green;
            FeedbackLabel.IsVisible = true;
        }
        catch (Exception ex)
        {
            FeedbackLabel.Text = "Erro ao enviar mensagem: " + ex.Message;
            FeedbackLabel.TextColor = Colors.Red;
            FeedbackLabel.IsVisible = true;
        }
    }

    private async void OnRegisterPaymentClicked(object sender, EventArgs e)
    {
        try
        {
            foreach (var order in _orderList)
            {
                order.Status = "Pago";
                order.PaymentDate = DateTime.Now;

                var orderHistoryService = new OrderHistoryService();
                await orderHistoryService.UpdateAsync(order);
            }

            FeedbackLabel.Text = "Pagamento registrado com sucesso!";
            FeedbackLabel.TextColor = Colors.Green;
            FeedbackLabel.IsVisible = true;

            await Task.Delay(2000); // tempo para usuário ler a mensagem
            Close(true); // fecha o popup e avisa a page que houve alteração
        }
        catch (Exception ex)
        {
            FeedbackLabel.Text = "Erro ao registrar pagamento: " + ex.Message;
            FeedbackLabel.TextColor = Colors.Red;
            FeedbackLabel.IsVisible = true;
        }
    }

    private void OnCloseClicked(object sender, EventArgs e)
    {
        Close();
    }
}
