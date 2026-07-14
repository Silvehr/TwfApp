using TwfApp.Core;
using TwfApp.Types;

namespace TwfApp.Application.Services;

public interface IJokeService
{
    Joke? CurrentJoke { get; }
    Task Initialize();
    Task<Exception?> GenerateJoke();
    Task<Exception?> ImportData(JokeObjectExportData data);
    Task<JokeObjectExportData> ExportData();
}