using CantinaV1.Models;
using CantinaV1.Popups;
using CantinaV1.Services.Externals;
using CantinaV1.Services.Internals;
using CommunityToolkit.Maui.Views;

namespace CantinaV1.Views
{
    public partial class PendingOrdersPage : ContentPage
    {
        private readonly OrderHistoryService _historyService;
        private readonly GenericConfigurationServices _genericConfigurationServices;
        private bool isFormVisible = true;

        public PendingOrdersPage()
        {
            InitializeComponent();
            _historyService = new OrderHistoryService();
            _genericConfigurationServices = new GenericConfigurationServices();
            LoadWhatsAppChargeConfiguration();
        }

        private async void LoadWhatsAppChargeConfiguration()
        {
            WhatsAppService whatsappService = new WhatsAppService();
            var whatsconfig = await whatsappService.LoadWhatsAppChargeConfiguration();
            GenericConfiguration? toggleConfig = await _genericConfigurationServices.GetGenericConfigurationAsync("WhatsAppChargeToggle");

            entryChargeDate.Date = whatsconfig.Date;

            if (whatsconfig.Pix != null)
                entryChargePix.Text = whatsconfig.Pix;

            if (whatsconfig.Message != null)
                editorChargeMessage.Text = whatsconfig.Message;

            if (toggleConfig != null)
                switchChargeToggle.IsToggled = Convert.ToBoolean(toggleConfig.Value);
        }
        private async void OnPreviewMessageClicked(object sender, EventArgs e)
        {
            string message = editorChargeMessage.Text;
            string paymentDate = entryChargeDate.Date.ToString("dd/MM/yyyy");
            string pix = entryChargePix.Text;
            string orderValue = "R$ x,xx";
            WhatsAppService whatsappService = new WhatsAppService();
            string chargeMessage = whatsappService.BuildChargeMessage(message, paymentDate, pix, orderValue);            
            var popup = new MessagePreviewPopup(chargeMessage);
            this.ShowPopup(popup);
        }

        private void OnToggleFormButtonClicked(object sender, EventArgs e)
        {
            isFormVisible = !isFormVisible;
            formSection.IsVisible = isFormVisible;

            // Atualiza o texto do botão conforme visibilidade
            toggleFormButton.Text = isFormVisible ? "▲ Ocultar parâmetros" : "▼ Mostrar parâmetro";
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
