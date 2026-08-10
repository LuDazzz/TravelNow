using TravelNow.Application;
using TravelNow.Extensions;
using TravelNow.Infrastructure;
using TravelNow.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddSwaggerPage();

// Register Application & Infrastructure DI
builder.Services.AddApplicationDI();
builder.Services.AddInfrastructureDI(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
// Global exception handling middleware
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
