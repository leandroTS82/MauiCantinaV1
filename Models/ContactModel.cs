using SQLite;

namespace CantinaV1.Models
{
    public class ContactModel
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string ClientName { get; set; }
        public string Number { get; set; }
    }
}
