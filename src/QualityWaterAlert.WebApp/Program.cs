using MudBlazor.Services;
using QualityWaterAlert.Infrastructure.Interfaces;
using QualityWaterAlert.Infrastructure.Providers;
using QualityWaterAlert.WebApp.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMudServices();

// Named client for data.gouv.fr — range requests only fetch a few MB per call
builder.Services.AddHttpClient("DataGouvFr", client =>
{
    client.BaseAddress = new Uri("https://www.data.gouv.fr/");
    client.DefaultRequestHeaders.Add("User-Agent", "QualityWaterAlert/1.0");
    client.Timeout = TimeSpan.FromMinutes(2);
});

// Singleton so the ZIP and per-department caches persist across requests
builder.Services.AddSingleton<IDataProvider>(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    return new DataGouvFrProvider(factory.CreateClient("DataGouvFr"));
});

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
