using Newtonsoft.Json;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Devices.Sensors;
using System.Text;
using Microsoft.Maui.Maps;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using System.Collections.Generic;
using System.Linq;

namespace Vehicle;

public partial class MapPage : ContentPage
{
    private CancellationTokenSource _cancelTokenSource;
    private bool _isCheckingLocation;
    private readonly HttpClient _httpClient;
    private PunderoApiService _punderoApiService;
    private Circle currentLocationCircle;
    private Pin warehousePin;

    public MapPage()
    {
        InitializeComponent();
        _httpClient = new HttpClient();
        ConfigureMap();
        LoadWarehouseLocation();
        LoadInTransitStores();

        map.PropertyChanged += Map_PropertyChanged;
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
            Radius = new Distance(10),// meters radius
            StrokeColor = Colors.Blue,
            StrokeWidth = 2,
            FillColor = new Color(0, 0, 255) // Semi-transparent blue
        };
        map.MapElements.Add(currentLocationCircle);
    }

    private void Map_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "VisibleRegion")
        {
            UpdateCircleRadiusBasedOnZoom();
        }
    }

    private void UpdateCircleRadiusBasedOnZoom()
    {
        if (map.VisibleRegion == null)
        {
            return;
        }

        var zoomLevel = map.VisibleRegion.Radius.Kilometers;

        // Update the circle radius based on zoom level
        var newRadius = CalculateRadius(zoomLevel);
        currentLocationCircle.Radius = new Distance(newRadius);
    }

    private double CalculateRadius(double zoomLevel)
    {
        // Adjust this calculation based on your specific requirements
        // Here, we set a minimum radius of 5 meters and increase it proportionally with zoom level
        return Math.Max(5, zoomLevel * 50);
    }

    private async void LoadWarehouseLocation()
    {
        try
        {
            var response = await _httpClient.GetStringAsync("http://localhost:8515/api/Warehouses/GetWarehouseLocation");
            var warehouse = JsonConvert.DeserializeObject<WarehouseDto>(response);

            Device.BeginInvokeOnMainThread(() =>
            {
                var location = new Location(warehouse.Latitude, warehouse.Longitude);
                warehousePin = new Pin
                {
                    Label = "Warehouse",
                    Address = warehouse.Address,
                    Type = PinType.Place,
                    Location = location
                };
                 
                map.Pins.Add(warehousePin);
                map.MoveToRegion(MapSpan.FromCenterAndRadius(location, Distance.FromKilometers(2)));
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading warehouse location: {ex.Message}");
        }
    }

    private async void OnToggled(object sender, ToggledEventArgs e)
    {
        _isCheckingLocation = e.Value;

        if (_isCheckingLocation)
        {
            await GetCurrentLocation();// Start location updates if toggle is on
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
                    LoadInTransitStores();
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
                Radius = new Distance(10), // 50 meters radius
                StrokeColor = Colors.Blue,
                StrokeWidth = 2,
                FillColor = new Color(0, 0, 255) // Semi-transparent blue
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

    private async void LoadInTransitStores()
    {
        try
        {
            var authToken = await SecureStorage.GetAsync("authToken");
            if (authToken == null) return;

            var driverIdString = await SecureStorage.GetAsync("driverId");
            if (string.IsNullOrEmpty(driverIdString) || !int.TryParse(driverIdString, out var driverId)) return;

            var response = await _httpClient.GetStringAsync($"http://localhost:8515/api/Invoice/getInTransitInvoicesForDriver/{driverId}");
            var invoices = JsonConvert.DeserializeObject<List<PunderoApiService.InvoiceDto>>(response);

            Device.BeginInvokeOnMainThread(() =>
            {
                foreach (var invoice in invoices)
                {
                    var location = new Location(invoice.StoreLatitude, invoice.StoreLongitude);
                    var pin = new Pin
                    {
                        Label = invoice.StoreName,
                        Type = PinType.Place,
                        Location = location
                    };

                    map.Pins.Add(pin);
                }
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading in-transit stores: {ex.Message}");
        }
    }
    private async Task<string> GetOptimizedRouteAsync(List<Location> locations)
    {
        var apiKey = "AIzaSyCj7-nNBHWtcscx1pCQBhUen9kNjMoB9pA"; // Replace with your actual API key

        var waypoints = string.Join("|", locations.Skip(1).Take(locations.Count - 2).Select(loc => $"{loc.Latitude},{loc.Longitude}"));
        var requestUri = $"https://maps.googleapis.com/maps/api/directions/json?origin={locations.First().Latitude},{locations.First().Longitude}&destination={locations.Last().Latitude},{locations.Last().Longitude}&waypoints=optimize:true|{waypoints}&key={apiKey}";

        try
        {
            var response = await _httpClient.GetStringAsync(requestUri);
            return response;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting optimized route: {ex.Message}");
            return null;
        }
    }

    private async Task DisplayOptimizedRoute(List<Location> locations)
    {
        var response = await GetOptimizedRouteAsync(locations);

        if (response == null)
        {
            await DisplayAlert("Error", "Failed to get optimized route.", "OK");
            return;
        }

        var directionsResponse = JsonConvert.DeserializeObject<GoogleDirectionsResponse>(response);
        var route = directionsResponse.Routes.FirstOrDefault();

        if (route == null)
        {
            await DisplayAlert("Error", "No routes found.", "OK");
            return;
        }

        var polyline = new Microsoft.Maui.Controls.Maps.Polyline
        {
            StrokeColor = Colors.Blue,
            StrokeWidth = 5
        };

        foreach (var leg in route.Legs)
        {
            foreach (var step in leg.Steps)
            {
                var points = DecodePolyline(step.Polyline.Points);
                foreach (var point in points)
                {
                    polyline.Geopath.Add(new Location(point.Latitude, point.Longitude));
                }
            }
        }

        map.MapElements.Clear();
        map.MapElements.Add(polyline);
        if (currentLocationCircle != null && !map.MapElements.Contains(currentLocationCircle))
        {
            map.MapElements.Add(currentLocationCircle);
        }
    }

    private List<Location> DecodePolyline(string encodedPoints)
    {
        if (string.IsNullOrEmpty(encodedPoints))
        {
            return null;
        }

        var poly = new List<Location>();
        var polylineChars = encodedPoints.ToCharArray();
        var index = 0;
        var currentLat = 0;
        var currentLng = 0;

        while (index < polylineChars.Length)
        {
            var sum = 0;
            var shifter = 0;
            int next5Bits;

            do
            {
                next5Bits = polylineChars[index++] - 63;
                sum |= (next5Bits & 31) << shifter;
                shifter += 5;
            } while (next5Bits >= 32 && index < polylineChars.Length);

            if (index >= polylineChars.Length)
            {
                break;
            }

            currentLat += (sum & 1) == 1 ? ~(sum >> 1) : (sum >> 1);

            sum = 0;
            shifter = 0;

            do
            {
                next5Bits = polylineChars[index++] - 63;
                sum |= (next5Bits & 31) << shifter;
                shifter += 5;
            } while (next5Bits >= 32 && index < polylineChars.Length);

            if (index >= polylineChars.Length && next5Bits >= 32)
            {
                break;
            }

            currentLng += (sum & 1) == 1 ? ~(sum >> 1) : (sum >> 1);

            var lat = currentLat / 1E5;
            var lng = currentLng / 1E5;
            poly.Add(new Location(lat, lng));
        }

        return poly;
    }

    private async void OnOptimizeRouteButtonClicked(object sender, EventArgs e)
    {
        var locations = new List<Location>();

        // Add current location
        var currentLocation = await Geolocation.GetLastKnownLocationAsync();
        if (currentLocation != null)
        {
            locations.Add(new Location(currentLocation.Latitude, currentLocation.Longitude));
        }

        // Add store locations
        var driverIdString = await SecureStorage.GetAsync("driverId");
        if (!string.IsNullOrEmpty(driverIdString) && int.TryParse(driverIdString, out var driverId))
        {
            var response = await _httpClient.GetStringAsync($"http://localhost:8515/api/Invoice/getInTransitInvoicesForDriver/{driverId}");
            var invoices = JsonConvert.DeserializeObject<List<PunderoApiService.InvoiceDto>>(response);

            foreach (var invoice in invoices)
            {
                locations.Add(new Location(invoice.StoreLatitude, invoice.StoreLongitude));
            }
        }

        // Add warehouse location
        var warehouseResponse = await _httpClient.GetStringAsync("http://localhost:8515/api/Warehouses/GetWarehouseLocation");
        var warehouse = JsonConvert.DeserializeObject<WarehouseDto>(warehouseResponse);
        locations.Add(new Location(warehouse.Latitude, warehouse.Longitude));

        await DisplayOptimizedRoute(locations);
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

    public class WarehouseDto
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Address { get; set; }
    }

    public class GoogleDirectionsResponse
    {
        [JsonProperty("routes")]
        public List<Route> Routes { get; set; }
    }

    public class Route
    {
        [JsonProperty("legs")]
        public List<Leg> Legs { get; set; }
    }

    public class Leg
    {
        [JsonProperty("steps")]
        public List<Step> Steps { get; set; }
    }

    public class Step
    {
        [JsonProperty("polyline")]
        public Polyline Polyline { get; set; }
    }

    public class Polyline
    {
        [JsonProperty("points")]
        public string Points { get; set; }
    }
}