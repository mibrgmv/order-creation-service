using Orders.CreationService.Contracts;
using CsharpPayloads = OrderCreationService.Application.Models.Payloads;

namespace OrderCreationService.Presentation.Grpc.Extensions;

public static class PayloadExtensions
{
    public static BasePayload SerializePayload(CsharpPayloads.BasePayload payload)
    {
        return payload switch
        {
            CsharpPayloads.AddOrderPayload addOrderPayload => new BasePayload
            {
                AddOrder = new AddOrderPayload
                {
                    OrderCreatedBy = addOrderPayload.OrderCreatedBy,
                },
            },

            CsharpPayloads.AddProductToOrderPayload addProductToOrderPayload => new BasePayload
            {
                AddProductToOrder = new AddProductToOrderPayload
                {
                    ProductId = addProductToOrderPayload.ProductId,
                    Quantity = addProductToOrderPayload.Quantity,
                },
            },

            CsharpPayloads.RemoveProductFromOrderPayload removeProductFromOrderPayload =>
                new BasePayload
                {
                    RemoveProductFromOrder = new RemoveProductFromOrderPayload
                    {
                        ProductId = removeProductFromOrderPayload.ProductId,
                    },
                },

            CsharpPayloads.UpdateStatePayload updateStatePayload =>
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

    public static CsharpPayloads.BasePayload DeserializePayload(BasePayload payload)
    {
        return payload.PayloadCase switch
        {
            BasePayload.PayloadOneofCase.AddOrder =>
                new CsharpPayloads.AddOrderPayload(
                    orderCreatedBy: payload.AddOrder.OrderCreatedBy),

            BasePayload.PayloadOneofCase.AddProductToOrder =>
                new CsharpPayloads.AddProductToOrderPayload(
                    productId: payload.AddProductToOrder.ProductId,
                    quantity: payload.AddProductToOrder.Quantity),

            BasePayload.PayloadOneofCase.RemoveProductFromOrder =>
                new CsharpPayloads.RemoveProductFromOrderPayload(
                    productId: payload.RemoveProductFromOrder.ProductId),

            BasePayload.PayloadOneofCase.UpdateState =>
                new CsharpPayloads.UpdateStatePayload(
                    oldState: (Application.Models.Enums.OrderState)payload.UpdateState.OldState,
                    newState: (Application.Models.Enums.OrderState)payload.UpdateState.NewState),

            BasePayload.PayloadOneofCase.None
                => new CsharpPayloads.BasePayload(),

            _ => new CsharpPayloads.BasePayload(),
        };
    }
}