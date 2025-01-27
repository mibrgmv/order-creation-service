namespace OrderCreationService.Application.Contracts.Requests;

public record struct AddProductDto(string Name, decimal Price);