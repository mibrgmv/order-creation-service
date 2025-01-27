using OrderCreationService.Application.Models.Enums;

namespace OrderCreationService.Application.Abstractions.Queries;

public record OrderQuery(
    long[] Ids,
    OrderState? OrderState,
    string? CreatedBy,
    int Cursor,
    int PageSize);