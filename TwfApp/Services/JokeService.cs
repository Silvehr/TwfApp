using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TwfApp.Application.Dtos;
using TwfApp.Application.Services;
using TwfApp.Core;
using TwfApp.Infrastructure;
using TwfApp.Types;
using TwfApp.Types.Config;

namespace TwfApp.Services;

public class JokeService : IJokeService
{
    public Joke? CurrentJoke { get; private set; }
    
    private readonly KierownikappDbContext _dbContext;
    private readonly KierownikappDbContextInitializer _dbContextInitializer;
    private readonly AppConfig _config;
    private readonly string _currentJokeFilePath;
    
    public JokeService(KierownikappDbContext db, KierownikappDbContextInitializer dbContextInitializer, AppConfig config)
    {
        _dbContext = db;
        _dbContextInitializer = dbContextInitializer;
        _config = config;
        _currentJokeFilePath = Path.Combine(config.AppDataDirectory, $"{config.CurrentJokeFileName}.json");
    }
    
    public async Task Initialize()
    {
        if (!File.Exists(_currentJokeFilePath))
        {
            await GenerateJoke();
        }
        else
        {
            var stream = File.OpenRead(_currentJokeFilePath);
            try
            {
                CurrentJoke = await JsonSerializer.DeserializeAsync<Joke>(stream);
                stream.Close();
                if (CurrentJoke!.CreationDate < DateTime.Today)
                {
                    await _dbContextInitializer.InitializedTask;
                    await GenerateJoke();
                }
            }
            catch (JsonException)
            {
                stream.Close();
                await _dbContextInitializer.InitializedTask;
                await GenerateJoke();
            }
        }
    }

    public async Task<Exception?> GenerateJoke()
    {
        var foodObject = await _dbContext.JokeObjects.OrderBy(jo => EF.Functions.Random())
            .FirstOrDefaultAsync(jo => jo.Type == JokeObjectType.Food);
        
        var otherObjects = await _dbContext.JokeObjects.OrderBy(jo => EF.Functions.Random()).Where(jo => jo.Type != JokeObjectType.Food).Take(4).ToListAsync();

        if (foodObject is null)
            return new InvalidOperationException("No food objects in database");
        
        if (otherObjects.Count != 4)
            return new InvalidOperationException("Not enough objects in database");

        var sittingPlace = otherObjects[0];
        var eatingObject = otherObjects[1];
        var flyingObject = otherObjects[2];
        var itemObject = otherObjects[3];
        CurrentJoke = new Joke($"Na <b>{sittingPlace.Name}</b> siedzą 2 <b>{eatingObject.Name}</b> i jedzą <b>{foodObject.Name}</b>. I tak sobie jedzą i jedzą, delektują się tymi <b>{foodObject.Name}</b>, gdy nagle przelatuje nad nimi <b>{flyingObject.Name}</b> &lt;NIOM!&gt;.<br><br><b>{eatingObject.Name}</b> patrzą się na ten <b>{flyingObject.Name}</b>, patrzą się na siebie i wracają do zajadania się tymi <b>{foodObject.Name}</b>. Nagle przelatuje nad nimi 2 <b>{flyingObject.Name}</b> &lt;NIOM&gt;! Liniowo niezależny z pierwszym o tym samym kierunku ale przeciwnym zwrocie.<br><br>Patrzą <b>{eatingObject.Name}</b> po sobie i jedna mówi do drugiej:<br> - \"Nisko dzisiaj latają\"<br>A druga na to:<br> - \"No, chyba muszą mieć tu jakąś <b>{itemObject.Name}</b>\"", DateTime.Today);

        Stream writeStream = !File.Exists(_currentJokeFilePath) 
            ? File.Create(_currentJokeFilePath) 
            : File.OpenWrite(_currentJokeFilePath);
        
        await JsonSerializer.SerializeAsync(writeStream, CurrentJoke);

        await writeStream.DisposeAsync();
        return null;
    }

    public async Task<Exception?> ImportData(JokeObjectExportData data)
    {
        if (!DataVersion.IsCompatible(_config.Version, data.Version))
            return new InvalidOperationException($"Cannot import data from version {data.Version} into version {_config.Version}");

        await _dbContext.JokeObjects.AddRangeAsync(data.JokeObjects.Select(dto => new Core.JokeObject(0, dto.Author, dto.Name, dto.Description, dto.Type, dto.CreationDate)));
        return null;
    }

    public async Task<JokeObjectExportData> ExportData()
    {
        return new JokeObjectExportData(_config.Version,
            _dbContext.JokeObjects.Select(joke =>
                new JokeObjectDto(joke.Author, joke.Name, joke.Description, joke.Type, joke.CreationDate)).ToList());
    }
}