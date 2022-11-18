using Grpc.Net.ClientFactory;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Netcorext.Extensions.Grpc.Interceptors;
using Netcorext.Extensions.Grpc.Options;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtension
{
    public static IHttpClientBuilder AddInsecureGrpcClient<TClient>(this IServiceCollection services, Action<IServiceProvider, OriginHeaderPassingInterceptorOptions>? configure = null)
        where TClient : class
    {
        services.TryAddSingleton(provider =>
                              {
                                  var options = new OriginHeaderPassingInterceptorOptions();
                                  
                                  configure?.Invoke(provider, options);

                                  return options;
                              });

        return services.AddGrpcClient<TClient>()
                       .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
                       {
                           ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                       })
                       .AddInterceptor<OriginHeaderPassingInterceptor>();
    }

    public static IHttpClientBuilder AddInsecureGrpcClient<TClient>(this IServiceCollection services, Action<GrpcClientFactoryOptions> configureClient, Action<IServiceProvider, OriginHeaderPassingInterceptorOptions>? configure = null)
        where TClient : class
    {
        services.TryAddSingleton(provider =>
                                 {
                                     var options = new OriginHeaderPassingInterceptorOptions();
                                  
                                     configure?.Invoke(provider, options);

                                     return options;
                                 });
        
        return services.AddGrpcClient<TClient>(configureClient)
                       .AddInterceptor<OriginHeaderPassingInterceptor>();
    }

    public static IHttpClientBuilder AddInsecureGrpcClient<TClient>(this IServiceCollection services, Action<IServiceProvider, GrpcClientFactoryOptions> configureClient, Action<IServiceProvider, OriginHeaderPassingInterceptorOptions>? configure = null)
        where TClient : class
    {
        services.TryAddSingleton(provider =>
                                 {
                                     var options = new OriginHeaderPassingInterceptorOptions();
                                  
                                     configure?.Invoke(provider, options);

                                     return options;
                                 });
        
        return services.AddGrpcClient<TClient>(configureClient)
                       .AddInterceptor<OriginHeaderPassingInterceptor>();
    }

    public static IHttpClientBuilder AddInsecureGrpcClient<TClient>(this IServiceCollection services, string name, Action<IServiceProvider, OriginHeaderPassingInterceptorOptions>? configure = null)
        where TClient : class
    {
        services.TryAddSingleton(provider =>
                                 {
                                     var options = new OriginHeaderPassingInterceptorOptions();
                                  
                                     configure?.Invoke(provider, options);

                                     return options;
                                 });
        
        return services.AddGrpcClient<TClient>(name)
                       .AddInterceptor<OriginHeaderPassingInterceptor>();
    }

    public static IHttpClientBuilder AddInsecureGrpcClient<TClient>(this IServiceCollection services, string name, Action<GrpcClientFactoryOptions> configureClient, Action<IServiceProvider, OriginHeaderPassingInterceptorOptions>? configure = null)
        where TClient : class
    {
        services.TryAddSingleton(provider =>
                                 {
                                     var options = new OriginHeaderPassingInterceptorOptions();
                                  
                                     configure?.Invoke(provider, options);

                                     return options;
                                 });
        
        return services.AddGrpcClient<TClient>(name, configureClient)
                       .AddInterceptor<OriginHeaderPassingInterceptor>();
    }
}