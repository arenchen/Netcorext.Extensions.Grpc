using Grpc.Core;
using Grpc.Core.Interceptors;

namespace Netcorext.Extensions.Grpc.Interceptors;

public class AuthorizationHeaderInterceptor : Interceptor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthorizationHeaderInterceptor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(TRequest request,
                                                                                  ClientInterceptorContext<TRequest, TResponse> context,
                                                                                  AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
    {
        var authorization = _httpContextAccessor.HttpContext?.Request.Headers.Authorization;

        if (string.IsNullOrWhiteSpace(authorization))
            return continuation(request, context);
        
        var metadata = new Metadata { { "Authorization", $"{authorization}" } };
        var options = context.Options.WithHeaders(metadata);
        var newContext = new ClientInterceptorContext<TRequest, TResponse>(context.Method, context.Host, options);
 
        return base.AsyncUnaryCall(request, newContext, continuation);
    }
    
    public override TResponse BlockingUnaryCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context, BlockingUnaryCallContinuation<TRequest, TResponse> continuation)
    {
        var authorization = _httpContextAccessor.HttpContext?.Request.Headers.Authorization;

        if (string.IsNullOrWhiteSpace(authorization))
            return continuation(request, context);
        
        var metadata = new Metadata { { "Authorization", $"{authorization}" } };
        var options = context.Options.WithHeaders(metadata);
        var newContext = new ClientInterceptorContext<TRequest, TResponse>(context.Method, context.Host, options);
 
        return base.BlockingUnaryCall(request, newContext, continuation);
    }
}