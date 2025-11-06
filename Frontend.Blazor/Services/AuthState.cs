using System;

namespace Frontend.Blazor.Services;

public class AuthState
{
    public bool IsLoggedIn { get; private set; }

    public event Action? OnChange;

    public void SetLoginState(bool loggedIn)
    {
        IsLoggedIn = loggedIn;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
