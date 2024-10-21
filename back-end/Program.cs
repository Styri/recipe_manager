using Dapper;
using Microsoft.Data.Sqlite;
using System.Data;
using Microsoft.Extensions.FileProviders;
using System.Diagnostics;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddCors(options => 
{
    options.AddPolicy("AllowAllOrigins", builder => 
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.WebHost.UseUrls("http://localhost:3000"); 


builder.Services.AddScoped<IDbConnection>(sp => new SqliteConnection("Data Source=recipes.db"));
builder.Services.AddControllers();

var app = builder.Build();

var url = "http://localhost:3000/index.html";
Process.Start(new ProcessStartInfo
{
    FileName = url,
    UseShellExecute = true 
});

using (var scope = app.Services.CreateScope()) 
{
    var dbConnection = scope.ServiceProvider.GetRequiredService<IDbConnection>();
    dbConnection.Open();
    dbConnection.Execute(@"
        CREATE TABLE IF NOT EXISTS recipes (
            recipe_id INTEGER PRIMARY KEY AUTOINCREMENT,
            name TEXT NOT NULL CHECK(length(name) <= 250),
            description TEXT NOT NULL CHECK(length(description) <= 500),
            category TEXT DEFAULT 'Other' CHECK(category IN ('Breakfast', 'Brunch', 'Lunch', 'Dinner', 'Supper', 'Other')),
            favorite INTEGER DEFAULT 0 CHECK(favorite IN (0, 1))
        );
    ");
    dbConnection.Execute(@"CREATE TABLE IF NOT EXISTS users (
    user_id INTEGER PRIMARY KEY AUTOINCREMENT,
    username TEXT NOT NULL UNIQUE,
    passwordhash TEXT NOT NULL
        );
    ");
}

// Middleware
app.UseCors("AllowAllOrigins");
app.UseRouting();

app.UseStaticFiles(new StaticFileOptions 
{ 
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "../front-end")),
    RequestPath = ""
});

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapFallbackToFile("index.html");

app.Run();
