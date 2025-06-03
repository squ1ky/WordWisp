using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using WordWisp.API.Data;
using WordWisp.API.Repositories.Interfaces;
using WordWisp.API.Repositories.Implementations;
using WordWisp.API.Services.Interfaces;
using WordWisp.API.Services.Implementations;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");
var jwtKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY");

// Email Config

builder.Configuration["Email:Host"] = Environment.GetEnvironmentVariable("EMAIL_HOST");
builder.Configuration["Email:Port"] = Environment.GetEnvironmentVariable("EMAIL_PORT");
builder.Configuration["Email:Username"] = Environment.GetEnvironmentVariable("EMAIL_USERNAME");
builder.Configuration["Email:Password"] = Environment.GetEnvironmentVariable("EMAIL_PASSWORD");
builder.Configuration["Email:From"] = Environment.GetEnvironmentVariable("EMAIL_FROM");

// Db Config

builder.Configuration["ConnectionStrings:DefaultConnection"] = connectionString;

// JWT Config

builder.Configuration["Jwt:Key"] = jwtKey;

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWebApp", policy =>
    {
        policy.WithOrigins("https://localhost:5261", "http://localhost:5261")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // for cookies/auth
    });
});

// JWT

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtKey)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ClockSkew = TimeSpan.Zero
        };
    });

// Auth

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailService, YandexEmailService>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserContextService, UserContextService>();

// Dictionary and Words

builder.Services.AddScoped<IDictionaryRepository, DictionaryRepository>();
builder.Services.AddScoped<IWordRepository, WordRepository>();

builder.Services.AddScoped<IDictionaryService, DictionaryService>();
builder.Services.AddScoped<IWordService, WordService>();
builder.Services.AddScoped<IUserService, UserService>();


// Db

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// CORS

app.UseCors("AllowWebApp");

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
