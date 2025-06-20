using CantinaV1.Models;
using CantinaV1.Popups;
using CantinaV1.Services.Internals;
using CommunityToolkit.Maui.Views;

namespace CantinaV1.Views
{
    public partial class PendingOrdersPage : ContentPage
    {
        private readonly OrderHistoryService _historyService;
        private readonly GenericConfigurationServices _genericConfigurationServices;

        public PendingOrdersPage()
        {
            InitializeComponent();
            _historyService = new OrderHistoryService();
            _genericConfigurationServices = new GenericConfigurationServices();
            LoadWhatsAppChargeConfiguration();
        }

        private async void LoadWhatsAppChargeConfiguration()
        {
            var dateConfig = await _genericConfigurationServices.GetGenericConfigurationAsync("WhatsAppChargeDate");
            var pixConfig = await _genericConfigurationServices.GetGenericConfigurationAsync("WhatsAppChargePix");
            var messageConfig = await _genericConfigurationServices.GetGenericConfigurationAsync("WhatsAppChargeMessage");
            var toggleConfig = await _genericConfigurationServices.GetGenericConfigurationAsync("WhatsAppChargeToggle");

            if (dateConfig != null && DateTime.TryParse(dateConfig.Value, out DateTime parsedDate))
                entryChargeDate.Date = parsedDate;

            if (pixConfig != null)
                entryChargePix.Text = pixConfig.Value;

            if (messageConfig != null)
                editorChargeMessage.Text = messageConfig.Value;

            if (toggleConfig != null)
                switchChargeToggle.IsToggled = Convert.ToBoolean(toggleConfig.Value);
        }
        private async void OnPreviewMessageClicked(object sender, EventArgs e)
        {
            string mensagem = editorChargeMessage.Text;
            string dataPagamento = entryChargeDate.Date.ToString("dd/MM/yyyy");
            string pix = entryChargePix.Text;
            string valorPedido = "R$ x,xx"; 

            string previewText = $"\n{mensagem}\n" +
                                 $"Data para pagamento: {dataPagamento}\n" +
                                 $"Valor: {valorPedido}\n" +
                                 $"Chave PIX: {pix}";

            var popup = new MessagePreviewPopup(previewText);
            this.ShowPopup(popup);
        }

        private async void OnChargeToggleChanged(object sender, ToggledEventArgs e)
        {
            bool isToggled = e.Value;

            await _genericConfigurationServices.SaveOrUpdateGenericConfiguration("WhatsAppChargeToggle", isToggled.ToString());

            if (isToggled)
            {
                await _genericConfigurationServices.SaveOrUpdateGenericConfiguration("WhatsAppChargeDate", entryChargeDate.Date.ToString("yyyy-MM-dd"));
                await _genericConfigurationServices.SaveOrUpdateGenericConfiguration("WhatsAppChargePix", entryChargePix.Text);
                await _genericConfigurationServices.SaveOrUpdateGenericConfiguration("WhatsAppChargeMessage", editorChargeMessage.Text);
            }
        }

        private async void OnItemTapped(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is OrderHistory selectedOrder)
            {
                var allItems = await _historyService.GetAllAsync();
                var details = allItems.Where(o => o.ClientName == selectedOrder.ClientName && o.Status == "Pendente");

                var popup = new OrderDetailPopup(details);
                this.ShowPopup(popup);
            }
        }
    }
}
