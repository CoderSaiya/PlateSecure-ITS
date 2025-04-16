using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PlateSecure.Application.Interfaces;
using PlateSecure.Domain.Documents;
using PlateSecure.Domain.Interfaces;

namespace PlateSecure.Infrastructure.Identity;

public class AuthService(
    IConfiguration configuration,
    IUserRepository userRepository
    ) : IAuthService
{
    public async Task<string> LoginAsync(string username, string password)
    {
        var user = await userRepository.GetUserByUsernameAsync(username);
        if (user is null) 
            throw new ApplicationException("Invalid username");
        
        if (VerifyPassword(password, user.PasswordHash))
            throw new ApplicationException("Invalid password");

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Role, user.Role)
        };

        return GenerateAccessToken(claims);
    }

    public async Task RegisterAsync(string username, string password, string role)
    {
        var user = await userRepository.GetUserByUsernameAsync(username);
        if (user is not null)
            throw new ApplicationException("User already exists");
        
        var passwordHash = HashPassword(password);
        var created = new User
        {
            Username = username,
            PasswordHash = passwordHash,
            Role = role
        };
        
        await userRepository.AddUserAsync(created);
    }
    
    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }
    
    private bool VerifyPassword(string password, string hashedPassword)
    {
        return HashPassword(password) == hashedPassword;
    }
    
    private string GenerateAccessToken(IEnumerable<Claim> claims)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: configuration["JWT:Issuer"],
            audience: configuration["JWT:Audience"],
            claims: claims,
            expires: DateTime.Now.AddDays(10),
            signingCredentials: creds);
        
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}