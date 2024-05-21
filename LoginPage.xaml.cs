using Newtonsoft.Json;
using System.Text;
using System.Text.Encodings.Web;

namespace Vehicle;

public partial class LoginPage : ContentPage
{
    private readonly HttpClient _httpClient;

    public LoginPage()
    {
        InitializeComponent();
        _httpClient = new HttpClient();
    }

    private async void LoginButton_Clicked(object sender, EventArgs e)
    {
        var email = EmailEntry.Text;
        var password = PasswordEntry.Text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            StatusLabel.Text = "Please enter email and password";
            return;
        }

        StatusLabel.Text = ""; // Clear previous message

        try
        {
            var url = "http://10.0.2.2:8515/auth/mobile/login";               
            var loginRequest = new LoginRequest { Email = email, Password = password };
            var content = new StringContent(JsonConvert.SerializeObject(loginRequest), Encoding.UTF8, "application/json");

            Console.WriteLine($"Sending login request to {url}"); // Log request details

            var response = await _httpClient.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();

                // Assuming your Web API returns a JSON object with a "token" property
                try
                {
                    var loginResponse = JsonConvert.DeserializeObject<LoginResponse>(responseContent);
                    if (loginResponse?.TokenValue != null)
                    {
                        await SecureStorage.SetAsync("authToken", loginResponse.TokenValue);
                        await SecureStorage.SetAsync("driverId", loginResponse.DriverId.ToString());

                        // Navigate to main page or perform other actions with the token
                        Application.Current.MainPage = new AppShell();
                        await Shell.Current.GoToAsync("//MapPage");

                    }
                    else
                    {
                        StatusLabel.Text = "Login failed: Incorrect User Data";
                    }
                }
                catch (JsonException ex)
                {
                    Console.WriteLine($"Error parsing login response: {ex.Message}");
                    StatusLabel.Text = "An error occurred (parsing response)";
                }
            }
            else
            {
                StatusLabel.Text = $"Login failed: {response.StatusCode}";
            }
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Network error during login: {ex.Message}");
            StatusLabel.Text = "Connection failed"; // Or provide a more user-friendly error message
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error during login: {ex.Message}");
            StatusLabel.Text = "An error occurred"; // Or provide a more user-friendly error message
        }
    }

    public class LoginResponse
    {
        public string TokenValue { get; set; }
        public int? DriverId { get; set; }
    }

    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
