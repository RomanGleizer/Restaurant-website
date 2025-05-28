using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Restaurant.Api.Data;
using Restaurant.Api.Models;
using Restaurant.Api.Services;

var builder = WebApplication.CreateBuilder(args);

var yandexConfig = builder.Configuration.GetSection("YandexCloud");
var accessKey = yandexConfig["AccessKey"];
var secretKey = yandexConfig["SecretKey"];
var bucketName = yandexConfig["BucketName"];

builder.Services.AddSingleton<IObjectStorageService>(sp =>
    new YandexObjectStorageService(accessKey, secretKey, bucketName));

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
    var storageService = scope.ServiceProvider.GetRequiredService<IObjectStorageService>();

    await UploadLocalFilesToObjectStorageAndUpdateDb(db, storageService, env);

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
            var imageFileName = Path.GetFileName(s.img);
            var objectStorageUrl = $"https://{bucketName}.storage.yandexcloud.net/{imageFileName}";

            db.MenuItems.Add(new MenuItem
            {
                Name = s.name,
                Description = s.description,
                Price = decimal.Parse(s.price),
                Category = s.category,
                ImagePath = objectStorageUrl
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

async Task UploadLocalFilesToObjectStorageAndUpdateDb(ApplicationDbContext db, IObjectStorageService storage, IWebHostEnvironment env)
{
    var folders = new[]
    {
        Path.Combine(env.WebRootPath, "db", "img"),
        Path.Combine(env.WebRootPath, "img")
    };

    foreach (var folder in folders)
    {
        if (!Directory.Exists(folder)) continue;
        var files = Directory.GetFiles(folder);
        foreach (var file in files)
        {
            var fileName = Path.GetFileName(file);
            if (!db.MenuItems.Any(mi => mi.ImagePath.Contains(fileName)))
            {
                using var fs = File.OpenRead(file);
                var url = await storage.UploadFileAsync(fs, fileName);

                var itemsToUpdate = db.MenuItems.Where(mi => mi.ImagePath.EndsWith(fileName)).ToList();
                foreach (var item in itemsToUpdate)
                {
                    item.ImagePath = url;
                }
                await db.SaveChangesAsync();
            }
        }
    }

    var singleFileName = "hamburger_menu_icon_259215.png";
    var singleFilePath = Path.Combine(env.WebRootPath, singleFileName);
    if (File.Exists(singleFilePath))
    {
        using var fs = File.OpenRead(singleFilePath);
        await storage.UploadFileAsync(fs, singleFileName);
    }
}

public record SeedItem(string id, string img, string name, string? description, string price, string category);