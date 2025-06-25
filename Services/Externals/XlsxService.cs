using CantinaV1.Models;
using CantinaV1.Services.Internals;
using ClosedXML.Excel;

namespace CantinaV1.Services.Externals
{
    public class XlsxService
    {
        private readonly ContactsService _contactsService = new();

        internal async Task<ResponseModel> ExportOrdersToXlsxAsync(List<OrderItem> orderItems, List<OrderHistory> orderHistory = null)
        {
            if ((orderItems == null || !orderItems.Any()) && (orderHistory == null || !orderHistory.Any()))
            {
                return new ResponseModel
                {
                    StatusCode = 400,
                    Message = "Nenhum pedido encontrado.",
                    Errors = new List<string> { "Nenhum pedido encontrado." }
                };
            }
            bool isOrderHistory = false;

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Pedidos");

            // Cabeçalhos
            worksheet.Cell(1, 1).Value = "Data";
            worksheet.Cell(1, 2).Value = "Nome";
            worksheet.Cell(1, 3).Value = "Produto";
            worksheet.Cell(1, 4).Value = "Valor";
            worksheet.Cell(1, 5).Value = "Quantidade";
            worksheet.Cell(1, 6).Value = "Total";
            worksheet.Cell(1, 7).Value = "Forma de Pagamento";
            worksheet.Cell(1, 8).Value = "Status";
            worksheet.Cell(1, 9).Value = "Data de Pagamento";
            worksheet.Cell(1, 10).Value = "Obs.";

            int row = 2;

            if (orderItems != null)
            {
                foreach (var order in orderItems)
                {
                    var status = order.PaymentMethod != null && order.PaymentMethod.ToLower().Trim() != "pagar depois" ? "Pago" : "Pendente";
                    var paymentDate = status == "Pago" ? order.Created : (DateTime?)null;

                    worksheet.Cell(row, 1).Value = order.Created.ToString("dd/MM/yyyy HH:mm");
                    worksheet.Cell(row, 2).Value = order.ClientName;
                    worksheet.Cell(row, 3).Value = order.ProductName;
                    worksheet.Cell(row, 4).Value = order.Price;
                    worksheet.Cell(row, 5).Value = order.Quantity;
                    worksheet.Cell(row, 6).Value = order.Total;
                    worksheet.Cell(row, 7).Value = order.PaymentMethod;
                    worksheet.Cell(row, 8).Value = status;
                    worksheet.Cell(row, 9).Value = paymentDate?.ToString("dd/MM/yyyy HH:mm") ?? "";
                    worksheet.Cell(row, 10).Value = order.OrderNotes;
                    row++;
                }
            }

            if (orderHistory != null)
            {
                isOrderHistory = true;
                foreach (var history in orderHistory)
                {
                    worksheet.Cell(row, 1).Value = history.Date.ToString("dd/MM/yyyy HH:mm");
                    worksheet.Cell(row, 2).Value = history.ClientName;
                    worksheet.Cell(row, 3).Value = history.ProductName;
                    worksheet.Cell(row, 4).Value = history.Price;
                    worksheet.Cell(row, 5).Value = history.Quantity;
                    worksheet.Cell(row, 6).Value = history.Total;
                    worksheet.Cell(row, 7).Value = history.PaymentMethod;
                    worksheet.Cell(row, 8).Value = history.Status;
                    worksheet.Cell(row, 9).Value = history.PaymentDate?.ToString("dd/MM/yyyy HH:mm") ?? "";
                    worksheet.Cell(row, 10).Value = history.Observation;
                    row++;
                }
            }

            worksheet.Columns().AdjustToContents();

            string fileName = (isOrderHistory) ? $"HistoricoPedidos_{DateTime.Now:yyyyMMddHHmmss}.xlsx" : 
                $"Pedidos_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
            string filePath = Path.Combine(FileSystem.CacheDirectory, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                workbook.SaveAs(stream);
            }

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

        internal async Task<List<OrderHistory>> ImportOrderHistoryXlsxAsync()
        {
            var orders = new List<OrderHistory>();

            try
            {
                var result = await FilePicker.PickAsync(new PickOptions
                {
                    PickerTitle = "Selecione o arquivo Excel",
                    FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                    {
                        { DevicePlatform.Android, new[] { "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" } },
                        { DevicePlatform.WinUI, new[] { ".xlsx" } },
                        { DevicePlatform.iOS, new[] { "com.microsoft.excel.xlsx" } },
                        { DevicePlatform.MacCatalyst, new[] { "com.microsoft.excel.xlsx" } }
                    })
                });

                if (result != null)
                {
                    using var stream = await result.OpenReadAsync();
                    using var workbook = new XLWorkbook(stream);
                    var worksheet = workbook.Worksheet(1);
                    var rows = worksheet.RowsUsed().Skip(1);

                    foreach (var row in rows)
                    {
                        orders.Add(ParseOrderHistoryRow(row));
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao importar arquivo Excel: " + ex.Message, ex);
            }

            return orders;
        }

        private OrderHistory ParseOrderHistoryRow(IXLRow row)
        {
            var paymentMethod = row.Cell(7).GetString();
            var statusCell = row.Cell(8).GetString();
            var status = !string.IsNullOrWhiteSpace(statusCell)
                ? statusCell
                : paymentMethod?.Trim().ToLower() != "pagar depois" ? "Pago" : "Pendente";

            DateTime.TryParse(row.Cell(1).GetString(), out var date);
            DateTime.TryParse(row.Cell(9).GetString(), out var paymentDate);
            decimal.TryParse(row.Cell(4).GetString().Replace("R$", "").Trim(), out var price);
            int.TryParse(row.Cell(5).GetString(), out var quantity);
            decimal.TryParse(row.Cell(6).GetString().Replace("R$", "").Trim(), out var total);

            return new OrderHistory
            {
                Date = date == default ? DateTime.Now : date,
                ClientName = row.Cell(2).GetString(),
                ProductName = row.Cell(3).GetString(),
                Price = price,
                Quantity = quantity,
                Total = total,
                PaymentMethod = paymentMethod,
                Status = status,
                PaymentDate = paymentDate == default ? null : paymentDate,
                Observation = row.Cell(10).GetString()
            };
        }

        internal async Task ImportContactXlsxAsync()
        {
            try
            {
                var result = await FilePicker.PickAsync(new PickOptions
                {
                    PickerTitle = "Selecione o arquivo excel",
                    FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                    {
                        { DevicePlatform.Android, new[] { "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" } },
                        { DevicePlatform.WinUI, new[] { ".xlsx" } },
                        { DevicePlatform.iOS, new[] { "com.microsoft.excel.xlsx" } },
                        { DevicePlatform.MacCatalyst, new[] { "com.microsoft.excel.xlsx" } }
                    })
                });

                if (result != null)
                {
                    using var stream = await result.OpenReadAsync();
                    using var workbook = new XLWorkbook(stream);
                    var worksheet = workbook.Worksheet(1);
                    var rows = worksheet.RowsUsed().Skip(1);

                    foreach (var row in rows)
                    {
                        string name = row.Cell(1).GetString();
                        string number = row.Cell(2).GetString();

                        if (!string.IsNullOrEmpty(name))
                        {
                            await _contactsService.SaveItemAsync(new ContactModel
                            {
                                ClientName = name.Trim(),
                                Number = number.Trim()
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Erro", ex.Message, "OK");
            }
        }
    }
}
