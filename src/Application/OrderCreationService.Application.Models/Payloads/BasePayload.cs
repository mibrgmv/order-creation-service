using System.Text.Json.Serialization;

namespace OrderCreationService.Application.Models.Payloads;

[JsonDerivedType(typeof(BasePayload), typeDiscriminator: nameof(BasePayload))]
[JsonDerivedType(typeof(AddOrderPayload), typeDiscriminator: nameof(AddOrderPayload))]
[JsonDerivedType(typeof(AddProductToOrderPayload), typeDiscriminator: nameof(AddProductToOrderPayload))]
[JsonDerivedType(typeof(OrderProcessingStatusPayload), typeDiscriminator: nameof(OrderProcessingStatusPayload))]
[JsonDerivedType(typeof(RemoveProductFromOrderPayload), typeDiscriminator: nameof(RemoveProductFromOrderPayload))]
[JsonDerivedType(typeof(UpdateStatePayload), typeDiscriminator: nameof(UpdateStatePayload))]
public class BasePayload
{
}