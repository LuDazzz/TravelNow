namespace TravelNow.Extensions;

public static class SwaggerExtensions
{
    public static IServiceCollection AddSwaggerPage(
        this IServiceCollection services,
        string apiTitle = "TravelNow API",
        string apiVersion = "v1")
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc(apiVersion, new Microsoft.OpenApi.OpenApiInfo
            {
                Title = apiTitle,
                Version = apiVersion
            });

            options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = Microsoft.OpenApi.SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = Microsoft.OpenApi.ParameterLocation.Header,
                Description = "Enter 'Bearer' [space] and your JWT token"
            });
        });

        return services;
    }

    public static IApplicationBuilder UseSwaggerPage(
        this IApplicationBuilder app,
        string apiVersion = "v1")
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint($"/swagger/{apiVersion}/swagger.json", $"TravelNow API {apiVersion}");
            options.RoutePrefix = "swagger";
        });

        return app;
    }
}