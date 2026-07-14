using TwfApp.Core;
using TwfApp.Types;

namespace TwfApp.Application.Dtos;

public record JokeObjectDto(string Author, string Name, string? Description, JokeObjectType Type, DateTime CreationDate);