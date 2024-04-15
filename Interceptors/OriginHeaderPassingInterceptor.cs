using Grpc.Core;
using Grpc.Core.Interceptors;
using Netcorext.Contracts;
using Netcorext.Extensions.Grpc.Helpers;
using Netcorext.Extensions.Grpc.Options;

namespace Netcorext.Extensions.Grpc.Interceptors;

public class OriginHeaderPassingInterceptor : Interceptor
{
    private readonly IContextState _contextState;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly OriginHeaderPassingInterceptorOptions _options;

    public OriginHeaderPassingInterceptor(IContextState contextState, IHttpContextAccessor httpContextAccessor, OriginHeaderPassingInterceptorOptions options)
    {
        _contextState = contextState;
        _httpContextAccessor = httpContextAccessor;
        _options = options;
    }

    public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(TRequest request,
                                                                                  ClientInterceptorContext<TRequest, TResponse> context,
                                                                                  AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
    {
        var entries = _httpContextAccessor.HttpContext?.Request.Headers
                                          .Where(_options.Handler)
                                          .Select(t => new Metadata.Entry(t.Key.ToLower(), t.Value))
                                          .ToArray();

        var authorization = _contextState.GetAuthorizationToken(_httpContextAccessor.HttpContext?.Request.Headers);

        if (entries?.Any() != true)
            return continuation(request, context);

        var metadata = new Metadata();

        foreach (var entry in entries)
        {
            metadata.Add(entry);
        }

        var options = context.Options.WithHeaders(metadata);

        var newContext = new ClientInterceptorContext<TRequest, TResponse>(context.Method, context.Host, options);

        return base.AsyncUnaryCall(request, newContext, continuation);
    }

    public override TResponse BlockingUnaryCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context, BlockingUnaryCallContinuation<TRequest, TResponse> continuation)
    {
        var entries = _httpContextAccessor.HttpContext?.Request.Headers
                                          .Where(_options.Handler)
                                          .Select(t => new Metadata.Entry(t.Key.ToLower(), t.Value))
                                          .ToArray();

        var authorization = _contextState.GetAuthorizationToken(_httpContextAccessor.HttpContext?.Request.Headers);

        if (entries?.Any() != true)
            return continuation(request, context);

        var metadata = new Metadata();

        foreach (var entry in entries)
        {
            metadata.Add(entry);
        }

        var options = context.Options.WithHeaders(metadata);

        var newContext = new ClientInterceptorContext<TRequest, TResponse>(context.Method, context.Host, options);

        return base.BlockingUnaryCall(request, newContext, continuation);
    }
}
