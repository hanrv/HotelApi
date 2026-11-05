using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using HotelApi.Data;
using HotelApi.Data.Repositories;
using HotelApi.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Evita errores de ciclos infinitos JSON al usar .Include() y permite serializar enums
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configuración de Entity Framework Core con SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Inyección del repositorio de reservas
builder.Services.AddScoped<IReservaRepository, ReservaRepository>();

var app = builder.Build();

// Manejador de excepciones globales
app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();