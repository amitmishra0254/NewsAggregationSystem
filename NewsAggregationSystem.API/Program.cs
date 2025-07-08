using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NewsAggregationSystem.API.Configurations;
using NewsAggregationSystem.API.Extensions;
using NewsAggregationSystem.Common.Constants;
using NewsAggregationSystem.Common.DTOs.SmtpProvider;
using NewsAggregationSystem.DAL.DbContexts;
using Serilog;
using Serilog.Events;
using System.Text;

namespace NewsAggregationSystem.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File(
                    path: "Logs/news-aggregation-system-.log",
                    rollingInterval: RollingInterval.Day,
                    fileSizeLimitBytes: 10 * 1024 * 1024, // 10MB
                    retainedFileCountLimit: 30,
                    rollOnFileSizeLimit: true,
                    shared: true,
                    flushToDiskInterval: TimeSpan.FromSeconds(1),
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
                )
                .CreateLogger();

            try
            {
                Log.Information("Starting News Aggregation System API");
                var builder = WebApplication.CreateBuilder(args);

                // Add services to the container.
                builder.Configuration
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables();

                builder.Services.AddDbContext<NewsAggregationSystemContext>(options =>
                {
                    options.UseSqlServer(builder.Configuration.GetConnectionString(ApplicationConstants.NewsAggregationSystemDbConnection),
                        migration => migration.MigrationsAssembly("NewsAggregationSystem.Migrations"));
                });

                builder.Services.Configure<EmailSettings>(
                    builder.Configuration.GetSection("EmailSettings"));

                builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

                builder.Services.AddHttpContextAccessor();
                builder.Services.RegisterRepositories();
                builder.Services.RegisterServices();
                builder.Services.RegisterProviders();
                builder.Host.UseSerilog();
                builder.Services.AddControllers();

                builder.Services.AddHangfire(config =>
                    config.UseSqlServerStorage(builder.Configuration.GetConnectionString("NewsAggregationSystemDbConnection")));

                builder.Services.AddHangfireServer();

                // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("v1", new OpenApiInfo { Title = "News Aggregation System", Version = "v1" });

                    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                    {
                        Description = "Standrad Authorization header using the Bearer scheme (\"bearer {token}\")",
                        Name = "Authorization",
                        In = ParameterLocation.Header,
                        Type = SecuritySchemeType.ApiKey,
                        Scheme = "Bearer"
                    });

                    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
                            new string[]{ }
                        }
                    });
                });

                builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:key"]))
                    };
                });

                var app = builder.Build();

                // Configure the HTTP request pipeline.
                if (app.Environment.IsDevelopment())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI();
                }

                app.UseHangfireDashboard();

                using (var scope = app.Services.CreateScope())
                {
                    var services = scope.ServiceProvider;
                    services.RegisterRecurringJobs();
                }

                app.UseHttpsRedirection();

                app.UseAuthorization();

                app.MapControllers();

                app.Run();
            }
            catch (Exception exception)
            {
                Log.Fatal(exception, "Application terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}