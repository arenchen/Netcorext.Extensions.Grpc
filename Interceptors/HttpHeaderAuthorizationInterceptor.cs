using Grpc.Core;
using Grpc.Core.Interceptors;

namespace Netcorext.Extensions.Grpc.Interceptors;

public class HttpHeaderAuthorizationInterceptor : Interceptor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpHeaderAuthorizationInterceptor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(TRequest request,
                                                                                  ClientInterceptorContext<TRequest, TResponse> context,
                                                                                  AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
    {
        var authorization = _httpContextAccessor.HttpContext?.Request.Headers.Authorization;
        var metadata = new Metadata { { "Authorization", $"{authorization}" } };
        context.Options.WithHeaders(metadata);

        return continuation(request, context);
    }
}