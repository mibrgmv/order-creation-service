using OrderCreationService.Application.Models.Enums;

namespace OrderCreationService.Application.Models.Models;

public record Order(long OrderId, OrderState OrderState, DateTime OrderCreatedAt, string OrderCreatedBy);