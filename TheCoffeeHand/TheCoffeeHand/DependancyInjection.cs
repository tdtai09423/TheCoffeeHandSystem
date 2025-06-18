using CloudinaryDotNet;
using Domain.Base;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Interfracture.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Repositories;
using Repositories.Base;
using Services;
using Services.Config;
using StackExchange.Redis;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using Interfracture.MessageBroker;
using Infrastructure.Base;
using MongoDB.Driver;
using Interfracture.Interfaces;
using Repositories.Repositories;

namespace TheCoffeeHand
{
    /// <summary>
    /// Provides dependency injection setup for the application.
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// Configures the application's services and dependencies.
        /// </summary>
        /// <param name="services">The service collection to configure.</param>
        /// <param name="configuration">The application configuration settings.</param>
        public static void AddApplication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddRepositoryLayer();
            services.AddServiceLayer(configuration);
            services.AddAutoMapper(typeof(Services.DependencyInjection).Assembly);

            // Add DbContext with SQLite
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            services.AddHttpContextAccessor();


            // Add Identity
            services.AddIdentity<ApplicationUser, ApplicationRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddDistributedMemoryCache();

            // Add Firebase and JWT Authentication
            services.AddAuthentication(configuration);

            // Add Authorization
            services.AddAuthorization();

            // Add Swagger Configuration
            services.AddSwaggerDocumentation();

            // Add Controllers and API-related services
            services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

            services.AddEndpointsApiExplorer();
            services.AddMemoryCache();

            // Add Redis Service
            services.AddSingleton<IConnectionMultiplexer>(_ =>
            {
                var redisHost = configuration["Redis:Host"];
                var redisPort = configuration["Redis:Port"];
                var redisPassword = configuration["Redis:Password"];

                var options = new ConfigurationOptions
                {
                    EndPoints = { $"{redisHost}:{redisPort}" },
                    Password = redisPassword,
                    AbortOnConnectFail = false
                };

                return ConnectionMultiplexer.Connect(options);
            });

            services.AddSingleton(provider =>
            {
                var config = provider.GetRequiredService<IConfiguration>();
                var cloudinarySettings = config.GetSection("CloudinarySettings").Get<CloudinarySettings>();

                return new Cloudinary(new Account(
                    cloudinarySettings!.CloudName,
                    cloudinarySettings.ApiKey,
                    cloudinarySettings.ApiSecret
                ));
            });

            // Add MongoDB Configuration
            services.Configure<MongoDBSettings>(configuration.GetSection("MongoDBSettings"));

            var mongoDbSettings = configuration.GetSection("MongoDBSettings").Get<MongoDBSettings>();

            services.AddSingleton<IMongoClient>(s => new MongoClient(mongoDbSettings.ConnectionString));
            services.AddScoped<IMongoDbUnitOfWork, MongoDbUnitOfWork>();

        }

        /// <summary>
        /// Configures authentication services including Firebase and custom JWT authentication.
        /// </summary>
        /// <param name="services">The service collection to configure.</param>
        /// <param name="configuration">The application configuration settings.</param>
        private static void AddAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            if (FirebaseApp.DefaultInstance == null)
            {
                string adminSdkRelativePath = configuration["Firebase:AdminSDKPath"]
                    ?? throw new BaseException.CoreException("config", "Missing Firebase:AdminSDKPath");

                string adminSdkFullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, adminSdkRelativePath);

                FirebaseApp.Create(new AppOptions()
                {
                    Credential = GoogleCredential.FromFile(adminSdkFullPath)
                });
            }

            var jwtKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]
                ?? throw new BaseException.CoreException("config", "Missing Jwt:Key")));

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            // Firebase Authentication
            .AddJwtBearer("Firebase", options =>
            {
                options.Authority = $"https://securetoken.google.com/{configuration["Firebase:ProjectId"]}";
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = $"https://securetoken.google.com/{configuration["Firebase:ProjectId"]}",
                    ValidateAudience = true,
                    ValidAudience = configuration["Firebase:ProjectId"],
                    ValidateLifetime = true
                };
            })
            // Custom JWT Authentication
            .AddJwtBearer("Jwt", options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false, // No Issuer validation
                    ValidateAudience = false, // No Audience validation
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = jwtKey,
                    RoleClaimType = ClaimTypes.Role
                };
            });
        }

        /// <summary>
        /// Configures Swagger documentation for the API.
        /// </summary>
        /// <param name="services">The service collection to configure.</param>
        private static void AddSwaggerDocumentation(this IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "The Coffee Hand API",
                    Version = "v1",
                    Description = "API for The Coffee Hand application"
                });

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter 'Bearer {your Firebase ID token}'"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                new string[] {}
            }
        });

                // Enable XML Comments (for Swagger API Documentation)
                var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
                options.IncludeXmlComments(xmlPath);
            });
        }

    }
}
