using SQLite;

namespace CantinaV1.Models
{
    public class Order
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Cliente { get; set; }
        public string FormaPagamento { get; set; }
        public decimal Total { get; set; }
    }
}
