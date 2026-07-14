namespace TwfApp.Types.Results;

public class InvalidResultAccess : Exception
{
    public InvalidResultAccess() : base("Cannot access Result values before checking result state")
    {
    }
}