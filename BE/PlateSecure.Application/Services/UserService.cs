using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using PlateSecure.Application.DTOs;
using PlateSecure.Application.Interfaces;
using PlateSecure.Domain.Interfaces;
using PlateSecure.Domain.Specifications;

namespace PlateSecure.Application.Services;

public class UserService(
    IUserRepository userRepository,
    ILogger<UserService> logger
    ) : IUserService
{
    public async Task<IEnumerable<UserResponse>> GetAllUsers(UserFilter? request)
    {
        var eventResponse = new List<UserResponse>();
        var filter = request ?? new UserFilter();

        if (filter.Password is not null)
            filter.Password = HashPassword(filter.Password);
        
        var users = await userRepository.GetUsersAsync(filter);
        foreach (var user in users)
        {
            var ids = user.Id.ToString();
            eventResponse.Add(new UserResponse(
                ids,
                user.Username,
                user.PasswordHash,
                user.Role
                )
            );
        }

        return eventResponse;
    }

    public async Task UpdateUserAsync(string id, UserDto request)
    {
        if (!ObjectId.TryParse(id, out var objId))
            throw new ArgumentException("ID không hợp lệ");

        var existingUser = await userRepository.GetByIdAsync(objId);
        if (existingUser is null)
            throw new KeyNotFoundException("Không tìm thấy người dùng");
        
        existingUser.Username = request.Username ?? existingUser.Username;
        existingUser.PasswordHash = request.Password is not null ? HashPassword(request.Password) : existingUser.PasswordHash;
        existingUser.Role = request.Role ?? existingUser.Role;

        await userRepository.UpdateAsync(existingUser);
        logger.LogInformation("Cập nhật người dùng #{UserId}", id);
    }

    public async Task DeleteUserAsync(string id)
    {
        if (!ObjectId.TryParse(id, out var objId))
            throw new ArgumentException("ID không hợp lệ");

        var existingEvent = await userRepository.GetByIdAsync(objId);
        if (existingEvent == null)
            throw new KeyNotFoundException("Không tìm thấy người dùng");

        await userRepository.DeleteAsync(objId);
        logger.LogInformation("Đã xóa người dùng #{UserId}", id);
    }

    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }
}