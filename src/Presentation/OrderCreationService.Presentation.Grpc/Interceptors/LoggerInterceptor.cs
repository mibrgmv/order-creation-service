using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Logging;
using OrderCreationService.Application.Services.Exceptions;
using OrderCreationService.Infrastructure.Persistence.Exceptions;

namespace OrderCreationService.Presentation.Grpc.Interceptors;

public class LoggerInterceptor : Interceptor
{
    private readonly ILogger<LoggerInterceptor> _logger;

    public LoggerInterceptor(ILogger<LoggerInterceptor> logger)
    {
        _logger = logger;
    }

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        _logger.LogInformation($"Starting receiving call. Type/Method: {MethodType.Unary} / {context.Method}");
        try
        {
            TResponse response = await continuation(request, context);
            _logger.LogInformation($"Finished receiving call. Type/Method: {MethodType.Unary} / {context.Method}");
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error thrown by {context.Method}.");

            throw ex switch
            {
                NotFoundException notFoundException =>
                    new RpcException(new Status(StatusCode.NotFound, notFoundException.Message)),
                ConflictException conflictException =>
                    new RpcException(new Status(StatusCode.AlreadyExists, conflictException.Message)),
                InvalidOperationException invalidOperationException =>
                    new RpcException(new Status(StatusCode.InvalidArgument, invalidOperationException.Message)),
                _ =>
                    new RpcException(new Status(StatusCode.Internal, ex.Message)),
            };
        }
    }

    public override async Task<TResponse> ClientStreamingServerHandler<TRequest, TResponse>(
        IAsyncStreamReader<TRequest> requestStream,
        ServerCallContext context,
        ClientStreamingServerMethod<TRequest, TResponse> continuation)
    {
        _logger.LogInformation($"Starting receiving call. Type/Method: {MethodType.Unary} / {context.Method}");
        try
        {
            TResponse response = await continuation(requestStream, context);
            _logger.LogInformation($"Finished receiving call. Type/Method: {MethodType.Unary} / {context.Method}");
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error thrown by {context.Method}");
            throw;
        }
    }

    public override async Task ServerStreamingServerHandler<TRequest, TResponse>(
        TRequest request,
        IServerStreamWriter<TResponse> responseStream,
        ServerCallContext context,
        ServerStreamingServerMethod<TRequest, TResponse> continuation)
    {
        _logger.LogInformation($"Starting receiving call. Type/Method: {MethodType.Unary} / {context.Method}");
        try
        {
            await continuation(request, responseStream, context);
            _logger.LogInformation($"Finished receiving call. Type/Method: {MethodType.Unary} / {context.Method}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error thrown by {context.Method}");
            throw;
        }
    }

    public override async Task DuplexStreamingServerHandler<TRequest, TResponse>(
        IAsyncStreamReader<TRequest> requestStream,
        IServerStreamWriter<TResponse> responseStream,
        ServerCallContext context,
        DuplexStreamingServerMethod<TRequest, TResponse> continuation)
    {
        _logger.LogInformation($"Starting receiving call. Type/Method: {MethodType.Unary} / {context.Method}");
        try
        {
            await continuation(requestStream, responseStream, context);
            _logger.LogInformation($"Finished receiving call. Type/Method: {MethodType.Unary} / {context.Method}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error thrown by {context.Method}");
            throw;
        }
    }
}