using Microsoft.AspNetCore.Mvc;
using PlateSecure.Application.DTOs;
using PlateSecure.Application.Interfaces;
using PlateSecure.Domain.Specifications;

namespace PlateSecure.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController(IUserService userService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> GetUsersList([FromQuery] UserFilter? filter)
    {
        var allowedSortFields = new[] { "Id", "CreateDate", "UpdateDate", "Username", "PasswordHash", "Role" };
        if (filter is not null && !allowedSortFields.Contains(filter.SortBy))
            return BadRequest("Invalid sort field");
            
        var logs = await userService.GetAllUsers(filter);
        return Ok(logs);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateUser([FromQuery] string id, [FromForm] UserDto dto)
    {
        await userService.UpdateUserAsync(id, dto);
        return Ok("User updated");
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteUser([FromQuery] string id)
    {
        await userService.DeleteUserAsync(id);
        return Ok("User deleted");
    }
}