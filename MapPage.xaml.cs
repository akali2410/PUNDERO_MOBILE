using Newtonsoft.Json;
using System.Text;

namespace Vehicle;

public partial class MapPage : ContentPage
{
    private CancellationTokenSource _cancelTokenSource;
    private bool _isCheckingLocation;
    private readonly HttpClient _httpClient;

    public MapPage()
    {
        InitializeComponent();
        _httpClient = new HttpClient();
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
                    Console.WriteLine($"Latitude: {location.Latitude}, Longitude: {location.Longitude}, Altitude: {location.Altitude}");

                    // Call method to update location in database
                    await UpdateLocationInDatabase(location.Latitude, location.Longitude);
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

    private async Task UpdateLocationInDatabase(double latitude, double longitude)
    {
        try
        {
            Console.WriteLine("Starting UpdateLocationInDatabase");

            var authToken = await SecureStorage.GetAsync("authToken");
            if (authToken == null)
            {
                Console.WriteLine("Auth token is missing.");
                return;
            }
            Console.WriteLine($"Auth token retrieved: {authToken}");

            var updateLocationDto = new UpdateLocationDto
            {
                Latitude = latitude,
                Longitude = longitude
            };
            Console.WriteLine($"UpdateLocationDto created: Latitude={updateLocationDto.Latitude}, Longitude={updateLocationDto.Longitude}");

            var json = JsonConvert.SerializeObject(updateLocationDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            Console.WriteLine($"JSON content created: {json}");

            if (_httpClient == null)
            {
                Console.WriteLine("HTTP Client is null");
                return;
            }

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("my-auth-token", authToken);
            Console.WriteLine("Custom authorization header set");

            var response = await _httpClient.PutAsync("http://10.0.2.2:8515/api/Location/update", content);
            Console.WriteLine("Request sent");

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Location updated successfully.");
            }
            else
            {
                Console.WriteLine($"Failed to update location. StatusCode: {response.StatusCode}");
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Response content: {responseContent}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating location: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
            }
        }
    }

    public void CancelRequest()
    {
        if (_isCheckingLocation && _cancelTokenSource != null && !_cancelTokenSource.IsCancellationRequested)
        {
            _cancelTokenSource.Cancel();
        }
    }

    public class UpdateLocationDto
    {
        public double Longitude { get; set; }
        public double Latitude { get; set; }
    }
}
