using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Worktest.backend.Models;
using Worktest.backend.Services;

var builder = WebApplication.CreateBuilder(args);

// Lägg till User Secrets för känsliga inställningar (t.ex. JWT hemlig nyckel)
builder.Configuration.AddUserSecrets<Program>();

// Lägg till tjänster i DI-containern
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Konfigurera Entity Framework Core med rätt anslutningssträng
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DevicesConnection")));

// Registrera ModbusService som en Singleton
builder.Services.AddSingleton<ModbusService>();

// Lägg till CORS-policy för Blazor frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazor", policy =>
        policy.WithOrigins("http://localhost:5165")  // Använd rätt port för din Blazor frontend
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials());  // Om du använder cookies eller andra credentials
});

// Lägg till JWT Authentication för backend
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var secretKey = builder.Configuration["JwtSettings:SecretKey"];
        if (string.IsNullOrEmpty(secretKey))
        {
            throw new InvalidOperationException("JWT SecretKey is missing in configuration.");
        }

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
        };
    });

// Lägg till autoriseringstjänster
builder.Services.AddAuthorization();

var app = builder.Build();

// Aktivera Swagger i utvecklingsläge
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Aktivera CORS (Cross-Origin Resource Sharing) för Blazor frontend
app.UseCors("AllowBlazor");

// Lägg till autentisering och auktorisering
app.UseAuthentication();
app.UseAuthorization();

// Mappa alla controllers
app.MapControllers();

// Kör appen
app.Run();
