using SQLite;
using System.ComponentModel;

namespace CantinaV1.Models
{
    public class OrderItem : INotifyPropertyChanged
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public DateTime Created { get; set; } = DateTime.Now;
        public string ClientName { get; set; }
        public string PaymentMethod { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }

        private int _quantity;
        public int Quantity
        {
            get => _quantity;
            set
            {
                if (_quantity != value)
                {
                    _quantity = value;
                    OnPropertyChanged(nameof(Quantity));
                    OnPropertyChanged(nameof(Total));
                }
            }
        }

        public decimal Total => Price * Quantity;
        public decimal TotalSum { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
