using Repositories.Seeds;
using TheCoffeeHand.MiddleWares;

namespace TheCoffeeHand {
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    public class Program {
        /// <summary>
        /// The main method which configures and runs the web application.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        public static async Task Main(string[] args) {
            var builder = WebApplication.CreateBuilder(args);

            // Register all services using DependencyInjection
            builder.Services.AddApplication(builder.Configuration);

            // Thêm cấu hình CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigins", policy =>
                {
                    policy.WithOrigins(
                            "http://localhost:3000",
                            "https://the-coffee-hand-fe.vercel.app"
                        )
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });


            builder.Services.AddHttpContextAccessor();

            var app = builder.Build();

            // Seed data
            using (var scope = app.Services.CreateScope()) {
                var services = scope.ServiceProvider;
                await Seed.Initialize(services);
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment() || app.Environment.IsStaging() || app.Environment.IsProduction())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "The Coffee Hand API v1");
                });
            }


            app.UseHttpsRedirection();

            app.UseCors("AllowSpecificOrigins");



            app.UseMiddleware<ExceptionMiddleware>();

            // Enable authentication and authorization
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
