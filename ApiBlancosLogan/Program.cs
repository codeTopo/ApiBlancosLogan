using ApiBlancosLogan.Models;
using ApiBlancosLogan.Tools;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
//todo esto es el jwt
builder.Configuration.AddJsonFile("appsettings.json");
var secretkey = builder.Configuration.GetValue<string>("settings:secretkey");
var allowedOrigins = builder.Configuration.GetSection("settings:AllowedOrigins").Get<string[]>();

if (string.IsNullOrEmpty(secretkey))
{
    // Manejar el caso cuando secretkey es null o una cadena vacía
    throw new InvalidOperationException("La clave 'settings:secretkey' no está configurada correctamente.");
}
var keyBytes = Encoding.UTF8.GetBytes(secretkey);

builder.Services.AddSingleton<JwtService>(_ => new JwtService(secretkey));
builder.Services.AddAuthentication(config =>
{
    config.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    config.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(config =>
{
    config.RequireHttpsMetadata = false;
    config.SaveToken = true;
    config.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
        ValidateIssuer = false,
        ValidateAudience = false,
    };
});
builder.Services.AddControllers();
//esto es para la coneccion a la base de datos 
builder.Services.AddDbContext<BlancosLoganContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), sqlServerOptions =>
    {
        sqlServerOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30), 
            errorNumbersToAdd: null 
        );
    })
);
//esto es para el cors quienes entran
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost4200", builder =>
    {
        builder.WithOrigins(allowedOrigins!)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
builder.Services.AddScoped<MercadoPagoService>();
var app = builder.Build();
// Configure the HTTP request pipeline.
app.UseHttpsRedirection();
app.UseCors("AllowLocalhost4200");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
