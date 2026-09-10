using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.EntityFrameworkCore;
using Roomy.API.Data;
using Roomy.API.Repositories;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Roomy")
    ?? "Data Source=roomy.db";

builder.Services.AddDbContext<RoomyDbContext>(options => options.UseSqlite(connectionString));

builder.Services.AddSingleton(TimeProvider.System);

builder.Services.AddScoped<IHotelRepository, HotelRepository>();
builder.Services.AddScoped<IRoomRepository, RoomRepository>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();

builder.Services.AddFastEndpoints();
builder.Services.SwaggerDocument(options =>
{
    options.DocumentSettings = settings =>
    {
        settings.Title = "Roomy API";
        settings.Version = "v1";
        settings.Description = "Hotel room booking API. Source: https://github.com/caitmcm/Roomy";
    };
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var database = scope.ServiceProvider.GetRequiredService<RoomyDbContext>();
    database.Database.EnsureCreated();
}

app.UseHttpsRedirection();
app.UseFastEndpoints();
app.UseSwaggerGen();

app.MapGet("/", () => Results.Redirect("/swagger"))
   .ExcludeFromDescription();

app.Run();

public partial class Program { }
