using System.Collections.ObjectModel;
using Newtonsoft.Json;
using Vehicle.Models;

namespace Vehicle;

public partial class InvoicePage : ContentPage
{
    private readonly PunderoApiService _punderoApiService;
    public ObservableCollection<InvoiceDto> Invoices { get; set; }

    public InvoicePage(PunderoApiService punderoApiService)
    {
        InitializeComponent();
        _punderoApiService = punderoApiService;
        Invoices = new ObservableCollection<InvoiceDto>();
        BindingContext = this;
        LoadInvoicesForDriver(); // Automatically load invoices
    }


    private async void LoadInvoicesForDriver()
    {

        try
        {
            var authToken = await SecureStorage.GetAsync("authToken");
            if (authToken == null)
            {
                await DisplayAlert("Error", "Missing authentication token.", "OK");
                return;
            }

            var driverIdString = await SecureStorage.GetAsync("driverId");
            if (string.IsNullOrEmpty(driverIdString) || !int.TryParse(driverIdString, out var driverId))
            {
                await DisplayAlert("Error", "Failed to retrieve driver ID.", "OK");
                return;
            }

            var invoices = await _punderoApiService.GetApprovedInvoicesForDriverAsync(driverId);
            Invoices.Clear();
            foreach (var invoice in invoices)
            {
                Invoices.Add(invoice);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting invoices: {ex.Message}");
            await DisplayAlert("Error", "Failed to retrieve invoices.", "OK");
        }
    }

    private async void OnChangeStatusButtonClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is int invoiceId)
        {
            try
            {
                await _punderoApiService.UpdateInvoiceStatusAsync(invoiceId, 3); // Set status to "In Transport"
                await DisplayAlert("Success", "Invoice status updated.", "OK");
                LoadInvoicesForDriver(); // Refresh the list
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating invoice status: {ex.Message}");
                await DisplayAlert("Error", "Failed to update invoice status.", "OK");
            }
        }
    }
}
