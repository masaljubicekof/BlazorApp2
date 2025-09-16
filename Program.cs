using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BlazorSupabase.App;
using System.Net.Http.Json;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");

// učitaj wwwroot/appsettings.json
using var bootHttp = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };
var cfg = await bootHttp.GetFromJsonAsync<AppConfig>("appsettings.json");

// HttpClient za API pozive + naši servisi
builder.Services.AddScoped(_ => new HttpClient());
builder.Services.AddScoped(sp => new SupaClient(cfg!));
builder.Services.AddScoped<AppState>();

await builder.Build().RunAsync();

public record AppConfig(SupabaseConfig Supabase);
public record SupabaseConfig(string Url, string AnonKey);
