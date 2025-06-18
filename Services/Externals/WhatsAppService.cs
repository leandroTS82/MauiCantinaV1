using CantinaV1.Models;
using CantinaV1.Data;
using System.Diagnostics;
using System.Text;
using CantinaV1.Services.Internals;

namespace CantinaV1.Services.Externals
{
    public class WhatsAppService
    {
        private readonly Database _database;
        private readonly GenericConfigurationServices _genericConfigurationServices;
        public WhatsAppService()
        {
            string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "cantina.db4");
            _database = new Database(dbPath);
            _genericConfigurationServices = new GenericConfigurationServices();
        }

        internal async Task SendMessage(string phoneNumber, string message)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(phoneNumber) || string.IsNullOrWhiteSpace(message))
                    return;

                // Monta o link para o WhatsApp com o número e mensagem
                string url = $"https://wa.me/{phoneNumber}?text={Uri.EscapeDataString(message)}";

                // Tenta abrir o link no WhatsApp
                await Launcher.OpenAsync(new Uri(url));
            }
            catch (Exception ex)
            {
                // Em caso de erro, escreve no log
                Debug.WriteLine($"Erro ao enviar mensagem via WhatsApp: {ex.Message}");
            }
        }


        internal async Task SendOrderAsync(List<OrderItem> orderItemsToSave)
        {
            if (orderItemsToSave == null || !orderItemsToSave.Any())
                return;

            try
            {
                // Recuperar configurações necessárias em paralelo
                var configKeys = new[] { "entryDDD", "entryTelefone", "switchHabilitado" };
                var configTasks = configKeys.Select(key => _genericConfigurationServices.GetGenericConfigurationAsync(key));
                var configs = (await Task.WhenAll(configTasks)).ToDictionary(c => c?.Key, c => c);

                // Verificar se alguma configuração está ausente
                if (!configs.ContainsKey("entryDDD") || !configs.ContainsKey("entryTelefone") || !configs.ContainsKey("switchHabilitado"))
                    return;

                var ddd = configs["entryDDD"]?.Value?.Trim();
                var telefone = configs["entryTelefone"]?.Value?.Trim();
                var receberPedidos = configs["switchHabilitado"]?.Value;

                // Validações
                if (!bool.TryParse(receberPedidos, out bool isEnabled) || !isEnabled)
                    return;

                if (string.IsNullOrEmpty(ddd) || string.IsNullOrEmpty(telefone))
                    return;

                string phoneNumber = $"550{ddd}{telefone}";

                var clientName = orderItemsToSave.First().ClientName?.Trim();
                if (string.IsNullOrEmpty(clientName))
                    return;

                // Monta a mensagem
                var messageBuilder = new StringBuilder()
                    .AppendLine($"Pedido de *{clientName}*:")
                    .AppendLine();

                foreach (var item in orderItemsToSave)
                {
                    messageBuilder.AppendLine($"{item.ProductName} - Qtd.: {item.Quantity}");
                }

                string message = messageBuilder.ToString();
                string url = $"https://wa.me/{phoneNumber}?text={Uri.EscapeDataString(message)}";

                await Launcher.OpenAsync(new Uri(url));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro ao enviar o pedido: {ex.Message}");
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
