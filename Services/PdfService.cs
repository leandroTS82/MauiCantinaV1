using CantinaV1.Models;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using System.Text;


namespace CantinaV1.Services
{
    public class PdfService
    {
        internal async Task GenerateOrdersPdfAsync(List<OrderItem> orders, List<Product> products)
        {
            if (orders == null || !orders.Any())
                return;

            var sb = new StringBuilder();

            sb.AppendLine("Produtos:");
            foreach (var product in products)
            {
                sb.AppendLine($"{product.Name} : R$ {product.Price:0.00}");
            }
            sb.AppendLine("##############################################");
            sb.AppendLine();

            var paymentTotals = new Dictionary<string, decimal>();

            var orderDate = orders.First().Created.ToString("yyyyMMddHHmmss");

            var groupedByPayment = orders
                .GroupBy(o => o.PaymentMethod)
                .OrderBy(g => g.Key);

            foreach (var paymentGroup in groupedByPayment)
            {
                sb.AppendLine($"*{paymentGroup.Key}*");

                decimal totalPerPayment = 0;

                var groupedByClient = paymentGroup
                    .GroupBy(o => o.ClientName.Trim())
                    .OrderBy(g => g.Key);

                foreach (var clientGroup in groupedByClient)
                {
                    string clientName = clientGroup.Key;
                    sb.AppendLine($"{clientName}:");

                    foreach (var order in clientGroup)
                    {
                        sb.AppendLine($"{order.ProductName} : qtd. {order.Quantity}");
                    }

                    decimal totalClient = clientGroup.Sum(o => o.Total);
                    sb.AppendLine($"Valor Total por {clientName} : R$ {totalClient:0.00}");
                    sb.AppendLine();

                    totalPerPayment += totalClient;
                }

                paymentTotals[paymentGroup.Key] = totalPerPayment;
                sb.AppendLine("##############################################");
            }

            sb.AppendLine();
            sb.AppendLine("Totais por Forma de Pagamento:");
            foreach (var kvp in paymentTotals)
            {
                sb.AppendLine($"{kvp.Key}: R$ {kvp.Value:0.00}");
            }

            // Criar o PDF
            var document = new PdfDocument();
            var page = document.AddPage();
            var gfx = XGraphics.FromPdfPage(page);
            var font = new XFont("Courier New", 12, XFontStyle.Regular);

            // Quebra de linha por altura
            var lines = sb.ToString().Split('\n');
            double yPoint = 40;
            foreach (var line in lines)
            {
                gfx.DrawString(line.TrimEnd(), font, XBrushes.Black, new XRect(40, yPoint, page.Width - 80, page.Height - 40), XStringFormats.TopLeft);
                yPoint += 20;

                if (yPoint >= page.Height - 40)
                {
                    page = document.AddPage();
                    gfx = XGraphics.FromPdfPage(page);
                    yPoint = 40;
                }
            }

            // Salvar o PDF
            using (var stream = File.Create($"relatorioCantina-{orderDate}"))
            {
                document.Save(stream);
            }
        }
    }
}
