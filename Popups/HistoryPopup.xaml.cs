using CommunityToolkit.Maui.Views;

namespace CantinaV1.Popups
{
    public partial class HistoryPopup : Popup
    {
        public string Observation { get; private set; } = string.Empty;
        public bool RegisterHistory { get; private set; } = false;

        public HistoryPopup()
        {
            InitializeComponent();
        }

        private void OnRegisterAndClearClicked(object sender, EventArgs e)
        {
            RegisterHistory = true;
            Observation = entryObservation.Text;
            Close(this);
        }

        private void OnClearWithoutRegisterClicked(object sender, EventArgs e)
        {
            RegisterHistory = false;
            Close(this);
        }
    }
}
