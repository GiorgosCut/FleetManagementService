using FleetManagementService.Components;
using Microsoft.AspNetCore.Mvc;

namespace FleetManagementService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            builder.Services.AddHttpClient();
            builder.Services.AddAntiforgery(options =>
            {     
                options.Cookie.Expiration = TimeSpan.Zero;

            });
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");

                app.UseHsts();
            }

            app.UseHttpsRedirection();


            app.UseAntiforgery();
            app.UseStaticFiles();


            app.MapRazorComponents<App>().DisableAntiforgery().AddInteractiveServerRenderMode();

            app.Run();
        }
    }
}
