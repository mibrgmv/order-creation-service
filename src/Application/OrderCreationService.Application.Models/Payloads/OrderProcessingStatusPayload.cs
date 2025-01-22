using OrderCreationService.Application.Models.Enums;

namespace OrderCreationService.Application.Models.Payloads;

public class OrderProcessingStatusPayload : BasePayload
{
    public OrderProcessingStatusPayload(OrderProcessingStatus orderProcessingStatus)
    {
        OrderProcessingStatus = orderProcessingStatus;
    }

    public OrderProcessingStatus OrderProcessingStatus { get; set; }
}