using PlateSecure.Application.DTOs;
using PlateSecure.Domain.Specifications;

namespace PlateSecure.Application.Interfaces;

public interface IUserService
{
    Task<IEnumerable<UserResponse>> GetAllUsers(UserFilter? request);
    Task UpdateUserAsync(string id, UserDto response);
    Task DeleteUserAsync(string id);
}