using DataAccessLayerEF;
using Microsoft.EntityFrameworkCore;

namespace WebAnalytics.CustomMiddleware
{
    public class OriginValidationMiddleware
    {
        private readonly RequestDelegate _next;

        public OriginValidationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ApplicationDbContext db)
        {
            var apiKey = context.Request.Headers["X-API-KEY"].FirstOrDefault();
            var origin = context.Request.Headers["Referer"].FirstOrDefault();
            // Apply only to API Controllers
            if (!context.Request.Path.StartsWithSegments("/api"))
            {
                await _next(context);
                return;
            }

           // var origin = context.Request.Headers["Origin"].FirstOrDefault()
            //          ?? context.Request.Headers["Referer"].FirstOrDefault();

         

           
            if (string.IsNullOrEmpty(origin) || string.IsNullOrEmpty(apiKey))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new
                {
                    message = "Missing Origin or API Key"
                });
                return;
            }

            // Normalize origin
            origin = new Uri(origin).GetLeftPart(UriPartial.Authority);
           
            var isOriginAllowed = await db.trnapplicationDetails.AnyAsync(x => x.origin == origin && x.ApplicationKey==apiKey && x.IsActive);

            

            if (!isOriginAllowed)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(new
                {
                    message = "Unauthorized Origin or Client"
                });
                return;
            }

            // Optional: Store validated info
            context.Items["Origin"] = origin;
            context.Items["ClientKey"] = apiKey;

            await _next(context);
        }
    }

}
