using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Blazored.LocalStorage;

namespace Frontend.Blazor.Services;

public class AuthService
{
    private readonly HttpClient _http;
    private readonly ILocalStorageService _localStorage;
    private readonly AuthState _authState;
    private const string TokenKey = "jwt_token";

    public AuthService(HttpClient http, ILocalStorageService localStorage, AuthState authState)
    {
        _http = http;
        _localStorage = localStorage;
        _authState = authState;
    }


   public async Task<bool> LoginAsync(string email, string password)
{
    var response = await _http.PostAsJsonAsync("api/auth/login", new { Email = email, Password = password });
    if (!response.IsSuccessStatusCode)
        return false;

    var json = await response.Content.ReadAsStringAsync();
    using var doc = JsonDocument.Parse(json);
    var token = doc.RootElement.GetProperty("token").GetString();

    if (string.IsNullOrWhiteSpace(token))
        return false;

    await _localStorage.SetItemAsync(TokenKey, token);
    _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    _authState.SetLoginState(true); // notify UI
    return true;
}

public async Task LogoutAsync()
{
    await _localStorage.RemoveItemAsync(TokenKey);
    _http.DefaultRequestHeaders.Authorization = null;
    _authState.SetLoginState(false); // notify UI
}


    public async Task<bool> IsLoggedInAsync()
    {
        var token = await _localStorage.GetItemAsync<string>(TokenKey);
        return !string.IsNullOrEmpty(token);
    }

    public async Task<string?> GetTokenAsync()
    {
        return await _localStorage.GetItemAsync<string>(TokenKey);
    }

    public async Task<bool> RegisterAsync(string email, string password)
    {
        var response = await _http.PostAsJsonAsync("api/auth/register", new { Email = email, Password = password });
        return response.IsSuccessStatusCode;
    }
}
