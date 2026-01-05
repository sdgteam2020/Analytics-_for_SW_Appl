namespace WebAnalytics.CustomMiddleware
{
    public class BackRestrictionMiddleware
    {
        private readonly RequestDelegate _next;

        public BackRestrictionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var previousUrl = context.Session.GetString("PreviousUrl");
            var currentUrl = context.Request.Path.ToString();

            if (ShouldRestrict(previousUrl, currentUrl))
            {
                context.Response.Redirect("/Account/Restricted");
                return;
            }

            context.Session.SetString("PreviousUrl", currentUrl);

            await _next(context);
        }

        private bool ShouldRestrict(string previousUrl, string currentUrl)
        {
            // Implement your restriction logic here
            // Example: Restrict navigating back to login page
            return !string.IsNullOrEmpty(previousUrl) && previousUrl.Contains("/Account/Login");
        }
    }
}

