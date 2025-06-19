using CantinaV1.Models;
using CantinaV1.Services.Internals;
using ClosedXML.Excel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace CantinaV1.ViewModels
{
    public class ContactsViewModel : BindableObject
    {
        private readonly ContactsService _contactsService = new();
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
            ImportXlsxCommand = new RelayCommand(async () => await ImportXlsxAsync());
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

        private async Task ImportXlsxAsync()
        {
            try
            {
                var result = await FilePicker.PickAsync(new PickOptions
                {
                    PickerTitle = "Selecione o arquivo excel",
                    FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                    {
                        { DevicePlatform.Android, new[] { "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" } },
                        { DevicePlatform.WinUI, new[] { ".xlsx" } },
                        { DevicePlatform.iOS, new[] { "com.microsoft.excel.xlsx" } },
                        { DevicePlatform.MacCatalyst, new[] { "com.microsoft.excel.xlsx" } }
                    })
                });

                if (result != null)
                {
                    using var stream = await result.OpenReadAsync();
                    using var workbook = new XLWorkbook(stream);
                    var worksheet = workbook.Worksheet(1);
                    var rows = worksheet.RowsUsed().Skip(1);

                    foreach (var row in rows)
                    {
                        string name = row.Cell(1).GetString();
                        string number = row.Cell(2).GetString();

                        if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(number) && number.All(char.IsDigit) && number.Length == 11)
                        {
                            await _contactsService.SaveItemAsync(new ContactModel { ClientName = name.Trim(), Number = number.Trim() });
                        }
                    }
                    await LoadContacts();
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
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
