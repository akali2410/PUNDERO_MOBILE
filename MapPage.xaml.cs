namespace Vehicle;

public partial class MapPage : ContentPage
{
    private CancellationTokenSource _cancelTokenSource;
    private bool _isCheckingLocation;

    public MapPage()
    {
        InitializeComponent();
    }

    private async void OnToggled(object sender, ToggledEventArgs e)
    {
        _isCheckingLocation = (bool)e.Value; // Set flag based on toggle state

        if (_isCheckingLocation)
        {
            await GetCurrentLocation(); // Start location updates if toggle is on
        }
        else
        {
            CancelRequest(); // Stop location updates if toggle is off
        }
    }

    public async Task GetCurrentLocation()
    {
        try
        {
            while (_isCheckingLocation) // Loop while the toggle is on
            {
                _cancelTokenSource = new CancellationTokenSource(); // New token for each request

                GeolocationRequest request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));

                Location location = await Geolocation.Default.GetLocationAsync(request, _cancelTokenSource.Token);

                if (location != null)
                {
                    // Update UI with location data (replace with your logic)
                    Console.WriteLine($"Latitude: {location.Latitude}, Longitude: {location.Longitude}, Altitude: {location.Altitude}");
                }

                await Task.Delay(TimeSpan.FromSeconds(10)); // Wait 10 seconds before next request
            }
        }
        catch (FeatureNotSupportedException fnsEx)
        {
            Console.WriteLine($"Error Location: {fnsEx.Message}");
        }
        catch (FeatureNotEnabledException fneEx)
        {
            Console.WriteLine($"Error Location: {fneEx.Message}");
        }
        catch (PermissionException pEx)
        {
            Console.WriteLine($"Error Location: {pEx.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error Location: {ex.Message}");
        }
        finally
        {
            _cancelTokenSource = null; // Clear token after loop ends
        }
    }

    public void CancelRequest()
    {
        if (_isCheckingLocation && _cancelTokenSource != null && _cancelTokenSource.IsCancellationRequested == false)
        {
            _cancelTokenSource.Cancel();
        }
    }
}
