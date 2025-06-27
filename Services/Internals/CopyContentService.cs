using CantinaV1.Models;
using System.Text;

namespace CantinaV1.Services.Internals
{
    public class CopyContentService
    {
        internal async Task<ResponseModel> CopyReportTextAsync(List<OrderItem> orders, List<Product> products)
        {
            if (orders == null || !orders.Any())
                return new ResponseModel
                {
                    StatusCode = 400,
                    Message = "Nenhum pedido encontrado.",
                    Errors = new List<string> { "Nenhum pedido encontrado." }
                };

            var messageBuilder = new StringBuilder();

            var dateOrder = orders.First().Created.ToString("dd/MM/yyyy");
            messageBuilder.AppendLine($"📅 Cantina - {dateOrder}");
            messageBuilder.AppendLine();

            // Produtos
            if (products != null && products.Any())
            {
                messageBuilder.AppendLine("🍽️ *Cardápio do Dia*");
                foreach (var product in products)
                {
                    messageBuilder.AppendLine($"- {product.Name}: R$ {product.Price:0.00}");
                }

                messageBuilder.AppendLine();
                messageBuilder.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━");
            }

            // Agrupamento por forma de pagamento
            var groupedByPayment = orders
                .GroupBy(o => o.PaymentMethod.Trim())
                .OrderBy(g => g.Key);

            Dictionary<string, decimal> totalByPayment = new();

            foreach (var paymentGroup in groupedByPayment)
            {
                string paymentMethod = paymentGroup.Key.ToLower() switch
                {
                    "dinheiro" => "💵 *Pagamentos em Dinheiro*",
                    "pix" => "💳 *Pagamentos via Pix*",
                    "cartão" => "💳 *Pagamentos no Cartão*",
                    _ => $"💰 *{paymentGroup.Key}*"
                };

                messageBuilder.AppendLine();
                messageBuilder.AppendLine(paymentMethod);
                messageBuilder.AppendLine();

                var groupedByClient = paymentGroup
                    .GroupBy(o => o.ClientName.Trim())
                    .OrderBy(g => g.Key);

                var clientList = groupedByClient.ToList();
                for (int i = 0; i < clientList.Count; i++)
                {
                    var clientGroup = clientList[i];
                    string clientName = clientGroup.Key;
                    messageBuilder.AppendLine($"👤 *{clientName}*");

                    foreach (var order in clientGroup)
                    {
                        messageBuilder.AppendLine($"• {order.ProductName} x{order.Quantity}");
                    }

                    decimal totalClient = clientGroup.Sum(o => o.Total);
                    messageBuilder.AppendLine($"💰 Total: R$ {totalClient:0.00}");

                    var orderNote = clientGroup.FirstOrDefault(o => !string.IsNullOrEmpty(o.OrderNotes))?.OrderNotes;
                    if (!string.IsNullOrWhiteSpace(orderNote))
                        messageBuilder.AppendLine($"> 📝 {orderNote}");

                    if (i < clientList.Count - 1) // Evita separador depois do último cliente
                        messageBuilder.AppendLine("-----------------------");

                    messageBuilder.AppendLine(); // Linha em branco entre os blocos
                }

                decimal totalFormaPagamento = paymentGroup.Sum(o => o.Total);
                totalByPayment[paymentGroup.Key] = totalFormaPagamento;

                messageBuilder.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━");
            }

            // Totais por forma de pagamento
            messageBuilder.AppendLine();
            messageBuilder.AppendLine("📊 *Totais por Forma de Pagamento*");
            foreach (var item in totalByPayment)
            {
                messageBuilder.AppendLine($"- {item.Key}: R$ {item.Value:0.00}");
            }
            decimal totalGeral = totalByPayment.Sum(o => o.Value);
            messageBuilder.AppendLine();
            messageBuilder.AppendLine($"🧾 *Total Geral*: R$ {totalGeral:0.00}");


            string finalText = messageBuilder.ToString();

            try
            {
                await Clipboard.SetTextAsync(finalText);

                return new ResponseModel
                {
                    StatusCode = 200,
                    Message = "Relatório copiado com sucesso!",
                    Errors = null
                };
            }
            catch (Exception ex)
            {
                return new ResponseModel
                {
                    StatusCode = 500,
                    Message = "Erro ao copiar relatório.",
                    Errors = new List<string> { ex.Message }
                };
            }
        }
        internal async Task<ResponseModel> CopyOrderHistoryReportTextAsync(List<OrderHistory> orders)
        {
            if (orders == null || !orders.Any())
                return new ResponseModel
                {
                    StatusCode = 400,
                    Message = "Nenhum pedido encontrado.",
                    Errors = new List<string> { "Nenhum pedido encontrado." }
                };

            var messageBuilder = new StringBuilder();
            var dateOrder = DateTime.Now.ToString("dd/MM/yyyy");
            messageBuilder.AppendLine($"📅 Histórico de Pedidos - {dateOrder}\n");

            // Agrupamento por forma de pagamento
            var groupedByPayment = orders
                .GroupBy(o => o.PaymentMethod.Trim())
                .OrderBy(g => g.Key);

            Dictionary<string, decimal> totalByPayment = new();

            foreach (var paymentGroup in groupedByPayment)
            {
                string paymentMethod = paymentGroup.Key.ToLower() switch
                {
                    "dinheiro" => "💵 *Pagamentos em Dinheiro*",
                    "pix" => "💳 *Pagamentos via Pix*",
                    "cartão" => "💳 *Pagamentos no Cartão*",
                    _ => $"💰 *{paymentGroup.Key}*"
                };

                messageBuilder.AppendLine();
                messageBuilder.AppendLine(paymentMethod);
                messageBuilder.AppendLine();

                var groupedByClient = paymentGroup
                    .GroupBy(o => o.ClientName.Trim())
                    .OrderBy(g => g.Key);

                var clientList = groupedByClient.ToList();
                for (int i = 0; i < clientList.Count; i++)
                {
                    var clientGroup = clientList[i];
                    string clientName = clientGroup.Key;
                    messageBuilder.AppendLine($"👤 *{clientName}*");

                    foreach (var order in clientGroup)
                    {
                        messageBuilder.AppendLine($"• {order.ProductName} x{order.Quantity}");
                    }

                    decimal totalClient = clientGroup.Sum(o => o.Total);
                    messageBuilder.AppendLine($"💰 Total: R$ {totalClient:0.00}");

                    var orderNote = clientGroup.FirstOrDefault(o => !string.IsNullOrEmpty(o.Observation))?.Observation;
                    if (!string.IsNullOrWhiteSpace(orderNote))
                        messageBuilder.AppendLine($"> 📝 {orderNote}");

                    if (i < clientList.Count - 1) // Evita separador depois do último cliente
                        messageBuilder.AppendLine("-----------------------");

                    messageBuilder.AppendLine(); // Linha em branco entre os blocos
                }

                decimal totalFormaPagamento = paymentGroup.Sum(o => o.Total);
                totalByPayment[paymentGroup.Key] = totalFormaPagamento;

                messageBuilder.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━");
            }

            // Totais por forma de pagamento
            messageBuilder.AppendLine();
            messageBuilder.AppendLine("📊 *Totais por Forma de Pagamento*");
            foreach (var item in totalByPayment)
            {
                messageBuilder.AppendLine($"- {item.Key}: R$ {item.Value:0.00}");
            }
            decimal totalGeral = totalByPayment.Sum(o => o.Value);
            messageBuilder.AppendLine();
            messageBuilder.AppendLine($"🧾 *Total Geral*: R$ {totalGeral:0.00}");


            string finalText = messageBuilder.ToString();

            try
            {
                await Clipboard.SetTextAsync(finalText);

                return new ResponseModel
                {
                    StatusCode = 200,
                    Message = "Relatório copiado com sucesso!",
                    Errors = null
                };
            }
            catch (Exception ex)
            {
                return new ResponseModel
                {
                    StatusCode = 500,
                    Message = "Erro ao copiar relatório.",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

    }
}
