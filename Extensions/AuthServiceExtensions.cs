using Microsoft.AspNetCore.Authentication.Cookies;


namespace HotelReservation.Extensions
{
    public static class AuthServiceExtensions
    {
        public static IServiceCollection AddAuthServices(this IServiceCollection services)
        {
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Auth/Login";
                    options.AccessDeniedPath = "/Auth/AccessDenied";
                    options.Cookie.HttpOnly = true; // Ensures cookies are secure
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Ensures cookies are always sent over HTTPS
                    options.Cookie.SameSite = SameSiteMode.Strict; // Prevents CSRF attacks
                    options.ExpireTimeSpan = TimeSpan.FromHours(2); // Ensures session lasts for 2 hours
                    options.SlidingExpiration = true; // Refreshes the session on activity
                    options.Cookie.IsEssential = true;  // Ensure cookie is not blocked  

                    // Redirect unauthenticated users to login with a warning message
                    options.Events = new CookieAuthenticationEvents
                    {
                        OnRedirectToLogin = context =>
                        {
                            if (!context.Request.Path.StartsWithSegments("/Auth/Login"))
                            {
                                var returnUrl = context.Request.Path + context.Request.QueryString;
                                context.Response.Redirect($"/Auth/Login?warning=You must be logged in to access this page.&returnUrl={returnUrl}");
                            }
                            return Task.CompletedTask;
                        }
                    };

                });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
                options.AddPolicy("HousekeepingOnly", policy => policy.RequireRole("Housekeeping"));
                options.AddPolicy("GuestOnly", policy => policy.RequireRole("Guest"));
                options.AddPolicy("FrontDeskOnly", policy => policy.RequireRole("FrontDesk"));
            });

            return services;
        }
    }
}
