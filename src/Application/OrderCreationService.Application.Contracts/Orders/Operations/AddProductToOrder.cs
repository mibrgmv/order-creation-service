namespace OrderCreationService.Application.Contracts.Orders.Operations;

public static class AddProductToOrder
{
    public readonly record struct Request(long ProductId, int Quantity);
}