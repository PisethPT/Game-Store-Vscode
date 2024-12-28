using Microsoft.EntityFrameworkCore;

namespace Game.Data;

public static class DataExtension
{
    public static async Task MigrateDbAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<Context>();
        await dbContext.Database.MigrateAsync();
    }
}

