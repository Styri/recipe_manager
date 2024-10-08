using Dapper;
using Microsoft.Data.Sqlite;
using System.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

builder.WebHost.UseUrls("https://localhost:3000");

// Use SQLite
builder.Services.AddScoped<IDbConnection>(sp =>
    new SqliteConnection("Data Source=recipes.db"));

builder.Services.AddControllers();

var app = builder.Build();

// Initialize database
using (var scope = app.Services.CreateScope())
{
    var dbConnection = scope.ServiceProvider.GetRequiredService<IDbConnection>();
    dbConnection.Open();
    dbConnection.Execute(@"
        CREATE TABLE IF NOT EXISTS recipes (
            recipe_id INTEGER PRIMARY KEY AUTOINCREMENT,
            name TEXT NOT NULL CHECK(length(name) <= 250),
            description TEXT CHECK(length(description) <= 500),
            category TEXT DEFAULT 'Other' CHECK(
                category IN ('Breakfast', 'Brunch', 'Lunch', 'Dinner', 'Supper', 'Other')
            ),
            favorite INTEGER DEFAULT 0 CHECK(favorite IN (0, 1))
        );
    ");
}

//middleware
app.UseCors("AllowAllOrigins");
app.UseRouting();
app.UseHttpsRedirection();
app.MapControllers();

app.Run();