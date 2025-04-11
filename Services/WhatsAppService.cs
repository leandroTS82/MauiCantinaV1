using CantinaV1.Models;
using CantinaV1.Data;
using System.Diagnostics;
using System.Text;

namespace CantinaV1.Services
{
    public class WhatsAppService
    {
        private readonly Database _database;

        public WhatsAppService()
        {
            string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "cantina.db4");
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
    }
}
