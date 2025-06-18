using CantinaV1.Models;
using CommunityToolkit.Maui.Views; // necessário para 'Popup'

namespace CantinaV1.Popups;

public partial class OrderDetailPopup : Popup
{
    public OrderDetailPopup(IEnumerable<OrderHistory> orderHistories)
    {
        InitializeComponent();

        var orderList = orderHistories.ToList();
        BindingContext = new
        {
            ClientName = orderList.FirstOrDefault()?.ClientName,
            Date = orderList.FirstOrDefault()?.Date,
            PaymentMethod = orderList.FirstOrDefault()?.PaymentMethod,
            Orders = orderList,
            TotalSum = orderList.Sum(o => o.Total)
        };
    }

    private void OnCloseClicked(object sender, EventArgs e)
    {
        Close();
    }
}
