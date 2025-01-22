namespace OrderCreationService.Application.Models.Payloads;

public sealed class RemoveProductFromOrderPayload : BasePayload
{
    public RemoveProductFromOrderPayload(long productId)
    {
        ProductId = productId;
    }

    public long ProductId { get; set; }
}