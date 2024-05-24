using Newtonsoft.Json;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Devices.Sensors;
using System.Text;
using Microsoft.Maui.Maps;

namespace Vehicle;

public partial class MapPage : ContentPage
{
    private CancellationTokenSource _cancelTokenSource;
    private bool _isCheckingLocation;
    private readonly HttpClient _httpClient;
    private Circle currentLocationCircle;

    public MapPage()
    {
        InitializeComponent();
        _httpClient = new HttpClient();
        ConfigureMap();
    }

    private void ConfigureMap()
    {
        var sarajevoLocation = new Location(43.8563, 18.4131);
        var mapSpan = MapSpan.FromCenterAndRadius(sarajevoLocation, Distance.FromKilometers(2));
        map.MoveToRegion(mapSpan);

        // Initialize the circle for the current location
        currentLocationCircle = new Circle
        {
            Center = sarajevoLocation,
            Radius = new Distance(5), // 50 meters radius
            StrokeColor = Colors.Blue,
            StrokeWidth = 2,
            FillColor = new Color(0, 0, 1, 0) // Semi-transparent blue
        };
        map.MapElements.Add(currentLocationCircle);
    }

    private async void OnToggled(object sender, ToggledEventArgs e)
    {
        _isCheckingLocation = (bool)e.Value;

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
            while (_isCheckingLocation)
            {
                _cancelTokenSource = new CancellationTokenSource(); // New token for each request

                var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
                var location = await Geolocation.GetLocationAsync(request, _cancelTokenSource.Token);

                if (location != null)
                {
                    Console.WriteLine($"Latitude: {location.Latitude}, Longitude: {location.Longitude}, Altitude: {location.Altitude}");

                    // Update location in the database
                    await UpdateLocationInDatabase(location.Latitude, location.Longitude);

                    // Update the circle representing the current location
                    UpdateCurrentLocationCircle(location.Latitude, location.Longitude);
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

    private void UpdateCurrentLocationCircle(double latitude, double longitude)
    {
        var location = new Location(latitude, longitude);

        if (currentLocationCircle == null)
        {
            // Create a new circle if it doesn't exist
            currentLocationCircle = new Circle
            {
                Center = location,
                Radius = new Distance(5), // 50 meters radius
                StrokeColor = Colors.Blue,
                StrokeWidth = 2,
                FillColor = new Color(0, 0, 1, 0) // Semi-transparent blue
            };
            map.MapElements.Add(currentLocationCircle);
        }
        else
        {
            // Update the existing circle's center
            currentLocationCircle.Center = location;
        }

        // Move the map to the current location
        map.MoveToRegion(MapSpan.FromCenterAndRadius(location, Distance.FromMeters(500)));
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

            var response = await _httpClient.PutAsync("http://localhost:8515/api/Location/update", content);
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
