
using Biogenom.Application.Handlers;
using Biogenom.Application.Interfaces;
using Biogenom.Domain.Interfaces.Repositories;
using Biogenom.Infostructure.Data;
using Biogenom.Infrastructure.Repositories;
using Biogenom.Infrastructure.ServiceOptions;
using Biogenom.Infrastructure.Services;
using GigaChatImageAnalyzer.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;

namespace Biogenom.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();

            builder.Services.AddDbContext<BiogenomDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddScoped<IAnalysisRequestRepository, AnalysisRequestRepository>();
            builder.Services.AddScoped<IMaterialRepository, MaterialRepository>();
            builder.Services.AddMediatR(cfg =>
            {
                // Регистрируем обработчики из сборки Application
                cfg.RegisterServicesFromAssembly(typeof(AnalyzeImageCommandHandler).Assembly);
                cfg.RegisterServicesFromAssembly(typeof(ConfirmItemsCommandHandler).Assembly);
            });
            builder.Services.Configure<GigaChatOptions>(builder.Configuration.GetSection("GigaChat"));
            builder.Services.Configure<GigaChatPromptsOptions>(builder.Configuration.GetSection("GigaChatPrompts"));
            
            builder.Services.AddHttpClient("giga")
                .ConfigurePrimaryHttpMessageHandler(() =>
                    new HttpClientHandler
                    {
                        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                    });
            

            builder.Services.AddScoped<IAiAuthorizationsService, GigaChatAuthorizationService>();
            builder.Services.AddScoped<GigaChatFileUploader>();
            builder.Services.AddScoped<GigaChatChatClient>();
            builder.Services.AddScoped<IAiService, GigaChatService>();
            builder.Services.AddScoped<IImageDownloader, ImageDownloader>();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.EnableAnnotations();
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Biogenom API", Version = "v1" });
            });

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<BiogenomDbContext>();
                var attempts = 12;
                var delayMs = 2000;
                while (true)
                {
                    try
                    {
                        db.Database.EnsureCreated();
                        break;
                    }
                    catch (Exception ex)
                    {
                        attempts--;
                        if (attempts <= 0)
                            throw;
                        Thread.Sleep(delayMs);
                    }
                }
            }
            // Configure the HTTP request pipeline.
            //if (app.Environment.IsDevelopment())
            //{
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "BiogenomAPI V1");
                    
                });
            //}

            //app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
