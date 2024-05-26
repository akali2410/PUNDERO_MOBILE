namespace Vehicle
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Shell.SetBackgroundColor(this, Color.FromArgb("#1976d2"));
            Shell.SetTitleColor(this, Colors.White);
            Shell.SetForegroundColor(this, Colors.White);
            
        }
        private async void OnSignOutClicked(object sender, EventArgs e)
        {
            // Clear the authentication token or any other user data
            await SecureStorage.SetAsync("authToken", string.Empty);
            await SecureStorage.SetAsync("driverId", string.Empty);

            // Navigate to the login page
            Application.Current.MainPage = new NavigationPage(new LoginPage());
        }
    }
}
