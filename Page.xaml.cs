namespace Vehicle;

public partial class Page : ContentPage
{

   
        private readonly PunderoApiService _punderoApiService;

   

    public Page(PunderoApiService punderoApiService) // Inject PunderoApiService in constructor
        {
            InitializeComponent();
            _punderoApiService = punderoApiService;
             
    }

        private async void LoadInvoicesButton_Clicked(object sender, EventArgs e)
        {

            try
            {
                var invoices = await _punderoApiService.GetInvoicesAsync();
                
            // Update your UI with the retrieved invoices
            InvoicesListView.ItemsSource = invoices;
            }
            catch (Exception ex)
            {
                // Handle errors (e.g., display an error message)
                Console.WriteLine($"Error getting invoices: {ex.Message}");
                await DisplayAlert("Error", "Failed to retrieve invoices.", "OK");
            }
        }
    }


