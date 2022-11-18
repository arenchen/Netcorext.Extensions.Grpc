using Grpc.Core;

namespace Netcorext.Extensions.Grpc.Options;

public class OriginHeaderPassingInterceptorOptions
{
    public Func<Metadata.Entry, bool> Handler { get; set; } = entry => entry.Key == "Authorization";
}