namespace OrderCreationService.Application.Models.Enums;

public enum OrderProcessingStatus
{
    Unspecified = 0,
    ApprovalReceived = 1,
    PackingStarted = 2,
    PackingFinished = 3,
    DeliveryStarted = 4,
    DeliveryFinished = 5,
}