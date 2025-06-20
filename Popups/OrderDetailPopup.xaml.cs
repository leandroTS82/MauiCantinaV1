using AndroidX.Annotations;
using CantinaV1.Models;
using CantinaV1.Services.Externals;
using CantinaV1.Services.Internals;
using CommunityToolkit.Maui.Views;

namespace CantinaV1.Popups;

public partial class OrderDetailPopup : Popup
{
    private readonly List<OrderHistory> _orderList;
    private GenericConfiguration? _dateConfig;
    private GenericConfiguration? _pixConfig;
    private GenericConfiguration? _messageConfig;

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
            var contactsService = new ContactsService();
            WhatsappMessageConfigModel? whatsChargeConfig = await whatsappService.LoadWhatsAppChargeConfiguration();

            if (whatsChargeConfig == null)
            {
                FeedbackLabel.Text = "Configuração de cobrança não encontrada.";
                FeedbackLabel.TextColor = Colors.Red;
                FeedbackLabel.IsVisible = true;
                return;
            }
            if (string.IsNullOrWhiteSpace(whatsChargeConfig.Message) || string.IsNullOrWhiteSpace(whatsChargeConfig.Pix))
            {
                FeedbackLabel.Text = "Mensagem ou PIX não configurados.";
                FeedbackLabel.TextColor = Colors.Red;
                FeedbackLabel.IsVisible = true;
                return;
            }
            if (whatsChargeConfig.Date == default)
            {
                FeedbackLabel.Text = "Data para pagamento não configurada.";
                FeedbackLabel.TextColor = Colors.Red;
                FeedbackLabel.IsVisible = true;
                return;
            }

            // Obtém o nome do cliente do primeiro pedido
            string clientName = _orderList.FirstOrDefault()?.ClientName?.Trim();

            if (string.IsNullOrEmpty(clientName))
            {
                FeedbackLabel.Text = "Nome do cliente não encontrado no pedido.";
                FeedbackLabel.TextColor = Colors.Red;
                FeedbackLabel.IsVisible = true;
                return;
            }

            // Busca o contato do cliente
            var contacts = await contactsService.GetAllAsync();
            var clientContact = contacts.FirstOrDefault(c => c.ClientName.Equals(clientName, StringComparison.OrdinalIgnoreCase));

            if (clientContact == null || string.IsNullOrWhiteSpace(clientContact.Number))
            {
                FeedbackLabel.Text = "Número do cliente não encontrado nos contatos.";
                FeedbackLabel.TextColor = Colors.Red;
                FeedbackLabel.IsVisible = true;
                return;
            }

            // Calcular o valor total pendente
            decimal totalPendente = _orderList.Sum(o => o.Total);
            string orderValue = $"R$ {totalPendente:0.00}";

            string chargeMessage = whatsappService.BuildChargeMessage(
                whatsChargeConfig.Message,
                whatsChargeConfig.Date.ToString("dd/MM/yyyy"),
                whatsChargeConfig.Pix,
                orderValue);

            // Monta o número completo (adiciona o 55 para o Brasil se necessário)
            string phoneNumber = clientContact.Number.StartsWith("55") ? clientContact.Number : $"55{clientContact.Number}";

            await whatsappService.SendMessage(phoneNumber, chargeMessage);

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
