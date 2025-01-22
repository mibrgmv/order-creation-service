namespace OrderCreationService.Application.Services.Exceptions;

public class InvalidOrderStateException : ConflictException
{
    public InvalidOrderStateException()
    {
    }

    public InvalidOrderStateException(string? message) : base(message)
    {
    }

    public InvalidOrderStateException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}