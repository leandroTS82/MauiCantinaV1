using SQLite;

namespace CantinaV1.Models
{
    public class GenericConfiguration
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }

    }
}
