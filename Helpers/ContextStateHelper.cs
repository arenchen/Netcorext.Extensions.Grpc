using System.Security.Claims;
using Netcorext.Contracts;

namespace Netcorext.Extensions.Grpc.Helpers;

public static class ContextStateHelper
{
    public static string? GetAuthorizationToken(this IContextState context, IHeaderDictionary? headers)
    {
        if (headers?.TryGetValue("Authorization", out var authorization) == true && !string.IsNullOrWhiteSpace(authorization))
            return authorization;
        if (context.User?.Identity == null)
            return null;

        var token = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.UserData)?.Value;

        if (string.IsNullOrWhiteSpace(token))
            return null;

        return context.User.Identity.AuthenticationType == "AuthenticationTypes.Basic"
                   ? $"Basic {token}"
                   : $"Bearer {token}";
    }
}
