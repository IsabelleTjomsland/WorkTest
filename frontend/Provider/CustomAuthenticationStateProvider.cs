using System.Security.Claims;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using Blazored.LocalStorage;

public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly ILocalStorageService _localStorageService;
    private readonly HttpClient _httpClient;

    public CustomAuthenticationStateProvider(ILocalStorageService localStorageService, HttpClient httpClient)
    {
        _localStorageService = localStorageService;
        _httpClient = httpClient;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _localStorageService.GetItemAsync<string>("jwtToken");
        var identity = new ClaimsIdentity();

        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var claims = ParseClaimsFromJwt(token);
            identity = new ClaimsIdentity(claims, "jwt");
        }

        var user = new ClaimsPrincipal(identity);
        return new AuthenticationState(user);
    }

    public async Task MarkUserAsAuthenticated(string token)
    {
        await _localStorageService.SetItemAsync("jwtToken", token);

        var claims = ParseClaimsFromJwt(token);
        var identity = new ClaimsIdentity(claims, "jwt");
        var user = new ClaimsPrincipal(identity);

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
    }

    public async Task MarkUserAsLoggedOut()
    {
        await _localStorageService.RemoveItemAsync("jwtToken");
        _httpClient.DefaultRequestHeaders.Authorization = null;

        var user = new ClaimsPrincipal(new ClaimsIdentity());
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
    }

    private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var payload = jwt.Split('.')[1];
        var jsonBytes = ParseBase64WithoutPadding(payload);
        var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

        var claims = keyValuePairs.Select(kvp => new Claim(kvp.Key, kvp.Value.ToString())).ToList();

        // Optional: Map common JWT claim names to .NET standard types
        if (keyValuePairs.TryGetValue("name", out var name))
        {
            claims.Add(new Claim(ClaimTypes.Name, name.ToString()));
        }
        else if (keyValuePairs.TryGetValue("sub", out var sub))
        {
            claims.Add(new Claim(ClaimTypes.Name, sub.ToString()));
        }

        return claims;
    }

    private byte[] ParseBase64WithoutPadding(string base64)
    {
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }

        return Convert.FromBase64String(base64);
    }
}
