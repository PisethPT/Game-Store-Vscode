using Game.Data;
using Game.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultSQLite");
builder.Services.AddSqlite<Context>(connectionString);

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin() // WithOrigins("http://127.0.0.1:5500")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();
var group = app.MapGroup("/").DisableAntiforgery();

group.MapGet("/", () => "Hello World! ASP.NET Web API");

group.MapGet("/Games", async (Context context) =>
{
    var games = await context.Games.ToListAsync();
    return Results.Ok(games.OrderByDescending(g => g.Id));
});

group.MapPost("/Games/PostGame", async (IFormFile image, [FromForm] Games newGame, Context context) =>
{
	if (image != null && image.Length > 0)
	{
		var uploadsPath = Path.Combine(builder.Environment.WebRootPath, "Images");

		if (!Directory.Exists(uploadsPath))
		{
			Directory.CreateDirectory(uploadsPath);
		}

		var fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
		var filePath = Path.Combine(uploadsPath, fileName);

		using (var fileStream = new FileStream(filePath, FileMode.Create))
		{
			await image.CopyToAsync(fileStream);
		}

		newGame.Image = "/Resources/" + fileName;

		var jsonFilePath = Path.Combine(builder.Environment.ContentRootPath, "data.json");

		List<Games> gamesList;
		if (File.Exists(jsonFilePath))
		{
			var jsonData = await File.ReadAllTextAsync(jsonFilePath);
			gamesList = JsonSerializer.Deserialize<List<Games>>(jsonData) ?? new List<Games>();
		}
		else
		{
			gamesList = new List<Games>();
		}

		gamesList.Add(newGame);

		// Serialize the updated list back to JSON
		var updatedJsonData = JsonSerializer.Serialize(gamesList, new JsonSerializerOptions { WriteIndented = true });

		// Write the updated JSON back to the file
		await File.WriteAllTextAsync(jsonFilePath, updatedJsonData);

		await context.Games.AddAsync(newGame);
		await context.SaveChangesAsync();
	}

	return Results.Ok(newGame);
});

await app.MigrateDbAsync();

app.UseCors();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(builder.Environment.WebRootPath, "Images")),
    RequestPath = "/Resources"
});

app.Run();