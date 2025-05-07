using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Restaurant.Api.Data;
using Restaurant.Api.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(opts =>
    opts.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Restaurant API V1");
        c.RoutePrefix = "swagger";
    });
}

using (var scope = app.Services.CreateScope())
{
    var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    db.Database.Migrate();

    if (!db.MenuItems.Any())
    {
        var jsonPath = Path.Combine(env.WebRootPath, "db", "db.json");
        var json = await File.ReadAllTextAsync(jsonPath);
        var seedData = JsonSerializer.Deserialize<List<SeedItem>>(
            json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        )!;

        foreach (var s in seedData)
        {
            db.MenuItems.Add(new MenuItem
            {
                Name = s.name,
                Description = s.description,
                Price = decimal.Parse(s.price),
                Category = s.category,
                ImagePath = Path.Combine("img", Path.GetFileName(s.img))
                                     .Replace("\\", "/")
            });
        }
        await db.SaveChangesAsync();
    }
}

app.UseDefaultFiles();
app.UseStaticFiles();
app.MapControllers();
app.MapFallbackToFile("index.html");
app.Run();

public record SeedItem(string id, string img, string name, string? description, string price, string category);
