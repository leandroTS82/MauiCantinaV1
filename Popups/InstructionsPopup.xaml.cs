using CommunityToolkit.Maui.Views;
namespace CantinaV1.Popups
{
    public partial class InstructionsPopup : Popup
    {
        public InstructionsPopup()
        {
            InitializeComponent();
        }
        private void OnCloseClicked(object sender, EventArgs e)
        {
            Close(); // Fecha o popup
        }
    }

}
