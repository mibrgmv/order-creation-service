using OrderCreationService.Application.Models.Enums;

namespace OrderCreationService.Application.Models.Payloads;

public sealed class UpdateStatePayload : BasePayload
{
    public UpdateStatePayload(OrderState oldState, OrderState newState)
    {
        OldState = oldState;
        NewState = newState;
    }

    public OrderState OldState { get; set; }

    public OrderState NewState { get; set; }
}