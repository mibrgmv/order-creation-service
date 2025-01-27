namespace OrderCreationService.Application.Services.Exceptions;

public sealed class ProductNotFoundException : NotFoundException
{
    public ProductNotFoundException()
    {
    }

    public ProductNotFoundException(string? message) : base(message)
    {
    }

    public ProductNotFoundException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}