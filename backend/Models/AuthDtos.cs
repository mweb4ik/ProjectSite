namespace PcComponentsApi.Models;


public class RegisterRequest
{   public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}
public class ForgotPasswordRequest
{
    public string Email { get; set; } = string.Empty;
}

public class ResetPasswordRequest
{
    public string Token { get; set; }  = string.Empty;
    public string NewPassword { get; set; }  = string.Empty;
}
public class UpdateUserRequest
{
    public string? Username { get; set; }
    public string? Email { get; set; }
}
