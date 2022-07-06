using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentValidation;
using FluentValidation.Results;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Netcorext.Contracts;

namespace Netcorext.Extensions.Grpc.Interceptors;

public class ExceptionInterceptor : Interceptor
{
    private readonly ILogger<ExceptionInterceptor> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
                                                                {
                                                                    PropertyNameCaseInsensitive = true,
                                                                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                                                                    WriteIndented = false,
                                                                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                                                                    ReferenceHandler = ReferenceHandler.IgnoreCycles
                                                                };

    public ExceptionInterceptor(ILogger<ExceptionInterceptor> logger)
    {
        _logger = logger;
    }

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request, ServerCallContext context, UnaryServerMethod<TRequest, TResponse> continuation)
    {
        try
        {
            return await continuation(request, context);
        }
        catch (Exception e)
        {
            return GetErrorResponse<TResponse>(e);
        }
    }

    public override TResponse BlockingUnaryCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context, BlockingUnaryCallContinuation<TRequest, TResponse> continuation)
    {
        try
        {
            return continuation(request, context);
        }
        catch (Exception e)
        {
            return GetErrorResponse<TResponse>(e);
        }
    }

    public override async Task<TResponse> ClientStreamingServerHandler<TRequest, TResponse>(IAsyncStreamReader<TRequest> requestStream, ServerCallContext context, ClientStreamingServerMethod<TRequest, TResponse> continuation)
    {
        try
        {
            return await continuation(requestStream, context);
        }
        catch (Exception e)
        {
            return GetErrorResponse<TResponse>(e);
        }
    }

    public override async Task ServerStreamingServerHandler<TRequest, TResponse>(TRequest request, IServerStreamWriter<TResponse> responseStream, ServerCallContext context, ServerStreamingServerMethod<TRequest, TResponse> continuation)
    {
        try
        {
            await continuation(request, responseStream, context);
        }
        catch (Exception e)
        {
            var response = GetErrorResponse<TResponse>(e);

            await responseStream.WriteAsync(response);
        }
    }

    public override async Task DuplexStreamingServerHandler<TRequest, TResponse>(IAsyncStreamReader<TRequest> requestStream, IServerStreamWriter<TResponse> responseStream, ServerCallContext context, DuplexStreamingServerMethod<TRequest, TResponse> continuation)
    {
        try
        {
            await continuation(requestStream, responseStream, context);
        }
        catch (Exception e)
        {
            var response = GetErrorResponse<TResponse>(e);

            await responseStream.WriteAsync(response);
        }
    }
    
    private TResponse GetErrorResponse<TResponse>(Exception ex)
    {
        string? code;
        string? message;
        IEnumerable<ValidationFailure>? errors = null;
        
        var e = GetInnerException(ex);

        switch (e)
        {
            case ValidationException validationEx:
                _logger.LogWarning(e, "{Message}", e);
                
                code = Result.InvalidInput;
                message = validationEx.Message;
                errors = validationEx.Errors;

                break;
            case ArgumentException argumentEx:
                _logger.LogWarning(e, "{Message}", e);
                
                code = Result.InvalidInput;
                message = argumentEx.Message;

                break;
            case BadHttpRequestException badHttpRequestEx:
                _logger.LogWarning(e, "{Message}", e);
                
                code = badHttpRequestEx.Message == "Request body too large."
                           ? Result.PayloadTooLarge
                           : Result.InvalidInput;

                message = badHttpRequestEx.Message;

                break;
            case RpcException rpcException:
                _logger.LogError(e, "{Message}", e);
                
                var result = Result.InternalServerError.Clone();
                
                result.Message = rpcException.Message;

                if (rpcException.StatusCode == StatusCode.Unauthenticated || rpcException.StatusCode == StatusCode.PermissionDenied)
                {
                    try
                    {
                        result = JsonSerializer.Deserialize<Result>(rpcException.Status.Detail, JsonOptions);
                    }
                    catch
                    {
                        // ignored
                    }
                }

                code = result.Code;
                message = result.Message;

                break;
            default:
                _logger.LogError(e, "{Message}", e);
                
                code = Result.InternalServerError;
                message = e.Message;

                break;
        }

        var response = Activator.CreateInstance<TResponse>();
        var type = typeof(TResponse);
        type.GetProperty("Code")?.SetValue(response, code);
        type.GetProperty("Message")?.SetValue(response, message);
        type.GetProperty("Errors")?.SetValue(response, errors);

        return response;
    }

    private static Exception GetInnerException(Exception e)
    {
        var ex = e;

        while (ex.InnerException != null)
        {
            ex = ex.InnerException;
        }

        return ex;
    }
}