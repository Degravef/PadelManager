using MudBlazor.Services;
using Web.Components;
using Web.Core.Interfaces;
using Web.Services;
using Web.Services.Context;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
       .AddInteractiveServerComponents();

builder.Services.AddMudServices();

builder.Services.AddScoped<UserContext>();
builder.Services.AddTransient<LoggingHandler>();
builder.Services.AddHttpClient<ISiteService, SiteHttpClient>("API", client =>
       {
           client.BaseAddress = new Uri("https://localhost:7282/");
       })
       .AddHttpMessageHandler<LoggingHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
   .AddInteractiveServerRenderMode();

app.Run();