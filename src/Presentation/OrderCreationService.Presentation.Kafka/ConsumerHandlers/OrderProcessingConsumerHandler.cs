using Kafka.Abstractions.Consumer;
using Kafka.Abstractions.Consumer.Models;
using Microsoft.Extensions.Logging;
using OrderCreationService.Application.Contracts.Orders;
using OrderCreationService.Application.Models.Enums;
using Orders.Kafka.Contracts;

namespace OrderCreationService.Presentation.Kafka.ConsumerHandlers;

public class OrderProcessingConsumerHandler : IKafkaConsumerHandler<OrderProcessingKey, OrderProcessingValue>
{
    private readonly IOrderService _orderService;
    private readonly ILogger<OrderProcessingConsumerHandler> _logger;

    public OrderProcessingConsumerHandler(
        IOrderService orderService,
        ILogger<OrderProcessingConsumerHandler> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    public async ValueTask HandleAsync(KafkaConsumerMessage<OrderProcessingKey, OrderProcessingValue> message, CancellationToken cancellationToken)
    {
        if (message.Value.EventCase is OrderProcessingValue.EventOneofCase.ApprovalReceived)
        {
            await _orderService.UpdateOrderProcessingStatusAsync(
                message.Value.ApprovalReceived.OrderId,
                OrderProcessingStatus.ApprovalReceived,
                cancellationToken);
        }
        else if (message.Value.EventCase is OrderProcessingValue.EventOneofCase.PackingStarted)
        {
            await _orderService.UpdateOrderProcessingStatusAsync(
                message.Value.PackingStarted.OrderId,
                OrderProcessingStatus.PackingStarted,
                cancellationToken);
        }
        else if (message.Value.EventCase is OrderProcessingValue.EventOneofCase.PackingFinished)
        {
            if (message.Value.PackingFinished.IsFinishedSuccessfully is false)
            {
                await _orderService.CancelOrderAsync(message.Value.PackingFinished.OrderId, cancellationToken);
                return;
            }

            await _orderService.UpdateOrderProcessingStatusAsync(
                message.Value.PackingFinished.OrderId,
                OrderProcessingStatus.PackingFinished,
                cancellationToken);
        }
        else if (message.Value.EventCase is OrderProcessingValue.EventOneofCase.DeliveryStarted)
        {
            await _orderService.UpdateOrderProcessingStatusAsync(
                message.Value.DeliveryStarted.OrderId,
                OrderProcessingStatus.DeliveryStarted,
                cancellationToken);
        }
        else if (message.Value.EventCase is OrderProcessingValue.EventOneofCase.DeliveryFinished)
        {
            if (message.Value.DeliveryFinished.IsFinishedSuccessfully is false)
            {
                await _orderService.CancelOrderAsync(message.Value.DeliveryFinished.OrderId, cancellationToken);
                return;
            }

            await _orderService.UpdateOrderProcessingStatusAsync(
                message.Value.DeliveryFinished.OrderId,
                OrderProcessingStatus.DeliveryFinished,
                cancellationToken);

            await _orderService.CompleteOrderAsync(message.Value.DeliveryFinished.OrderId, cancellationToken);
        }
        else
        {
            _logger.LogError($"Invalid event case received = {message.Value.EventCase} for order = {message.Key.OrderId}");
        }
    }
}