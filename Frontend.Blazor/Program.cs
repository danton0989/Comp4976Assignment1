using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Frontend.Blazor;
using Frontend.Blazor.Services;
using Blazored.LocalStorage;



var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "https://comp4976assignment1backend.azurewebsites.net";

// Configure HttpClient
builder.Services.AddScoped(sp => new HttpClient 
{ 
    BaseAddress = new Uri("https://comp4976assignment1backend.azurewebsites.net") 
});

builder.Services.AddBlazoredLocalStorage();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ObituaryService>();
builder.Services.AddScoped<AuthState>();


await builder.Build().RunAsync();
