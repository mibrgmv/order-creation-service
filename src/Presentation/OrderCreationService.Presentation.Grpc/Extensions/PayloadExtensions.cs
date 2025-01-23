using Orders.CreationService.Contracts;

namespace OrderCreationService.Presentation.Grpc.Extensions;

public static class PayloadExtensions
{
    public static BasePayload SerializePayload(Application.Models.Payloads.BasePayload payload)
    {
        return payload switch
        {
            Application.Models.Payloads.AddOrderPayload addOrderPayload => new BasePayload
            {
                AddOrder = new AddOrderPayload
                {
                    OrderCreatedBy = addOrderPayload.OrderCreatedBy,
                },
            },

            Application.Models.Payloads.AddProductToOrderPayload addProductToOrderPayload => new BasePayload
            {
                AddProductToOrder = new AddProductToOrderPayload
                {
                    ProductId = addProductToOrderPayload.ProductId,
                    Quantity = addProductToOrderPayload.Quantity,
                },
            },

            Application.Models.Payloads.RemoveProductFromOrderPayload removeProductFromOrderPayload =>
                new BasePayload
                {
                    RemoveProductFromOrder = new RemoveProductFromOrderPayload
                    {
                        ProductId = removeProductFromOrderPayload.ProductId,
                    },
                },

            Application.Models.Payloads.UpdateStatePayload updateStatePayload =>
                new BasePayload
                {
                    UpdateState = new UpdateStatePayload
                    {
                        NewState = (OrderState)updateStatePayload.NewState,
                        OldState = (OrderState)updateStatePayload.OldState,
                    },
                },

            _ => new BasePayload(),
        };
    }
}