using System.Text;

using Application.Interfaces;
using Application.Services;

using Domain.Interfaces;

using Infrastructure;
using Infrastructure.Repositories;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

using Shared;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger with JWT support
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "ClassAPI", Version = "v1" });

    // Configure Swagger to accept JWT tokens
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' followed by your token"
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
            Array.Empty<string>()
        }
    });
});

// DbContext (InMemory)
builder.Services.AddDbContext<ClassDbContext>(options =>
    options.UseInMemoryDatabase("ClassDb"));

// Dependency Injection - Repositories
builder.Services.AddScoped<IClassRepository, ClassRepository>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();

// Dependency Injection - Services
builder.Services.AddScoped<IClassService, ClassService>();
builder.Services.AddScoped<IBookingService, BookingService>();

// HTTP Client for AccountAPI
builder.Services.AddHttpClient<IAccountAPIClient, AccountAPIClient>(client =>
{
    var accountApiUrl = builder.Configuration["AccountApi:BaseUrl"] ?? "https://localhost:5001";
    client.BaseAddress = new Uri(accountApiUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// JWT Authentication (same settings as AccountAPI)
var jwtSecret = builder.Configuration["Jwt:Secret"] ??
    throw new InvalidOperationException("JWT Secret is not configured in appsettings.json");
var key = Encoding.UTF8.GetBytes(jwtSecret);

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
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ClockSkew = TimeSpan.Zero // Tokens expire exactly at expiry time
        };
    });

// Authorization Policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("MemberOnly", policy =>
        policy.RequireClaim("role", "Member"));
    options.AddPolicy("InstructorOnly", policy =>
        policy.RequireClaim("role", "Instructor"));
});

var app = builder.Build();

// Configure HTTP pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Shared Middleware
app.UseGlobalExceptionHandler();
app.UseRequestLogging();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Optional: Seed some demo data (if needed)
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ClassDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    if (!context.FitnessClasses.Any())
    {
        logger.LogInformation("Seeding demo class data...");
        var demoClass = new Domain.Entities.FitnessClass
        {
            Id = Guid.NewGuid(),
            Title = "Yoga Basics",
            Description = "Introductory yoga session",
            InstructorId = Guid.Parse("11111111-1111-1111-1111-111111111111"), // Replace with actual instructor ID
            InstructorName = "instructor1",
            ScheduledAt = DateTime.UtcNow.AddDays(1),
            Capacity = 10,
            BookedCount = 0
        };
        context.FitnessClasses.Add(demoClass);
        context.SaveChanges();
        logger.LogInformation("Demo class seeded.");
    }
}

app.Run();