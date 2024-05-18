using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Vehicle;

namespace Vehicle
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                }).UseMauiMaps();
            builder.Services.AddSingleton<LoginPage>();
            builder.Services.AddSingleton<InvoicePage>();
            builder.Services.AddSingleton<MapPage>();
            //builder.Services.AddSingleton<Invoice>();
            builder.Services.AddSingleton<IMap>(Map.Default);
            builder.Services.AddSingleton<IGeolocation>(Geolocation.Default);
            builder.Services.AddSingleton<PunderoApiService>(_ => new PunderoApiService("http://10.0.2.2:8515"));
            

#if DEBUG
            builder.Logging.AddDebug();
#endif
            

            return builder.Build();
        }
    }
}
