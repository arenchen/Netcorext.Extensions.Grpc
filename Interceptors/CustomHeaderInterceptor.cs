using Grpc.Core;
using Grpc.Core.Interceptors;

namespace Netcorext.Extensions.Grpc.Interceptors;

public class CustomHeaderInterceptor : Interceptor
{
    private readonly IDictionary<string, string> _headers;

    public CustomHeaderInterceptor(IDictionary<string, string> headers)
    {
        _headers = headers;
    }

    public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(TRequest request,
                                                                                  ClientInterceptorContext<TRequest, TResponse> context,
                                                                                  AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
    {
        var metadata = new Metadata();

        foreach (var header in _headers)
        {
            metadata.Add(new Metadata.Entry(header.Key, header.Value));
        }

        var options = context.Options.WithHeaders(metadata);
        var newContext = new ClientInterceptorContext<TRequest, TResponse>(context.Method, context.Host, options);

        return base.AsyncUnaryCall(request, newContext, continuation);
    }

    public override TResponse BlockingUnaryCall<TRequest, TResponse>(TRequest request,
                                                                     ClientInterceptorContext<TRequest, TResponse> context,
                                                                     BlockingUnaryCallContinuation<TRequest, TResponse> continuation)
    {
        var metadata = new Metadata();

        foreach (var header in _headers)
        {
            metadata.Add(new Metadata.Entry(header.Key, header.Value));
        }

        var options = context.Options.WithHeaders(metadata);
        var newContext = new ClientInterceptorContext<TRequest, TResponse>(context.Method, context.Host, options);

        return base.BlockingUnaryCall(request, newContext, continuation);
    }
}