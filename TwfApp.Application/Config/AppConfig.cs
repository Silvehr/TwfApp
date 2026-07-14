using System;
using System.Collections.Generic;
using System.Text;

namespace TwfApp.Types.Config
{
    public record AppConfig(DataVersion Version, string AppDataDirectory, string CurrentJokeFileName, string JokesDatabaseName);
}
