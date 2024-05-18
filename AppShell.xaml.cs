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
    }
}
