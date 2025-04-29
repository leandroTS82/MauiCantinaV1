using CantinaV1.Models;
using CantinaV1.Data;
using System.Diagnostics;
using System.Text;

namespace CantinaV1.Services.Internals
{
    public class WhatsAppService
    {
        private readonly Database _database;

        public WhatsAppService()
        {
            string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "cantina.db4");
            _database = new Database(dbPath);
        }

        internal async Task SendOrderAsync(List<OrderItem> orderItemsToSave)
        {
            if (orderItemsToSave == null || !orderItemsToSave.Any())
                return;

            var config = await _database.GetConfiguracaoAsync();

            if (config == null || !config.ReceberPedidos || string.IsNullOrEmpty(config.DDD) || string.IsNullOrEmpty(config.Telefone))
                return;

            string phoneNumber = $"550{config.DDD}{config.Telefone}";

            string clientName = orderItemsToSave.First().ClientName.Trim();

            var messageBuilder = new StringBuilder();
            messageBuilder.AppendLine($"Pedido de *{clientName}*:");
            messageBuilder.AppendLine();

            foreach (var item in orderItemsToSave)
            {
                messageBuilder.AppendLine($"{item.ProductName} - Qtd.: {item.Quantity}");
            }

            string message = messageBuilder.ToString();

            string url = $"https://wa.me/{phoneNumber}?text={Uri.EscapeDataString(message)}";

            try
            {
                await Launcher.OpenAsync(new Uri(url));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro ao abrir o WhatsApp: {ex.Message}");
            }
        }

        internal async Task SendOrdersToCustomNumberAsync(List<OrderItem> orders, List<Product> products, string phoneNumber)
        {
            if (orders == null || !orders.Any())
                return;

            var messageBuilder = new StringBuilder();

            // Seção de produtos
            messageBuilder.AppendLine("Produtos:");
            foreach (var product in products)
            {
                messageBuilder.AppendLine($"{product.Name} : R$ {product.Price:0.00}");
            }
            messageBuilder.AppendLine("##############################################");
            messageBuilder.AppendLine();

            var paymentTotals = new Dictionary<string, decimal>();

            // Agrupar pedidos por método de pagamento
            var groupedByPayment = orders
                .GroupBy(o => o.PaymentMethod)
                .OrderBy(g => g.Key);

            foreach (var paymentGroup in groupedByPayment)
            {
                messageBuilder.AppendLine($"*{paymentGroup.Key}*");

                decimal totalPerPayment = 0;

                // Agrupar por cliente
                var groupedByClient = paymentGroup
                    .GroupBy(o => o.ClientName.Trim())
                    .OrderBy(g => g.Key);

                foreach (var clientGroup in groupedByClient)
                {
                    string clientName = clientGroup.Key;
                    messageBuilder.AppendLine($"{clientName}:");

                    foreach (var order in clientGroup)
                    {
                        messageBuilder.AppendLine($"{order.ProductName} : qtd. {order.Quantity}");
                    }

                    decimal totalClient = clientGroup.Sum(o => o.Total);
                    messageBuilder.AppendLine($"Valor Total por {clientName} : R$ {totalClient:0.00}");
                    messageBuilder.AppendLine();

                    totalPerPayment += totalClient;
                }

                paymentTotals[paymentGroup.Key] = totalPerPayment;
                messageBuilder.AppendLine("##############################################");
            }

            // Seção de totais por forma de pagamento
            messageBuilder.AppendLine();
            messageBuilder.AppendLine("Totais por Forma de Pagamento:");
            foreach (var kvp in paymentTotals)
            {
                messageBuilder.AppendLine($"{kvp.Key}: R$ {kvp.Value:0.00}");
            }

            string message = messageBuilder.ToString();
            string url = $"https://wa.me/{phoneNumber}?text={Uri.EscapeDataString(message)}";

            try
            {
                await Launcher.OpenAsync(new Uri(url));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro ao abrir o WhatsApp: {ex.Message}");
            }
        }

    }
}
