using Api.Handlers;
using Api.Middleware;
using Bll.Extensions;
using Dal;
using Dal.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services
       .AddAuthentication("HeaderAuth")
       .AddScheme<AuthenticationSchemeOptions, HeaderHandler>("HeaderAuth", null);
builder.Services.AddAuthorization();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddDbContext<PadelDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddDalServices();
builder.Services.AddBllServices();

WebApplication app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();
app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => { options.SwaggerEndpoint("/openapi/v1.json", "PadelManager API v1"); });
}
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
