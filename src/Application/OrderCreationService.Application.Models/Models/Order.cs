using OrderCreationService.Application.Models.Enums;

namespace OrderCreationService.Application.Models.Models;

public record Order(long OrderId, OrderState OrderState, DateTimeOffset OrderCreatedAt, string OrderCreatedBy);