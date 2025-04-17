namespace PlateSecure.Application.DTOs;

public sealed record UserResponse(
    string Id,
    string Username,
    string Password,
    string Role
    );