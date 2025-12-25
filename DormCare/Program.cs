using System;
using Serilog;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using Prometheus;
using DormCare.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ApiExplorer;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog (console + Elasticsearch)
var elasticUri = builder.Configuration["Elasticsearch:Uri"] ?? "http://localhost:9200";
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.Elasticsearch(new Serilog.Sinks.Elasticsearch.ElasticsearchSinkOptions(new Uri(elasticUri))
    {
        AutoRegisterTemplate = true
    })
    .CreateLogger();
builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

// API versioning + Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddApiVersioning(options =>
{
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(2, 0);
    options.ReportApiVersions = true;
});

builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "DormCare API V1", Version = "v1" });
    c.SwaggerDoc("v2", new OpenApiInfo { Title = "DormCare API V2", Version = "v2" });
    c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
});

// OpenTelemetry Tracing & Metrics
var serviceName = "DormCare";
var serviceVersion = "1.0.0";

// Create a TracerProvider via SDK (avoids dependency on IServiceCollection extension methods)
var tracerProvider = Sdk.CreateTracerProviderBuilder()
    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName: serviceName, serviceVersion: serviceVersion))
    .AddAspNetCoreInstrumentation()
    .AddHttpClientInstrumentation()
    .AddSource("DormCare")
    .AddZipkinExporter(o => { o.Endpoint = new Uri(builder.Configuration["Zipkin:Endpoint"] ?? "http://localhost:9411/api/v2/spans"); })
    .Build();

// EF Core multi-provider configuration
var provider = builder.Configuration.GetValue<string>("DatabaseProvider") ?? "Sqlite";
builder.Services.AddDbContext<DormCare.Data.ApplicationDbContext>(options =>
{
    switch (provider.ToLowerInvariant())
    {
        case "mssql":
        case "sqlserver":
            options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer") ?? builder.Configuration.GetConnectionString("DefaultConnection"));
            break;
        case "postgres":
        case "postgresql":
            options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres") ?? builder.Configuration.GetConnectionString("DefaultConnection"));
            break;
        case "inmemory":
            options.UseInMemoryDatabase("DormCareInMemory");
            break;
        default:
            options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=dormcare.db");
            break;
    }
});

// Configure authentication: local cookie + optional external OpenID Connect provider (e.g. Okta)
builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
    })
    .AddGoogle("Google", options =>
    {
        options.ClientId = builder.Configuration["Google:ClientId"];
        options.ClientSecret = builder.Configuration["Google:ClientSecret"];
        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.Scope.Add("profile");
        options.Scope.Add("email");
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Enable Swagger UI in development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "DormCare API V1");
        c.SwaggerEndpoint("/swagger/v2/swagger.json", "DormCare API V2");
    });
}

// Expose Prometheus metrics endpoint using prometheus-net
app.UseHttpMetrics();
app.MapMetrics();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        if (db.Database.IsRelational())
        {
            db.Database.Migrate();
        }
        else
        {
            db.Database.EnsureCreated();
        }
    }
    catch (Exception ex)
    {
        // Log and continue so the app doesn't crash due to migration issues in dev environment
        try
        {
            Serilog.Log.Error(ex, "Database migration failed");
        }
        catch { }
    }
}

app.Run();
