namespace OrderCreationService.Application.Contracts.Requests;

public record struct AddProductToOrderDto(long ProductId, int Quantity);