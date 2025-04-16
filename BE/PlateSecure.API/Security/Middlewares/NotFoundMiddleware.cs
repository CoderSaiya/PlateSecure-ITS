namespace PlateSecure.Security.Middlewares;

public class NotFoundMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLower();
        if (path is null)
        {
            context.Response.StatusCode = 404;
            await context.Response.WriteAsync("404 Request path is null");
            return;
        }
        
        bool isExist = Endpoints.Public.Any(e => path.StartsWith(e)) ||
                       Endpoints.Staff.Any(e => path.StartsWith(e)) ||
                       Endpoints.Admin.Any(e => path.StartsWith(e));
        
        if (!isExist)
        {
            context.Response.StatusCode = 404;
            await context.Response.WriteAsync("404 Not Found");
            return;
        }
        
        await next(context);
    }
}