using System;
using System.Collections.Generic;
using System.Text;

namespace TwfApp.Types;

public record JokeObjectDto(string Author, string Name, string? Description, JokeObjectType Type, DateTime CreationDate);