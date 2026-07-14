using System;
using System.Collections.Generic;
using System.Text;
using TwfApp.Application.Dtos;
using TwfApp.Types.Config;

namespace TwfApp.Types
{
    public record JokeObjectExportData(DataVersion Version, List<JokeObjectDto> JokeObjects);
}
