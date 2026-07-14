using TwfApp.Types.Config;

namespace TwfApp.Types.JokeGenerator
{
    public record JokeObjectExportData(DataVersion Version, List<JokeObjectDto> JokeObjects);
}
