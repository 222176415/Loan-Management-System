using LoanManagementSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using LoanManagementSystem.Application.Interfaces;
using LoanManagementSystem.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using LoanManagementSystem.Application.DTOs;
using LoanManagementSystem.Infrastructure.Hubs;
using LoanManagementSystem.Infrastructure.Workers;
using Scalar.AspNetCore;
using LoanManagementSystem.Api.Controllers.Hubs;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://*:{port}");

// 1. Database & Infrastructure
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DbConnection")));

builder.Services.AddHttpContextAccessor();

// 2. Business Services
builder.Services.AddScoped<ICurrentTenantService, CurrentTenantService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<LoanService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<RoleService>();
builder.Services.AddScoped<OrganizationService>();
builder.Services.AddScoped<PaymentService>();
builder.Services.AddScoped<ClientService>();
// 3. SignalR & Modern OpenAPI
builder.Services.AddSignalR();
//builder.Services.AddOpenApi(); 
builder.Services.AddHealthChecks();
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes.Add("Bearer", new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Enter your JWT token"
        });

        document.SecurityRequirements.Add(new OpenApiSecurityRequirement
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
                Array.Empty<string>()
            }
        });

        return Task.CompletedTask;
    });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };

       
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddHostedService<ChronosWorker>();
builder.Services.AddScoped<IExcelService, ExcelService>();
builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
// 5. CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowedCorsOrigins", policy =>
    {
        /*policy.WithOrigins("http://localhost:3000") */
        policy.WithOrigins("https://loanmanagementsystem-eta.vercel.app/")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials(); 
    });
});

builder.Services.AddControllers();

var app = builder.Build();

// 6. Middleware Pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi(); 
    app.MapScalarApiReference(); 
}

app.UseHttpsRedirection();
app.UseCors("AllowedCorsOrigins");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers(); 
app.MapHub<NotificationHub>("/hubs/notifications");
app.MapHub<AuditHub>("/hubs/audit");
app.UseHealthChecks("/health");

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        Console.WriteLine("---- Creating scope for database migration...");
        var context = services.GetRequiredService<ApplicationDbContext>();
        Console.WriteLine("System: Checking pending migrations...");
        await context.Database.MigrateAsync();

        Console.WriteLine("---Seeding database...");
        await DatabaseSeeder.SeedAsync(context);
        Console.WriteLine("-- Database seeding completed successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Migration/Seed Error: {ex.Message}");
    }
}

Console.WriteLine("Step 18: Application is starting – running...");
app.Run();
