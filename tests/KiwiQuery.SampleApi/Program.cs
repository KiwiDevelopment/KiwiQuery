using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using KiwiQuery.SampleApi;

var builder = WebApplication.CreateBuilder(args);

var keepAlive = new SqliteConnection("DataSource=:memory:");
keepAlive.Open();

builder.Services.AddSingleton(keepAlive);
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(keepAlive));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
    db.Products.AddRange(
        new Product { Name = "Apple" },
        new Product { Name = "Banana" });
    db.SaveChanges();
}

app.MapGet("/products", async (AppDbContext db) =>
    await db.Products.ToListAsync());

app.MapGet("/products/{id}", async (int id, AppDbContext db) =>
    await db.Products.FindAsync(id) is { } product
        ? Results.Ok(product)
        : Results.NotFound());

app.Run();

public partial class Program { }
