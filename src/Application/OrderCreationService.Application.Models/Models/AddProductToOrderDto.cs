namespace OrderCreationService.Application.Models.Models;

public record struct AddProductToOrderDto(long ProductId, int Quantity);