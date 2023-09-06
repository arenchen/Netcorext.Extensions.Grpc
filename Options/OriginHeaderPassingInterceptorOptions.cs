using Grpc.Core;
using Microsoft.Extensions.Primitives;

namespace Netcorext.Extensions.Grpc.Options;

public class OriginHeaderPassingInterceptorOptions
{
    public Func<KeyValuePair<string, StringValues>, bool> Handler { get; set; } = entry => entry.Key == "Authorization";
}
