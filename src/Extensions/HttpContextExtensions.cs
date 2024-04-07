using System.Security.Claims;

namespace ChatService.Extensions;

internal static class HttpContextExtensions {
    public static bool TryGetUserId(this HttpContext context, out string userId) {
        var idClaim = context.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);

        if (idClaim is null) {
            userId = string.Empty;
            return false;
        }

        userId = idClaim.Value;
        return true;
    }
}