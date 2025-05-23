using OrderCreationService.Application.Models.Enums;
using OrderCreationService.Application.Models.Payloads;

namespace OrderCreationService.Application.Models.Models;

public record OrderHistoryItem(
    long OrderHistoryItemId,
    long OrderId,
    DateTimeOffset OrderHistoryItemCreatedAt,
    OrderHistoryItemKind OrderHistoryItemKind,
    BasePayload Payload);