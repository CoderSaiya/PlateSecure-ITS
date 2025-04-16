namespace PlateSecure.Application.DTOs;

public sealed record RegisterRequest(string Username, string Password, string Role);