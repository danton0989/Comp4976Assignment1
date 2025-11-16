using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Frontend.Blazor;
using Frontend.Blazor.Services;
using Blazored.LocalStorage;



var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Get backend URL from Aspire service discovery or configuration
var apiBaseUrl = builder.Configuration["services:obituaryapi:https:0"] 
    ?? builder.Configuration["services:obituaryapi:http:0"]
    ?? builder.Configuration["ApiBaseUrl"]
    ?? "https://localhost:7223"; // Fallback for standalone development

Console.WriteLine($"🔗 Using API base URL: {apiBaseUrl}");

// Configure HttpClient to use backend
builder.Services.AddScoped(sp => new HttpClient 
{ 
    BaseAddress = new Uri(apiBaseUrl)
});

builder.Services.AddBlazoredLocalStorage();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ObituaryService>();
builder.Services.AddScoped<AuthState>();


await builder.Build().RunAsync();
