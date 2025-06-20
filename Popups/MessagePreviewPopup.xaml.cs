using CommunityToolkit.Maui.Views;

namespace CantinaV1.Popups
{
    public partial class MessagePreviewPopup : Popup
    {
        public MessagePreviewPopup(string previewMessage)
        {
            InitializeComponent();
            labelPreview.Text = previewMessage;
        }

        private void OnCloseClicked(object sender, EventArgs e)
        {
            Close();
        }
    }
}
