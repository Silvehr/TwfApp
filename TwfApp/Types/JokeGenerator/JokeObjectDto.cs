namespace TwfApp.Types.JokeGenerator;

public record JokeObjectDto(string Author, string Name, string? Description, JokeObjectType Type, DateTime CreationDate);