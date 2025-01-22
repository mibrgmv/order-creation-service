namespace OrderCreationService.Application.Abstractions.Requests;

public record struct AddProductToOrderDto(long ProductId, int Quantity);