namespace WebAnalytics.CustomMiddleware
{
    public static class AuthorizationPolicies
    {
        public static void AddAuthorizationPolicies(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy("Endorsing",
                    policy => policy.RequireClaim("Endorsing"));

                options.AddPolicy("Initiator",
                   policy => policy.RequireClaim("Initiator")); 
                options.AddPolicy("ApprovingAuth",
                   policy => policy.RequireClaim("ApprovingAuth"));
                // Initiator OR ApprovingAuth
                options.AddPolicy("InitiatorOrApprover", policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(c =>
                            c.Type == "Initiator" || c.Type == "ApprovingAuth")
                    ));
            });
        }

    }
}
