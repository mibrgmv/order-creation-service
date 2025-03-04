namespace OrderCreationService.Application.Contracts.Orders.Operations;

public static class AddOrder
{
    public readonly record struct Request(string OrderCreatedBy);
}