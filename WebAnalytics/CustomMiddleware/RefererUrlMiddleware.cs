
using System.Reflection;

namespace WebAnalytics.CustomMiddleware
{
    public class RefererUrlMiddleware
    {
        private readonly RequestDelegate _next;

        public RefererUrlMiddleware(RequestDelegate next)
        {
            _next = next;

        }

        public async Task Invoke(HttpContext context)
        {
            try
            {


                // Do something with context near the beginning of request processing.
                var myHeader = context.Request.Path.ToString();
                string referer = context.Request.Headers["Referer"].ToString();
                if (referer != "" && myHeader != "/")
                {
                    await _next.Invoke(context);
                }

                else if (myHeader == "/" || myHeader.Contains("Login") || myHeader.Contains("Logout") || myHeader.Contains("Default") || myHeader.Contains("Account"))
                {
                    await _next.Invoke(context);
                }
                else if (referer=="")
                {


                    context.Response.Redirect("/Account/Error");

                }
            }
            catch (Exception ex) { }

        }
    }

    public static class MyMiddlewareExtensions
    {
        public static IApplicationBuilder UseUrlRefreshMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RefererUrlMiddleware>();
        }
    }
}
