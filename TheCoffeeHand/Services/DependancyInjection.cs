using Microsoft.Extensions.DependencyInjection;
using Services.ServiceInterfaces;
using Services.Services;
using Services.Services.RedisCache;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Services {
    public static class DependencyInjection
    {
        public static void AddServiceLayer(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IFirebaseAuthService, FirebaseAuthService>();
            services.AddScoped<IUserServices, UserServices>();
            services.AddScoped<ICategoryService, CategoryServices>();
            services.AddScoped<IRedisCacheServices, RedisCacheServices>();
            services.AddScoped<IIngredientService, IngredientService>();
            services.AddScoped<IDrinkService, DrinkService>();
            services.AddScoped<IRecipeService, RecipeService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IOrderDetailService, OrderDetailService>();
            //services.AddScoped<IFCMService, FCMService>();
            services.AddHttpClient<IFCMService, FCMService>();
            services.AddScoped<IImageService, ImageService>();

            //// Add Message Queue Service
            //services.Configure<MessageBrokerSetting>(builer =>
            //    configuration.GetSection("MessageBroker"));

            //services.AddSingleton(provider =>
            //    provider.GetRequiredService<IOptions<MessageBrokerSetting>>().Value);

            //services.AddMassTransit(x => {
            //    x.SetKebabCaseEndpointNameFormatter();

            //    x.UsingRabbitMq((context, cfg) => {
            //        MessageBrokerSetting settings = context.GetRequiredService<MessageBrokerSetting>();
            //        if (string.IsNullOrEmpty(settings.Host)) {
            //            throw new ArgumentNullException(nameof(settings.Host), "Host cannot be null or empty.");
            //        }
            //        cfg.Host(new Uri(settings.Host), h => {
            //            h.Username(settings.UserName);
            //            h.Password(settings.Password);
            //        });

            //        cfg.ConfigureEndpoints(context);
            //    });
            //});

            var rabbitConfig = configuration.GetSection("RabbitMQ");
            var factory = new ConnectionFactory()
            {
                Uri = new Uri(rabbitConfig["ConnectionString"])
            };

            services.AddSingleton(factory);

            services.AddSingleton<IRabbitMQService, RabbitMQService>();
            services.AddHostedService<RabbitMQConsumerService>();
            services.AddScoped<IMachineInfoService, MachineInfoService>();

        }
    }
}
