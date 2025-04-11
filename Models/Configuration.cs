using SQLite;

namespace CantinaV1.Models
{
    public class Configuration
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string DDD { get; set; }
        public string Telefone { get; set; }
        public bool ReceberPedidos { get; set; }
    }
}
