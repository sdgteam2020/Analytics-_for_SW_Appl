using DataAccessLayerEF;
using Domain.Identitytable;
using Domain.interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Newtonsoft.Json.Serialization;
using WebAnalytics.CustomMiddleware;
using WebAnalytics.Helpers;
var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// ----------------------- Services -----------------------
builder.Services.AddDbContextPool<ApplicationDbContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("DBConnection")));

// Use Identity’s cookie (avoid defining your own separate "Cookies" scheme)
builder.Services
    .AddIdentity<ApplicationUser, ApplicationRole>(opts =>
    {
        opts.Password.RequireNonAlphanumeric = true;
        opts.Password.RequireUppercase = true;
        opts.Password.RequireDigit = true;
        opts.Password.RequiredLength = 8;
        opts.Password.RequiredUniqueChars = 1;
        opts.User.RequireUniqueEmail = false;

        // Lockout
        opts.Lockout.AllowedForNewUsers = true;
        opts.Lockout.MaxFailedAccessAttempts = 3;
        opts.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();
builder.Services.AddRepository();
// Validate security stamp on EVERY request (forces revalidation)
builder.Services.Configure<SecurityStampValidatorOptions>(o =>
{
    o.ValidationInterval = TimeSpan.Zero;
});
// Antiforgery cookie hardened (__Host- prefix requires Secure + path "/" + no Domain)
builder.Services.AddAntiforgery(o => o.SuppressXFrameOptionsHeader = true);
builder.Services.AddAntiforgery(o =>
{
    o.Cookie.Name = "__Host-AntiForgery";

    //  o.Cookie.HttpOnly = true;
    // o.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    o.Cookie.Path = "/"; // required for __Host- cookies
    // o.Cookie.SameSite = SameSiteMode.Strict; // enable if you don't post from cross-site contexts
});// Identity application cookie hardening
builder.Services.ConfigureApplicationCookie(o =>
{
    o.LoginPath = "/Account/Login";
    o.AccessDeniedPath = "/Account/AccessDenied";
    o.SlidingExpiration = true;
    o.ExpireTimeSpan = TimeSpan.FromMinutes(30);

    o.Cookie.HttpOnly = true;
    o.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    o.Cookie.SameSite = SameSiteMode.Strict; // use Lax/None if you have external IdP redirects
    // Ensure immediate revalidation
    o.Events.OnValidatePrincipal = SecurityStampValidator.ValidatePrincipalAsync;
});
// Session cookie hardened
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(20);
    options.Cookie.Name = ".WebAnalytics.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    //options.Cookie.SameSite = SameSiteMode.Strict;
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", cors =>
        cors
             .SetIsOriginAllowed(origin =>
             {
                 using var scope = builder.Services.BuildServiceProvider().CreateScope();
                 var svc = scope.ServiceProvider.GetRequiredService<IAllowedOriginService>();
                 return svc.IsAllowed(origin);
             }) // allow all dynamically
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});


// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "OrderStore", Version = "v1" });
});
builder.Services.AddSingleton<ITagHelperInitializer<ScriptTagHelper>, AppendVersionTagHelperInitializer>();
builder.Services.AddSingleton<ITagHelperInitializer<LinkTagHelper>, AppendVersionTagHelperInitializer>();
builder.Services.AddControllers().AddNewtonsoftJson(options =>
{
    options.SerializerSettings.ContractResolver = new DefaultContractResolver();
});
// knows the original request was HTTPS (needed for Cookie Secure + HSTS correctness)
builder.Services.Configure<ForwardedHeadersOptions>(opts =>
{
    opts.ForwardedHeaders = ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedFor;
    // opts.KnownProxies.Add(IPAddress.Parse("YOUR_PROXY_IP"));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.Use(async (ctx, next) =>
{
    // 1) Content Security Policy
    ctx.Response.Headers["Content-Security-Policy"] =
        //"default-src 'self'; " +
        "script-src 'self'; " +
        "style-src 'self'; " + // allow Bootstrap inline styles
        "img-src 'self' data:; " +
        "font-src 'self' data:; " +
        //"connect-src 'self'; " +
        "frame-ancestors 'none'; " +
        "base-uri 'self'; " +
        "form-action 'self';";

    // 2) X-Frame-Options (align with frame-ancestors)
    ctx.Response.Headers["X-Frame-Options"] = "DENY";

    // 3) Referrer-Policy
    ctx.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

    // Extra good headers
    ctx.Response.Headers["X-Content-Type-Options"] = "nosniff";
    ctx.Response.Headers["X-XSS-Protection"] = "1; mode=block";

    // Use HSTS only on HTTPS + production
    ctx.Response.Headers["Strict-Transport-Security"] =
        "max-age=31536000; includeSubDomains; preload";

    // Hide tech details where possible
    ctx.Response.Headers.Remove("X-Powered-By");
    ctx.Response.Headers.Remove("x-aspnet-version");

    await next();
});
app.UseHttpsRedirection(); 
app.UseCookiePolicy(new CookiePolicyOptions
{
    Secure = CookieSecurePolicy.Always,
    MinimumSameSitePolicy = SameSiteMode.Strict
});
//app.Use(async (context, next) =>
//{
//    context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
//    await next();
//});



app.UseRouting();
app.UseCors("CorsPolicy");
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
//app.UsePathBase("/HitCounter");
//app.Use((ctx, next) => { ctx.Request.PathBase = "/HitCounter"; return next(); }); // if needed
// AuthN then AuthZ
app.UseAuthentication();
app.UseAuthorization();
app.UseHttpsRedirection();
// Session BEFORE endpoints (so MVC/Razor can use it)
app.UseSession();

// Custom middlewares (after auth so you can access User, etc.)
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseMiddleware<XssProtectionMiddleware>();
app.UseMiddleware<OriginValidationMiddleware>();
//app.UseUrlRefreshMiddleware();

app.MapStaticAssets();
//https://localhost:7144/swagger/index.html
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
