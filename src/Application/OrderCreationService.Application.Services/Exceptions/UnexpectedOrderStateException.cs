namespace OrderCreationService.Application.Services.Exceptions;

public class UnexpectedOrderStateException : ConflictException
{
    public UnexpectedOrderStateException()
    {
    }

    public UnexpectedOrderStateException(string? message) : base(message)
    {
    }

    public UnexpectedOrderStateException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}