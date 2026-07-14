using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TwfApp.Application.Services;
using TwfApp.Infrastructure;
using TwfApp.Services;
using TwfApp.Types.Config;
using TwfApp.Types.Modals;

namespace TwfApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("ComicSans.ttf", "ComicSans");
                });

            var config = new AppConfig(new DataVersion(1, 1), FileSystem.AppDataDirectory, "currentJoke", "kierownikapp");
            builder.Services.AddSingleton(config);
            builder.Services.AddSingleton<ModalService>();
            builder.Services.AddDbContext<KierownikappDbContext>(opt =>
            {
                opt.UseSqlite($"Data Source={Path.Join(config.AppDataDirectory, config.JokesDatabaseName)}.db;");
            });
            
            builder.Services.AddSingleton<IJokeService, JokeService>();
            
            builder.Services.AddSingleton<KierownikappDbContextInitializer>();
            builder.Services.AddSingleton<IInitializer>(sp => sp.GetRequiredService<JokeServiceInitializer>());
            builder.Services.AddSingleton<IInitializer>(sp => sp.GetRequiredService<KierownikappDbContextInitializer>());
            builder.Services.AddSingleton<JokeServiceInitializer>();
            builder.Services.AddSingleton<AppInitializer>();
            
            builder.Services.AddMauiBlazorWebView();

#if DEBUG
    		builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif

            var app = builder.Build();

            _ = Task.Run(async () =>
            {
                using var scope = app.Services.CreateScope();
                var initializer = scope.ServiceProvider.GetRequiredService<AppInitializer>();
                await initializer.InitializeAsync();
            });
            
            
            
            return app;
        }
    }

    public sealed class JokeServiceInitializer(IServiceProvider services, KierownikappDbContextInitializer dbContextInitializer) : IInitializer
    {
        private readonly TaskCompletionSource _initializedTcs = new();
        
        public bool IsInitialized { get; private set; }
        public Task InitializedTask => _initializedTcs.Task;

        public async Task InitializeAsync()
        {
            await dbContextInitializer.InitializedTask;
            
            using var scope = services.CreateScope();
            var jokeService = scope.ServiceProvider.GetRequiredService<IJokeService>();
            await jokeService.Initialize();
            IsInitialized = true;
            _initializedTcs.SetResult();
        }
    }
    public sealed class KierownikappDbContextInitializer(IServiceProvider services) : IInitializer
    {
        private readonly TaskCompletionSource _initializedTcs = new();
        
        public bool IsInitialized { get; private set; }
        public Task InitializedTask => _initializedTcs.Task;

        public async Task InitializeAsync()
        {
            try
            {
                using var scope = services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<KierownikappDbContext>();

                await db.Database.MigrateAsync();
                
                await db.Database.ExecuteSqlRawAsync("""
                                               INSERT INTO "JokeObjects" (Author, Name, Description, Type, CreationDate, Editable) VALUES
                                               ('Gall Anonim', 'Sosna Limba', 'Podstawowa i niezmienna forma żartu kierownika', 0,'2023-06-29 00:00:00', 0),
                                               ('Gall Anonim', 'Krowa', 'Podstawowa i niezmienna forma żartu kierownika', 0,'2023-06-29 00:00:00', 0),
                                               ('Gall Anonim', 'Śliwka w Czekoladzie', 'Podstawowa i niezmienna forma żartu kierownika', 1,'2023-06-29 00:00:00', 0),
                                               ('Gall Anonim', 'Wektor', 'Podstawowa i niezmienna forma żartu kierownika', 0,'2023-06-29 00:00:00', 0),
                                               ('Gall Anonim', 'Baza', 'Podstawowa i niezmienna forma żartu kierownika', 0,'2023-06-29 00:00:00', 0) 
                                               ON CONFLICT(Name) DO NOTHING;
                                               """);
                IsInitialized = true;
                _initializedTcs.SetResult();
            }
            catch (Exception ex)
            {
                _initializedTcs.SetException(ex);
            }
        }
    }
}
