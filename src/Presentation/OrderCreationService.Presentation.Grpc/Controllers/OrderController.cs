using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using OrderCreationService.Application.Contracts.Orders;
using OrderCreationService.Application.Contracts.Orders.Requests;
using OrderCreationService.Application.Models.Models;
using OrderCreationService.Presentation.Grpc.Extensions;
using Orders.CreationService.Contracts;
using AddOrderDto = OrderCreationService.Application.Contracts.Requests.AddOrderDto;
using AddProductToOrderDto = OrderCreationService.Application.Contracts.Requests.AddProductToOrderDto;

namespace OrderCreationService.Presentation.Grpc.Controllers;

public class OrderController : OrderService.OrderServiceBase
{
    private readonly IOrderService _orderService;

    public OrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    public override async Task<AddOrdersResponse> AddOrders(
        AddOrdersRequest request,
        ServerCallContext context)
    {
        var orders = new List<AddOrderDto>();

        orders.AddRange(request.Orders.Select(x =>
            new AddOrderDto(x.OrderCreatedBy)));

        long[] ids = await _orderService.AddOrdersAsync(orders, context.CancellationToken);
        return new AddOrdersResponse { OrdersIds = { ids } };
    }

    public override async Task<AddProductToOrderResponse> AddProductsToOrder(
        AddProductsToOrderRequest request,
        ServerCallContext context)
    {
        var products = new List<AddProductToOrderDto>();

        products.AddRange(request.Products.Select(x =>
            new AddProductToOrderDto(x.ProductId, x.Quantity)));

        await _orderService.AddProductsToOrderAsync(request.OrderId, products, context.CancellationToken);
        return new AddProductToOrderResponse();
    }

    public override async Task<RemoveProductsFromOrderResponse> RemoveProductsFromOrder(
        RemoveProductsFromOrderRequest request,
        ServerCallContext context)
    {
        await _orderService.RemoveProductsFromOrderAsync(request.OrderId, request.ProductIds.ToArray(), context.CancellationToken);
        return new RemoveProductsFromOrderResponse();
    }

    public override async Task<StartOrderProcessingResponse> StartOrderProcessing(
        StartOrderProcessingRequest request,
        ServerCallContext context)
    {
        await _orderService.StartOrderProcessingAsync(request.OrderId, context.CancellationToken);
        return new StartOrderProcessingResponse();
    }

    public override async Task<CancelOrderResponse> CancelOrder(
        CancelOrderRequest request,
        ServerCallContext context)
    {
        await _orderService.CancelOrderAsync(request.OrderId, context.CancellationToken);
        return new CancelOrderResponse();
    }

    public override async Task QueryOrders(
        OrderQuery request,
        IServerStreamWriter<OrderDto> responseStream,
        ServerCallContext context)
    {
        var applicationRequest = new QueryOrders.Request(
            request.Ids.ToArray(),
            (Application.Models.Enums.OrderState?)request.OrderState,
            request.CreatedBy,
            request.Cursor,
            request.PageSize);

        await foreach (Order order in _orderService.QueryOrdersAsync(applicationRequest, context.CancellationToken))
        {
            await responseStream.WriteAsync(
                new OrderDto
                {
                OrderState = (OrderState)order.OrderState,
                OrderCreatedAt = order.OrderCreatedAt.ToTimestamp(),
                OrderCreatedBy = order.OrderCreatedBy,
                },
                context.CancellationToken);
        }
    }

    public override async Task QueryItems(
        OrderItemQuery request,
        IServerStreamWriter<OrderItemDto> responseStream,
        ServerCallContext context)
    {
        var applicationRequest = new QueryOrderItems.Request(
            request.Ids.ToArray(),
            request.OrderIds.ToArray(),
            request.ProductIds.ToArray(),
            request.Deleted,
            request.Cursor,
            request.PageSize);

        await foreach (OrderItem item in _orderService.QueryOrderItemsAsync(applicationRequest, context.CancellationToken))
        {
            await responseStream.WriteAsync(
                new OrderItemDto
                {
                    OrderId = item.OrderId,
                    OrderItemQuantity = item.OrderItemQuantity,
                    ProductId = item.ProductId,
                },
                context.CancellationToken);
        }
    }

    public override async Task QueryHistory(OrderHistoryQuery request, IServerStreamWriter<OrderHistoryItemDto> responseStream, ServerCallContext context)
    {
        var applicationRequest = new QueryOrderHistory.Request(
            request.OrderIds.ToArray(),
            request.OrderIds.ToArray(),
            request.Kind == OrderHistoryItemKind.Unspecified ? null : (Application.Models.Enums.OrderHistoryItemKind?)request.Kind,
            request.Cursor,
            request.PageSize);

        await foreach (OrderHistoryItem item in _orderService.QueryOrderHistoryAsync(applicationRequest, context.CancellationToken))
        {
            await responseStream.WriteAsync(
                new OrderHistoryItemDto
                {
                    OrderId = item.OrderId,
                    OrderHistoryItemCreatedAt = item.OrderHistoryItemCreatedAt.ToTimestamp(),
                    OrderHistoryItemKind = (OrderHistoryItemKind)item.OrderHistoryItemKind,
                    Payload = PayloadExtensions.SerializePayload(item.Payload),
                },
                context.CancellationToken);
        }
    }
}