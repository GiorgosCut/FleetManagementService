
using API.Context;
using API.Models;
using Microsoft.EntityFrameworkCore;


namespace API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<FleetContext>(options =>
                options.UseInMemoryDatabase("InMemoryDatabase"));

            builder.Services.AddControllers();
            
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder.WithOrigins("http://localhost:44370")
                                      .AllowAnyMethod()
                                      .AllowAnyHeader());
            });

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseRouting();
            app.MapControllers();
            app.UseHttpsRedirection();
            app.UseCors("CorsPolicy");

            app.Run();
        }
    }
}
