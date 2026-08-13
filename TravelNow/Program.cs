using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TravelNow.Application;
using TravelNow.Extensions;
using TravelNow.Infrastructure;
using TravelNow.Middlewares;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSwaggerPage();

builder.Services.AddApplicationDI();
builder.Services.AddInfrastructureDI(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<TravelNowDbContext>();
        dbContext.Database.Migrate();
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Migration failed. App starting without migrations.");
    }
}

using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        await scope.ServiceProvider.SeedRolesAsync();
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Role seeding failed. App starting without seeded roles.");
    }
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerPage();
}

app.MapControllers();

app.Run();

public partial class Program
{
}