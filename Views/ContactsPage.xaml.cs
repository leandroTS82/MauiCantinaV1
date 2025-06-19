namespace CantinaV1.Views;

public partial class ContactsPage : ContentPage
{
    public ContactsPage()
    {
        InitializeComponent();
    }

    private void CollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (BindingContext is CantinaV1.ViewModels.ContactsViewModel viewModel)
        {
            var selectedContact = e.CurrentSelection.FirstOrDefault() as CantinaV1.Models.ContactModel;
            viewModel.SelectedContact = selectedContact;
        }
    }
}
