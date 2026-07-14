using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using TwfApp.Types.Config;
using TwfApp.Types.Results;

namespace TwfApp.Types
{
    public class JokeService(JokeObjectDb db, AppConfig config, ServiceStartupResultCollectorService resultCollector)
    {
        public JokeObjectDb Database { get; } = db;
        public Joke? CurrentJoke { get; private set; }
        public string CurrentJokeFilePath { get; } = Path.Combine(config.AppDataDirectory, $"{config.CurrentJokeFileName}.json");
        public bool Initialized { get; private set; } = false;
        private readonly ServiceStartupResultCollectorService _resultCollector = resultCollector;
        
        public async Task<Exception?> Initialize()
        {
            Exception? e = await Database.Initialize();

            if (e is not null) {
                return e;
            }

            if (!File.Exists(CurrentJokeFilePath))
            {
                e = await GenerateJoke();
                if (e is not null)
                    return e;
            }
            else
            {
                var stream = File.OpenRead(CurrentJokeFilePath);
                try
                {
                    CurrentJoke = await JsonSerializer.DeserializeAsync<Joke>(stream);
                    stream.Close();
                    if (CurrentJoke!.CreationDate < DateTime.Today){ 
                        e = await GenerateJoke();
                        if (e is not null)
                            return e;
                    }
                }
                catch (JsonException)
                {
                    stream.Close();
                    await GenerateJoke();
                }
                catch(Exception ex)
                {
                    return ex;
                }
            }

            Initialized = true;
            return null;
        }

        public Task Stop()
        {
            Initialized = false;
            return Task.CompletedTask;
        }

        public async Task<Exception?> GenerateJoke()
        {
            var foodObjectResult = await Database.GetRandomObjectsByCategory(JokeObjectType.Food, 1);
            var otherObjectResult = await Database.GetRandomObjectsByCategory(JokeObjectType.GenericObject, 4);

            if (!foodObjectResult.IsSuccess)
                return foodObjectResult.Error!;

            if (!otherObjectResult.IsSuccess)
                return otherObjectResult.Error!;

            var foodObject = (foodObjectResult.Value!)[0];
            var otherObjects = (otherObjectResult.Value!);
            var sittingPlace = otherObjects[0];
            var eatingObject = otherObjects[1];
            var flyingObject = otherObjects[2];
            var itemObject = otherObjects[3];
            CurrentJoke = new Joke($"Na <b>{sittingPlace.Name}</b> siedzą 2 <b>{eatingObject.Name}</b> i jedzą <b>{foodObject.Name}</b>. I tak sobie jedzą i jedzą, delektują się tymi <b>{foodObject.Name}</b>, gdy nagle przelatuje nad nimi <b>{flyingObject.Name}</b> &lt;NIOM!&gt;.<br><br><b>{eatingObject.Name}</b> patrzą się na ten <b>{flyingObject.Name}</b>, patrzą się na siebie i wracają do zajadania się tymi <b>{foodObject.Name}</b>. Nagle przelatuje nad nimi 2 <b>{flyingObject.Name}</b> &lt;NIOM&gt;! Liniowo niezależny z pierwszym o tym samym kierunku ale przeciwnym zwrocie.<br><br>Patrzą <b>{eatingObject.Name}</b> po sobie i jedna mówi do drugiej:<br> - \"Nisko dzisiaj latają\"<br>A druga na to:<br> - \"No, chyba muszą mieć tu jakąś <b>{itemObject.Name}</b>\"", DateTime.Today);

            Stream writeStream;
            if(!File.Exists(CurrentJokeFilePath))
                writeStream = File.Create(CurrentJokeFilePath);
            else
                writeStream = File.OpenWrite(CurrentJokeFilePath);


            await JsonSerializer.SerializeAsync(writeStream, CurrentJoke);

            writeStream.Dispose();
            return null;
        }

        public async Task<Exception?> ImportData(JokeObjectExportData data)
        {
            if (!DataVersion.IsCompatible(config.Version, data.Version))
                return new InvalidOperationException($"Cannot import data from version {data.Version} into version {config.Version}");

            return await Database.AppendRangeAsync(data.JokeObjects, true); 
        }

        public async Task<Result<JokeObjectExportData, Exception>> ExportData()
        {
            var result = await Database.GetJokeObjectsAsync();
            if (result.IsSuccess)
                return new(new JokeObjectExportData(config.Version, [.. (result.Value!).Select(j => new JokeObjectDto(j.Author, j.Name, j.Description, j.Type, j.CreationDate))]));
            else
                return new(result.Error!);
        }
    }
}
