using System.Net.Http.Headers;
using System.Net.Http.Json;
using Frontend.Blazor.Models;

namespace Frontend.Blazor.Services;

public class ObituaryService
{
    private readonly HttpClient _http;
    private readonly AuthService _auth;

    public ObituaryService(HttpClient http, AuthService auth)
    {
        _http = http;
        _auth = auth;
    }

    private async Task SetAuthHeaderAsync()
    {
        var token = await _auth.GetTokenAsync();
        if (!string.IsNullOrEmpty(token))
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<List<Obituary>?> GetAllAsync(string? search = null)
    {
        var url = "api/Obituaries";
        if (!string.IsNullOrWhiteSpace(search))
            url += $"?search={Uri.EscapeDataString(search)}";

        return await _http.GetFromJsonAsync<List<Obituary>>(url);
    }

    public async Task<Obituary?> GetAsync(int id)
    {
        return await _http.GetFromJsonAsync<Obituary>($"api/Obituaries/{id}");
    }

    public async Task<bool> CreateAsync(ObituaryCreateDto dto)
    {
        await SetAuthHeaderAsync();

        using var form = new MultipartFormDataContent();
        form.Add(new StringContent(dto.FullName), "FullName");
        form.Add(new StringContent(dto.DateOfBirth.ToString("yyyy-MM-dd")), "DateOfBirth");
        form.Add(new StringContent(dto.DateOfDeath.ToString("yyyy-MM-dd")), "DateOfDeath");
        form.Add(new StringContent(dto.Biography), "Biography");

        if (dto.PhotoFile != null)
            form.Add(new StreamContent(dto.PhotoFile.OpenReadStream(5_000_000)), "Photo", dto.PhotoFile.Name);

        var res = await _http.PostAsync("api/Obituaries", form);
        return res.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateAsync(int id, ObituaryUpdateDto dto)
    {
        await SetAuthHeaderAsync();
        var res = await _http.PutAsJsonAsync($"api/Obituaries/{id}", dto);
        return res.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        await SetAuthHeaderAsync();
        var res = await _http.DeleteAsync($"api/Obituaries/{id}");
        return res.IsSuccessStatusCode;
    }
}
