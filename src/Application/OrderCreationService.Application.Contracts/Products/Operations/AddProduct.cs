namespace OrderCreationService.Application.Contracts.Products.Operations;

public static class AddProduct
{
    public readonly record struct Request(string Name, decimal Price);
}