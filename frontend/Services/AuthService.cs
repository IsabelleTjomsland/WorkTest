using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

public class AuthService
{
    private readonly IJSRuntime _jsRuntime;
    private bool _isAuthenticated;

    // Define the event for authentication state change
    public event Action<bool>? AuthenticationStateChanged;

    public AuthService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<bool> IsUserAuthenticated()
    {
        var token = await GetToken();
        _isAuthenticated = !string.IsNullOrEmpty(token);
        return _isAuthenticated;
    }

    public async Task<string> GetToken()
    {
        return await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "jwtToken");
    }

    public async Task Login(string token)
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "jwtToken", token);
        _isAuthenticated = true;
        AuthenticationStateChanged?.Invoke(true); // Notify subscribers
    }

    public async Task Logout()
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "jwtToken");
        _isAuthenticated = false;
        AuthenticationStateChanged?.Invoke(false); // Notify subscribers
    }
}
