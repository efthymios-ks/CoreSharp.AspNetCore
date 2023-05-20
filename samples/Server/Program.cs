using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Server.Extensions;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
ConfigureServices(services);

var app = builder.Build();
ConfigureApp(app);
app.Run();

static void ConfigureServices(IServiceCollection services)
{
    // System 
    services.AddControllers();
    services.AddEndpointsApiExplorer();
    services.AddAppSwagger();

    // User 
    services.AddAppServices();
    services.AddResponseCompression(options =>
    {
        options.EnableForHttps = true;
    });
}

static void ConfigureApp(WebApplication app)
{
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseAuthorization();
    app.UseResponseCompression();
    app.MapControllers();
}