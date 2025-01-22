namespace OrderCreationService.Application.Models.Models;

public record OrderItem(
    long OrderItemId,
    long OrderId,
    long ProductId,
    int OrderItemQuantity,
    bool OrderItemDeleted);