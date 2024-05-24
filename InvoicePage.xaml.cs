using System.Collections.ObjectModel;
using Newtonsoft.Json;
using Vehicle.Models;
using System.Timers;

namespace Vehicle;

public partial class InvoicePage : ContentPage
{
    private readonly System.Timers.Timer _refreshTimer;
    private readonly PunderoApiService _punderoApiService;
    public ObservableCollection<PunderoApiService.InvoiceDto> Invoices { get; set; }
    public InvoicePage(PunderoApiService punderoApiService)
    {
        InitializeComponent();
        _punderoApiService = punderoApiService;
        Invoices = new ObservableCollection<PunderoApiService.InvoiceDto>();
        BindingContext = this;
        LoadInvoicesForDriver();// Automatically load invoices

        _refreshTimer = new System.Timers.Timer(1000); // Set the interval to 60 seconds (60000 ms)
        _refreshTimer.Elapsed += async (sender, e) => await RefreshInvoices();
        _refreshTimer.AutoReset = true;
        _refreshTimer.Enabled = true;
    }
    private async void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e)
    {
        await RefreshInvoices();
    }
    private async Task RefreshInvoices()
    {
        try
        {
            
            var authToken = await SecureStorage.GetAsync("authToken");
            if (authToken == null) return;

            var driverIdString = await SecureStorage.GetAsync("driverId");
            if (string.IsNullOrEmpty(driverIdString) || !int.TryParse(driverIdString, out var driverId)) return;

            var newInvoices = await _punderoApiService.GetApprovedInvoicesForDriverAsync(driverId);

            Device.BeginInvokeOnMainThread(() =>
            {
                foreach (var newInvoice in newInvoices)
                {
                    if (!Invoices.Any(i => i.IdInvoice == newInvoice.IdInvoice))
                    {
                        Invoices.Add(newInvoice);
                    }
                }

                NoInvoicesLabel.IsVisible = !Invoices.Any(); // Toggle visibility based on the count
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error refreshing invoices: {ex.Message}\n{ex.StackTrace}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner Exception: {ex.InnerException.Message}\n{ex.InnerException.StackTrace}");
            }
        }
    }
    //Pocetak LoadaInvoice
    private async Task LoadInvoicesForDriver()
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

            NoInvoicesLabel.IsVisible = !Invoices.Any(); // Toggle visibility based on the count
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting invoices: {ex.Message}\n{ex.StackTrace}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner Exception: {ex.InnerException.Message}\n{ex.InnerException.StackTrace}");
            }
            //await DisplayAlert("Error", "Failed to retrieve invoices.", "OK");
            NoInvoicesLabel.IsVisible = !Invoices.Any();
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

                var invoice = Invoices.FirstOrDefault(i => i.IdInvoice == invoiceId);
                if (invoice != null)
                {
                    Invoices.Remove(invoice);
                }

                NoInvoicesLabel.IsVisible = !Invoices.Any(); // Toggle visibility based on the count
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating invoice status: {ex.Message}\n{ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}\n{ex.InnerException.StackTrace}");
                }
                await DisplayAlert("Error", "Failed to update invoice status.", "OK");
            }
        }
    }
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _refreshTimer.Stop();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _refreshTimer.Start();
    }
    ~InvoicePage()
    {
        _refreshTimer?.Dispose();
    }
}
