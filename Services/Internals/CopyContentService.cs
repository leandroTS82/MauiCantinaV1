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
            messageBuilder.AppendLine($"Cantina do dia: {orders.First().Created.ToString("dd/MM/yyyy")}");
            messageBuilder.AppendLine();

            // Produtos
            if (products != null && products.Any())
            {
                messageBuilder.AppendLine("*Produtos*:");
                foreach (var product in products)
                {
                    messageBuilder.AppendLine($"{product.Name} : R$ {product.Price:0.00}");
                }

                messageBuilder.AppendLine("##############################################");
                messageBuilder.AppendLine();
            }

            // Agrupamento por forma de pagamento
            var groupedByPayment = orders
                .GroupBy(o => o.PaymentMethod)
                .OrderBy(g => g.Key);

            Dictionary<string, decimal> totalByPayment = new();

            foreach (var paymentGroup in groupedByPayment)
            {
                string paymentMethod = paymentGroup.Key;
                messageBuilder.AppendLine($"   *{paymentMethod.Trim()}*");
                messageBuilder.AppendLine();

                // Agrupar por cliente
                var groupedByClient = paymentGroup
                    .GroupBy(o => o.ClientName.Trim())
                    .OrderBy(g => g.Key);

                foreach (var clientGroup in groupedByClient)
                {
                    string clientName = clientGroup.Key;
                    messageBuilder.AppendLine($"*{clientName.Trim()}*:");

                    var orderNote = clientGroup.FirstOrDefault(o => !string.IsNullOrEmpty(o.OrderNotes))?.OrderNotes ?? string.Empty;

                    foreach (var order in clientGroup)
                    {
                        messageBuilder.AppendLine($"{order.ProductName} : qtd. {order.Quantity}");
                    }

                    decimal totalClient = clientGroup.Sum(o => o.Total);
                    messageBuilder.AppendLine($"Valor Total: R$ {totalClient:0.00}");
                    if (!string.IsNullOrEmpty(orderNote))
                        messageBuilder.AppendLine($"> *Obs.*: {orderNote}");

                    messageBuilder.AppendLine("----------------");
                    messageBuilder.AppendLine();
                }

                decimal totalFormaPagamento = paymentGroup.Sum(o => o.Total);
                totalByPayment[paymentMethod] = totalFormaPagamento;
                messageBuilder.AppendLine();
                messageBuilder.AppendLine("##############################################");
            }

            // Totais por forma de pagamento
            messageBuilder.AppendLine();
            messageBuilder.AppendLine("Totais por Forma de Pagamento:");
            foreach (var item in totalByPayment)
            {
                messageBuilder.AppendLine($"{item.Key} : R$ {item.Value:0.00}");
            }

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
