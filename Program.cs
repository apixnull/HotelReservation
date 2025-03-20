using HotelReservation.Data;
using HotelReservation.Extensions;
using HotelReservation.Services;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

namespace HotelReservation
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add logging
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();
          

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // Add database context
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
            );

            // turn this off for smoother run
            //builder.Services.AddHttpsRedirection(options =>
            //{
            //    options.HttpsPort = 7033; // 🔹 Make sure this matches your launchSettings.json
            //});

            // Configure Cookie Policy (Security Fix)
            builder.Services.Configure<CookiePolicyOptions>(options =>
            {
                options.MinimumSameSitePolicy = SameSiteMode.Strict;
                options.HttpOnly = HttpOnlyPolicy.Always;
                options.Secure = CookieSecurePolicy.Always;
            });

            // Add services
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddHttpClient<VerimailService>();
            builder.Services.AddScoped<VerimailService>();
            builder.Services.AddScoped<AuthService>();
            builder.Services.AddScoped<UserRegistrationService>();
            builder.Services.AddScoped<EmailService>();
            builder.Services.AddScoped<ManageUserService>();
            builder.Services.AddAuthServices();
            builder.Services.AddScoped<ManageRoomService>();
            builder.Services.AddScoped<BookingService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseStaticFiles(); // Required to serve files from wwwroot
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads")),
                RequestPath = "/uploads"
            });


            // Apply Cookie Policy Fix
            app.UseCookiePolicy();

            app.UseRouting();
            // Authentication & Authorization
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "areas",
                pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
            );

            // Define Routes
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
