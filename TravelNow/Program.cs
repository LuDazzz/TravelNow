using Microsoft.EntityFrameworkCore;
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

// Auto-run EF migrations on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<TravelNowDbContext>();
    dbContext.Database.Migrate();
}

using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.SeedRolesAsync().GetAwaiter().GetResult();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment() || builder.Configuration.GetValue<bool>("EnableSwagger"))
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseSwaggerPage();
}

app.MapControllers();

app.Run();

public partial class Program
{
}