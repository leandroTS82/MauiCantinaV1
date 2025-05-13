using CantinaV1.Models;
using ClosedXML.Excel;

namespace CantinaV1.Services.Externals
{
    public class XlsxService
    {
        internal async Task<ResponseModel> ExportOrdersToXlsxAsync(List<OrderItem> orders)
        {
            if (orders == null || !orders.Any())
            {
                return new ResponseModel
                {
                    StatusCode = 400,
                    Message = "Nenhum pedido encontrado.",
                    Errors = new List<string> { "Nenhum pedido encontrado." }
                };
            }

            // Criar workbook e planilha
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Pedidos");

            // Cabeçalho
            worksheet.Cell(1, 1).Value = "Data";
            worksheet.Cell(1, 2).Value = "Nome";
            worksheet.Cell(1, 3).Value = "Produto";
            worksheet.Cell(1, 4).Value = "Valor";
            worksheet.Cell(1, 5).Value = "Quantidade";
            worksheet.Cell(1, 6).Value = "Total";
            worksheet.Cell(1, 7).Value = "Forma de Pagamento";
            worksheet.Cell(1, 8).Value = "Obs.";

            // Dados
            int row = 2;
            foreach (var order in orders)
            {
                worksheet.Cell(row, 1).Value = order.Created.ToString("dd/MM/yyyy HH:mm");
                worksheet.Cell(row, 2).Value = order.ClientName;
                worksheet.Cell(row, 3).Value = order.ProductName;
                worksheet.Cell(row, 4).Value = order.Price;
                worksheet.Cell(row, 5).Value = order.Quantity;
                worksheet.Cell(row, 6).Value = order.Total;
                worksheet.Cell(row, 7).Value = order.PaymentMethod;
                worksheet.Cell(row, 8).Value = order.OrderNotes;
                row++;
            }

            worksheet.Columns().AdjustToContents();

            // Caminho do arquivo
            string fileName = $"Pedidos_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
            string filePath = Path.Combine(FileSystem.CacheDirectory, fileName);

            // Salvar arquivo
            using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                workbook.SaveAs(stream);
            }

            // Compartilhar arquivo
            await Share.RequestAsync(new ShareFileRequest
            {
                Title = "Compartilhar Excel",
                File = new ShareFile(filePath)
            });

            return new ResponseModel
            {
                StatusCode = 200,
                Message = "Excel exportado com sucesso.",
                Errors = null
            };
        }
    }
}
