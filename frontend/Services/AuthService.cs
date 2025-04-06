using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

public class AuthService
{
    private readonly IJSRuntime _jsRuntime;
    private bool _isAuthenticated;

    /// <summary>
    /// Event triggered when the authentication state changes.
    /// </summary>
    public event Action<bool>? AuthenticationStateChanged;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthService"/> class.
    /// </summary>
    /// <param name="jsRuntime">The JS Runtime service for interacting with the browser's localStorage.</param>
    public AuthService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    /// <summary>
    /// Checks if the user is authenticated by verifying the existence of a JWT token in localStorage.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The result is <c>true</c> if the user is authenticated, otherwise <c>false</c>.</returns>
    public async Task<bool> IsUserAuthenticated()
    {
        var token = await GetToken();
        _isAuthenticated = !string.IsNullOrEmpty(token);
        return _isAuthenticated;
    }

    /// <summary>
    /// Retrieves the JWT token from the browser's localStorage.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The result is the JWT token stored in localStorage, or <c>null</c> if not found.</returns>
    public async Task<string> GetToken()
    {
        return await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "jwtToken");
    }

    /// <summary>
    /// Logs the user in by storing the JWT token in localStorage and updating the authentication state.
    /// </summary>
    /// <param name="token">The JWT token to store in localStorage.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task Login(string token)
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "jwtToken", token);
        _isAuthenticated = true;
        AuthenticationStateChanged?.Invoke(true); // Notify subscribers of the authentication state change
    }

    /// <summary>
    /// Logs the user out by removing the JWT token from localStorage and updating the authentication state.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task Logout()
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "jwtToken");
        _isAuthenticated = false;
        AuthenticationStateChanged?.Invoke(false); // Notify subscribers of the authentication state change
    }
}
