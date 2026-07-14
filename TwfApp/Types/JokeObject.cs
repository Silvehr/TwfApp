using System;
using System.Collections.Generic;
using System.Text;

namespace TwfApp.Types
{
    public enum JokeObjectType
    {
        GenericObject = 0,
        Food = 1
    }

    public class JokeObject(int id, string author, string name, string? description, JokeObjectType type, DateTime creationDate, bool editable = true)
    {
        public int Id { get; set; } = id;
        public string Author { get; set; } = author;
        public string Name { get; set; } = name;
        public string? Description { get; set; } = description;
        public JokeObjectType Type { get; set; }  = type;
        public DateTime CreationDate { get; set; } = creationDate;
        public bool Editable { get; set; } = editable;
    }
}
