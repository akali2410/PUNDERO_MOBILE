using Newtonsoft.Json;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Devices.Sensors;
using System.Text;
using System.Diagnostics;
using Microsoft.Maui.Maps;

namespace Vehicle;

public partial class MapPage : ContentPage
{
    private CancellationTokenSource _cancelTokenSource;
    private bool _isCheckingLocation;
    private readonly HttpClient _httpClient;
    private Pin currentLocationPin;

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

    private async Task GetCurrentLocation()
    {
        try
        {
            while (_isCheckingLocation) // Loop while the toggle is on
            {
                _cancelTokenSource = new CancellationTokenSource(); // New token for each request

                var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
                var location = await Geolocation.GetLocationAsync(request, _cancelTokenSource.Token);

                if (location != null)
                {
                    Debug.WriteLine($"Latitude: {location.Latitude}, Longitude: {location.Longitude}, Altitude: {location.Altitude}");

                    // Initialize or update the current location pin
                    UpdateMapWithCurrentLocation(location.Latitude, location.Longitude);

                    // Call method to update location in database
                    await UpdateLocationInDatabase(location.Latitude, location.Longitude);
                }

                await Task.Delay(TimeSpan.FromSeconds(10)); // Wait 10 seconds before next request
            }
        }
        catch (FeatureNotSupportedException fnsEx)
        {
            Debug.WriteLine($"Error Location: {fnsEx.Message}");
        }
        catch (FeatureNotEnabledException fneEx)
        {
            Debug.WriteLine($"Error Location: {fneEx.Message}");
        }
        catch (PermissionException pEx)
        {
            Debug.WriteLine($"Error Location: {pEx.Message}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error Location: {ex.Message}");
        }
        finally
        {
            _cancelTokenSource = null; // Clear token after loop ends
        }
    }

    private void UpdateMapWithCurrentLocation(double latitude, double longitude)
    {
        var location = new Location(latitude, longitude);

        if (location != null)
        {
            if (currentLocationPin == null)
            {
                currentLocationPin = new Pin
                {
                    Label = "Current Location",
                    Type = PinType.SavedPin,
                    Location = location,
                    
                };
                map.Pins.Add(currentLocationPin);
            }
            else
            {
                currentLocationPin.Location = location;
            }

            // Move the map to the current location
            //map.MoveToRegion(MapSpan.FromCenterAndRadius(location, Distance.FromMeters(500)));
        }
    }

    private async Task UpdateLocationInDatabase(double latitude, double longitude)
    {
        try
        {
            Debug.WriteLine("Starting UpdateLocationInDatabase");

            var authToken = await SecureStorage.GetAsync("authToken");
            if (authToken == null)
            {
                Debug.WriteLine("Auth token is missing.");
                return;
            }
            Debug.WriteLine($"Auth token retrieved: {authToken}");

            var updateLocationDto = new UpdateLocationDto
            {
                Latitude = latitude,
                Longitude = longitude
            };
            Debug.WriteLine($"UpdateLocationDto created: Latitude={updateLocationDto.Latitude}, Longitude={updateLocationDto.Longitude}");

            var json = JsonConvert.SerializeObject(updateLocationDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            Debug.WriteLine($"JSON content created: {json}");

            if (_httpClient == null)
            {
                Debug.WriteLine("HTTP Client is null");
                return;
            }

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("my-auth-token", authToken);
            Debug.WriteLine("Custom authorization header set");

            var response = await _httpClient.PutAsync("http://10.0.2.2:8515/api/Location/update", content);
            Debug.WriteLine("Request sent");

            if (response.IsSuccessStatusCode)
            {
                Debug.WriteLine("Location updated successfully.");
            }
            else
            {
                Debug.WriteLine($"Failed to update location. StatusCode: {response.StatusCode}");
                var responseContent = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"Response content: {responseContent}");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error updating location: {ex.Message}");
            if (ex.InnerException != null)
            {
                Debug.WriteLine($"Inner Exception: {ex.InnerException.Message}");
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