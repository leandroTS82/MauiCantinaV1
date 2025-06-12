using CantinaV1.Models;
using ClosedXML.Excel;

namespace CantinaV1.Services.Externals
{
    public class OrderHistoryImportService
    {
        public List<OrderHistory> ImportFromXlsx(string filePath)
        {
            var histories = new List<OrderHistory>();

            using (var workbook = new XLWorkbook(filePath))
            {
                var worksheet = workbook.Worksheet(1);
                var rows = worksheet.RangeUsed().RowsUsed();

                foreach (var row in rows.Skip(1))
                {
                    histories.Add(new OrderHistory
                    {
                        Date = DateTime.Parse(row.Cell(1).GetString()),
                        ClientName = row.Cell(2).GetString(),
                        ProductName = row.Cell(3).GetString(),
                        Price = decimal.Parse(row.Cell(4).GetString().Replace("R$", "").Trim()),
                        Quantity = int.Parse(row.Cell(5).GetString()),
                        Total = decimal.Parse(row.Cell(6).GetString().Replace("R$", "").Trim()),
                        PaymentMethod = row.Cell(7).GetString(),
                        Observation = row.Cell(8).GetString()
                    });
                }
            }

            return histories;
        }
    }
}
