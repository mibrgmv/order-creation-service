using OrderCreationService.Application.Models.Enums;

namespace OrderCreationService.Application.Abstractions.Queries;

public record OrderQuery(
    long[] Ids,
    OrderState? State,
    string? CreatedBy,
    int Cursor,
    int PageSize);