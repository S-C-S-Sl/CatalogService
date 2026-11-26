
using Asp.Versioning;
using CatalogService.Interactors;
using CatalogService.Resources;
using CatalogService.Resources.MongoDb;
using Mappify;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;

namespace CatalogService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            AddWebServices(builder);
            AddApiVersioningServices(builder.Services);
            ConfigureForwardedHeaders(builder);
            AddCatalogServices(builder);

            var app = builder.Build();
            ConfigureRequestPipeline(app);

            app.Run();
        }

        private static void AddWebServices(WebApplicationBuilder builder)
        {
            builder.Services.AddControllers();
            builder.Services.AddHealthChecks();
        }

        private static void AddApiVersioningServices(IServiceCollection services)
        {
            services.AddApiVersioning(options =>
                {
                    options.ApiVersionReader = new UrlSegmentApiVersionReader();
                    options.ReportApiVersions = true;
                })
                .AddMvc()
                .AddApiExplorer(options =>
                {
                    options.GroupNameFormat = "'v'VVV";          // Dokumentname v1 statt 1.0
                    options.SubstituteApiVersionInUrl = true;    // Pfade im Dokument: v1/products statt v{version}/products
                })
                .AddOpenApi();
        }

        private static void ConfigureForwardedHeaders(WebApplicationBuilder builder)
        {
            builder.Services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
                                                                          | ForwardedHeaders.XForwardedHost | ForwardedHeaders.XForwardedPrefix;
                foreach (var network in builder.Configuration.GetSection("ForwardedHeaders:KnownNetworks").Get<string[]>() ?? [])
                {
                    options.KnownIPNetworks.Add(System.Net.IPNetwork.Parse(network));
                }
            });
        }

        private static void AddCatalogServices(WebApplicationBuilder builder)
        {
            builder.Services.AddMappify();
            builder.Services.AddSingleton<IProductIdGenerator, GuidProductIdGenerator>();
            builder.Services.AddMappifyProfile<ProductMappingProfile>();
            builder.Services.AddMongoDbResources(builder.Configuration);
            builder.Services.AddSingleton<IProductInteractor, ProductInteractor>();
        }

        private static void ConfigureRequestPipeline(WebApplication app)
        {
            app.UseForwardedHeaders();

            if (app.Environment.IsDevelopment())
            {
                app.UseHttpsRedirection();
            }

            app.UseAuthorization();
            app.MapOpenApi().WithDocumentPerVersion();
            app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
            app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = check => check.Tags.Contains("ready") });

            app.MapControllers();
        }
    }
}
