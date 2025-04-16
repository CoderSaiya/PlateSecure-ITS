namespace PlateSecure.Security.Middlewares;

public class CheckRoleMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value!.ToLower()!;

        if (Endpoints.Public.Any(e => EndpointMatcher.IsMatch(e, path)))
        {
            await next(context);
            return;
        }

        if (context.User.Identity is not { IsAuthenticated: true })
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("You are not authenticated.");
            return;
        }

        using (var scope = context.RequestServices.CreateScope())
        {
            var expClaim = context.User.FindFirst("exp");
            if (expClaim is not null && long.TryParse(expClaim.Value, out var expSeconds))
            {
                var tokenExpiryDate = DateTimeOffset.FromUnixTimeSeconds(expSeconds).UtcDateTime;
                if (tokenExpiryDate < DateTimeOffset.UtcNow)
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync("Unauthorized - Token has expired.");
                }
            }
            
            bool isStaffEndpoint = Endpoints.Staff.Any(e => EndpointMatcher.IsMatch(e, path));
            bool isAdminEndpoint = Endpoints.Admin.Any(e => EndpointMatcher.IsMatch(e, path));

            bool hasAccess = (isStaffEndpoint && context.User.IsInRole("Staff")) || 
                             (isAdminEndpoint && context.User.IsInRole("Admin"));

            if (!hasAccess)
            {
                context.Response.StatusCode = 403;
                await context.Response.WriteAsync("Forbidden - Insufficient role permissions");
                return;
            }
        }
        
        await next(context);
    }
}