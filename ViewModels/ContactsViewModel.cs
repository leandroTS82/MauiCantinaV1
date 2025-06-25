using CantinaV1.Models;
using CantinaV1.Services.Externals;
using CantinaV1.Services.Internals;
using ClosedXML.Excel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace CantinaV1.ViewModels
{
    public class ContactsViewModel : BindableObject
    {
        private readonly ContactsService _contactsService = new();
        private readonly XlsxService _xlsxService = new();
        public ObservableCollection<ContactModel> Contacts { get; set; } = new();

        public IRelayCommand ImportXlsxCommand { get; }
        public IRelayCommand SaveContactCommand { get; }
        public IRelayCommand DeleteAllContactsCommand { get; }
        public IRelayCommand<ContactModel> DeleteContactCommand { get; }

        private ContactModel selectedContact;
        public ContactModel SelectedContact
        {
            get => selectedContact;
            set
            {
                selectedContact = value;
                if (selectedContact != null)
                {
                    ClientName = selectedContact.ClientName;
                    Number = selectedContact.Number;
                }
                OnPropertyChanged();
            }
        }

        private string clientName;
        public string ClientName
        {
            get => clientName;
            set { clientName = value; OnPropertyChanged(); }
        }

        private string number;
        public string Number
        {
            get => number;
            set { number = value; OnPropertyChanged(); }
        }

        public ContactsViewModel()
        {
            ImportXlsxCommand = new RelayCommand(async () => await _xlsxService.ImportContactXlsxAsync());
            SaveContactCommand = new RelayCommand(async () => await SaveContactAsync());
            DeleteAllContactsCommand = new RelayCommand(async () => await DeleteAllContactsAsync());
            DeleteContactCommand = new RelayCommand<ContactModel>(async (contact) => await DeleteContactAsync(contact));

            LoadContacts();
        }

        private async Task SaveContactAsync()
        {
            if (string.IsNullOrEmpty(ClientName) || string.IsNullOrEmpty(Number) || !Number.All(char.IsDigit) || Number.Length != 11)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Fill correctly: Name and 11-digit Number.", "OK");
                return;
            }

            if (SelectedContact != null)
            {
                SelectedContact.ClientName = ClientName;
                SelectedContact.Number = Number;
                await _contactsService.UpdateAsync(SelectedContact);
                SelectedContact = null;
            }
            else
            {
                await _contactsService.SaveItemAsync(new ContactModel { ClientName = ClientName, Number = Number });
            }

            await LoadContacts();
            ClientName = Number = string.Empty;
        }       

        private async Task DeleteAllContactsAsync()
        {
            bool confirm = await Application.Current.MainPage.DisplayAlert("Confirm", "Excluir todos os contatos?", "Sim", "Não");
            if (!confirm) return;

            await _contactsService.DeleteAllAsync();
            await LoadContacts();
        }

        private async Task DeleteContactAsync(ContactModel contact)
        {
            if (contact == null) return;

            bool confirm = await Application.Current.MainPage.DisplayAlert("Confirm", $"Excluir {contact.ClientName}?", "Sim", "Não");
            if (!confirm) return;

            await _contactsService.DeleteAsync(contact);
            await LoadContacts();
        }

        private async Task LoadContacts()
        {
            var list = await _contactsService.GetAllAsync();
            Contacts.Clear();
            foreach (var item in list)
                Contacts.Add(item);
        }
    }
}
