namespace OrderCreationService.Application.Models.Payloads;

public sealed class AddOrderPayload : BasePayload
{
    public AddOrderPayload(string orderCreatedBy)
    {
        OrderCreatedBy = orderCreatedBy;
    }

    public string OrderCreatedBy { get; set; }
}