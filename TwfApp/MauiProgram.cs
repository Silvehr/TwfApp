using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using SQLitePCL;
using TwfApp.Types;
using TwfApp.Types.Config;
using TwfApp.Types.JokeGenerator;
using TwfApp.Types.Modals;

namespace TwfApp
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
                });

            builder.Services.AddSingleton<ModalService>();
            builder.Services.AddSingleton<ServiceStartupResultCollectorService>();
            builder.Services.AddSingleton(new AppConfig(new DataVersion(1,1), FileSystem.AppDataDirectory, "currentJoke", "jokes"));
            builder.Services.AddSingleton(new SqliteConnection($"Data Source={FileSystem.AppDataDirectory}/jokes.db"));
            builder.Services.AddSingleton<JokeObjectDb>();
            builder.Services.AddSingleton<JokeService>();
            builder.Services.AddSingleton<AppInitializer>();

            builder.Services.AddMauiBlazorWebView();

#if DEBUG
    		builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }

    public class AppInitializer
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ServiceStartupResultCollectorService _resultCollector;
        public AppInitializer(IServiceProvider serviceProvider, ServiceStartupResultCollectorService resultCollector)
        {
            _serviceProvider = serviceProvider;
            _resultCollector = resultCollector;
        }

        public async Task InitializeAsync()
        {
            await _serviceProvider.GetRequiredService<SqliteConnection>().OpenAsync();
            _resultCollector.AddServiceInitializationResult<JokeObjectDb>(await _serviceProvider.GetRequiredService<JokeObjectDb>().Initialize());
            _resultCollector.AddServiceInitializationResult<JokeService>(await _serviceProvider.GetRequiredService<JokeService>().Initialize());
        }

        public async Task ReinitializeAsync()
        {
            await StopServicesAsync();

            await ClearStateAsync();

            await InitializeAsync();
        }

        private async Task StopServicesAsync()
        {
            await _serviceProvider.GetRequiredService<SqliteConnection>().CloseAsync();
            await _serviceProvider.GetRequiredService<JokeObjectDb>().Stop();
            await _serviceProvider.GetRequiredService<JokeService>().Stop();
        }

        private Task ClearStateAsync()
        {
            var jokeService = _serviceProvider.GetRequiredService<JokeService>();
            File.Delete(jokeService.Database.DbFilePath);
            File.Delete(jokeService.CurrentJokeFilePath);
            return Task.CompletedTask;
        }
    }

    public class ServiceStartupResultCollectorService
    {
        private readonly Dictionary<Type, Exception?> _serviceResults = [];

        public void AddServiceInitializationResult<T>(Exception? result)
        {
            if(!_serviceResults.TryAdd(typeof(T), result))
                _serviceResults[typeof(T)] = result;
        }

        public Exception? GetServiceInitializationResult<T>()
        {
            return _serviceResults[typeof(T)];
        }
    }
}
