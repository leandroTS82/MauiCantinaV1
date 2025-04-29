using CantinaV1.Models;
using System.Text;

namespace CantinaV1.Services.Externals
{
    public class CsvService
    {
        internal async Task<ResponseModel> ExportCsv(List<OrderItem> orders)
        {

            if (orders == null || !orders.Any())
                return new ResponseModel
                {
                    StatusCode = 400,
                    Message = "Nenhum pedido encontrado.",
                    Errors = new List<string> { "Nenhum pedido encontrado." }
                };

            // Criar CSV
            var csvBuilder = new StringBuilder();
            csvBuilder.AppendLine("Data,Nome, Produto,Valor,Quant.,Total,Forma Pag.");

            foreach (var order in orders)
            {
                csvBuilder.AppendLine($"{order.Created},{order.ClientName},{order.ProductName},{order.Price},{order.Quantity},{order.Total},{order.PaymentMethod}");
            }

            // Criar caminho do arquivo
            string fileName = $"Pedidos_{DateTime.Now:yyyyMMddHHmmss}.csv";
            string filePath = Path.Combine(FileSystem.CacheDirectory, fileName);

            // Salvar o arquivo
            File.WriteAllText(filePath, csvBuilder.ToString(), Encoding.UTF8);

            // Compartilhar o arquivo
            await Share.RequestAsync(new ShareFileRequest
            {
                Title = "Compartilhar CSV",
                File = new ShareFile(filePath)
            });

            return new ResponseModel
            {
                StatusCode = 200,
                Message = "CSV exportado com sucesso.",
                Errors = null
            };
        }
    }
}
