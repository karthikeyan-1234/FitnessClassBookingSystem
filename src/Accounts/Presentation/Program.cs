using System.Security.Claims;
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

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger with JWT support
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "AccountAPI", Version = "v1" });

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
builder.Services.AddDbContext<AccountDbContext>(options =>
    options.UseInMemoryDatabase("AccountDb"));

// Dependency Injection - Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Dependency Injection - Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();

// JWT Authentication
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
    options.AddPolicy("InstructorOnly", policy =>
        policy.RequireClaim(ClaimTypes.Role, "Instructor"));
    options.AddPolicy("MemberOnly", policy =>
        policy.RequireClaim(ClaimTypes.Role, "Member"));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Shared middleware
app.UseGlobalExceptionHandler();
app.UseRequestLogging();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Seed initial data (optional)
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AccountDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

    if (!context.Users.Any())
    {
        logger.LogInformation("Seeding initial users...");

        var instructor = new Domain.Entities.User
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Username = "instructor1",
            PasswordHash = hasher.Hash("password"),
            Role = Domain.Enums.Role.Instructor
        };

        var member = new Domain.Entities.User
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Username = "member1",
            PasswordHash = hasher.Hash("password"),
            Role = Domain.Enums.Role.Member
        };

        context.Users.AddRange(instructor, member);
        context.SaveChanges();
        logger.LogInformation("Seed completed. Instructor (instructor1/password), Member (member1/password)");
    }
}

app.Run();