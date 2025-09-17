using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BlazorSupabase.App;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");

// Bezbedno učitavanje wwwroot/appsettings.json
AppConfig cfg;
try
{
    using var bootHttp = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };
    cfg = await bootHttp.GetFromJsonAsync<AppConfig>("appsettings.json")
          ?? throw new Exception("Empty appsettings.json");
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Failed to load appsettings.json: {ex.Message}");
    // fallback da se app ne sruši – popunićeš URL/Key pa restartovati
    cfg = new AppConfig(new SupabaseConfig("", ""));
}

builder.Services.AddScoped(_ => new HttpClient());
builder.Services.AddScoped(sp => new SupaClient(cfg));
builder.Services.AddScoped<AppState>();

await builder.Build().RunAsync();

public record AppConfig(SupabaseConfig Supabase);
public record SupabaseConfig(string Url, string AnonKey);
