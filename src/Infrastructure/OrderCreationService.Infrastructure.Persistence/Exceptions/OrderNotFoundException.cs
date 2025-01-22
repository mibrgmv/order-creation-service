namespace OrderCreationService.Infrastructure.Persistence.Exceptions;

public sealed class OrderNotFoundException : NotFoundException
{
    public OrderNotFoundException()
    {
    }

    public OrderNotFoundException(string? message) : base(message)
    {
    }

    public OrderNotFoundException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}