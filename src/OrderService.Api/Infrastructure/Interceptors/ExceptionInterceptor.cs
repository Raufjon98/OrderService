using Grpc.Core;
using Grpc.Core.Interceptors;
using OrderService.Api.Features.Common.Exceptions;

namespace OrderService.Api.Infrastructure.Interceptors;

public class ExceptionInterceptor : Interceptor
{
    private readonly ILogger<ExceptionInterceptor> _logger;

    public ExceptionInterceptor(ILogger<ExceptionInterceptor> logger)
    {
        _logger = logger;
    }

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request, ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        try
        {
            return await continuation(request, context);
        }
        catch (NotFoundException e)
        {
            throw new RpcException(new Status(StatusCode.NotFound, e.Message));
        }
        catch (ExistsException e)
        {
            throw new RpcException(new Status(StatusCode.AlreadyExists, e.Message));
        }
        catch (ValidationException ex)
        {
            var metadata = new Metadata();

            foreach (var error in ex.Errors)
            {
                foreach (var message in error.Value)
                {
                    metadata.Add("validation-error", message);
                }
            }

            throw new RpcException(
                new Status(StatusCode.InvalidArgument, ex.Message),
                metadata);
        }
        catch (RpcException)
        {
            throw; 
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            throw new RpcException(new Status(StatusCode.Internal, e.Message));
        }
    }
}