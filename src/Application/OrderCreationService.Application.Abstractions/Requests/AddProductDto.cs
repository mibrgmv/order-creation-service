namespace OrderCreationService.Application.Abstractions.Requests;

public record struct AddProductDto(string Name, decimal Price);