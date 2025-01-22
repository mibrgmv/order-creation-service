namespace OrderCreationService.Application.Services.Exceptions;

public class ConflictException : Exception
{
    protected ConflictException()
    {
    }

    protected ConflictException(string? message) : base(message)
    {
    }

    protected ConflictException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}