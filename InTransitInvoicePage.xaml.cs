using System.Collections.ObjectModel;
using Newtonsoft.Json;
using Vehicle.Models;
using Microsoft.Maui.Controls;
using System.Linq;

namespace Vehicle;

public partial class InTransitInvoicePage : ContentPage
{
    private readonly PunderoApiService _punderoApiService;
    public ObservableCollection<PunderoApiService.InvoiceDto> InTransitInvoices { get; set; }
    private System.Timers.Timer _refreshTimer;

    public InTransitInvoicePage(PunderoApiService punderoApiService)
    {
        InitializeComponent();
        _punderoApiService = punderoApiService ?? throw new ArgumentNullException(nameof(punderoApiService));
        InTransitInvoices = new ObservableCollection<PunderoApiService.InvoiceDto>();
        BindingContext = this;
        LoadInTransitInvoicesForDriver(); // Automatically load invoices

        InitializeTimer();
    }

    private void InitializeTimer()
    {
        _refreshTimer = new System.Timers.Timer(1000); // Set the interval to 60 seconds (60000 ms)
        _refreshTimer.Elapsed += async (sender, e) => await RefreshInvoices();
        _refreshTimer.AutoReset = true;
        _refreshTimer.Enabled = true;
    }

    private async Task LoadInTransitInvoicesForDriver()
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

            var invoices = await _punderoApiService.GetInTransitInvoicesForDriverAsync(driverId);
            InTransitInvoices.Clear();
            foreach (var invoice in invoices)
            {
                InTransitInvoices.Add(invoice);
            }
            NoInvoicesLabel.IsVisible = !InTransitInvoices.Any();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting invoices: {ex.Message}\n{ex.StackTrace}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner Exception: {ex.InnerException.Message}\n{ex.InnerException.StackTrace}");
            }
            // await DisplayAlert("Error", "Failed to retrieve invoices.", "OK");
            NoInvoicesLabel.IsVisible = !InTransitInvoices.Any();
        }
    }

    private async Task RefreshInvoices()
    {
        try
        {
            
            var authToken = await SecureStorage.GetAsync("authToken");
            if (authToken == null) return;

            var driverIdString = await SecureStorage.GetAsync("driverId");
            if (string.IsNullOrEmpty(driverIdString) || !int.TryParse(driverIdString, out var driverId)) return;

            var newInvoices = await _punderoApiService.GetInTransitInvoicesForDriverAsync(driverId);

            Device.BeginInvokeOnMainThread(() =>
            {
                foreach (var newInvoice in newInvoices)
                {
                    if (!InTransitInvoices.Any(i => i.IdInvoice == newInvoice.IdInvoice))
                    {
                        InTransitInvoices.Add(newInvoice);
                    }
                }

                NoInvoicesLabel.IsVisible = !InTransitInvoices.Any(); // Toggle visibility based on the count
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

    private async void OnChangeStatusButtonClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is int invoiceId)
        {
            try
            {
                await _punderoApiService.UpdateInvoiceStatusAsync(invoiceId, 5); // Set status to "Delivered"
                await DisplayAlert("Success", "Invoice status updated.", "OK");

                var invoice = InTransitInvoices.FirstOrDefault(i => i.IdInvoice == invoiceId);
                if (invoice != null)
                {
                    InTransitInvoices.Remove(invoice);
                }

                NoInvoicesLabel.IsVisible = !InTransitInvoices.Any(); // Toggle visibility based on the count
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
        _refreshTimer?.Stop();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _refreshTimer?.Start();
    }

    ~InTransitInvoicePage()
    {
        _refreshTimer?.Dispose();
    }
}
