namespace OrderCreationService.Application.Models.Payloads;

public sealed class AddProductToOrderPayload : BasePayload
{
    public AddProductToOrderPayload(long productId, int quantity)
    {
        ProductId = productId;
        Quantity = quantity;
    }

    public long ProductId { get; set; }

    public int Quantity { get; set; }
}