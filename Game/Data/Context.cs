using System;
using Game.Models;
using Microsoft.EntityFrameworkCore;

namespace Game.Data;

public class Context : DbContext
{
    public DbSet<Games> Games => Set<Games>();
    public Context(DbContextOptions<Context> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Games>().HasData(
            new
            {
                Id = 1,
                Title = "PES 2025",
                Description = "PES 2025 Football Game.",
                Price = 49.99M,
                Image = "img.png"
            }
        );
        base.OnModelCreating(modelBuilder);
    }
}
