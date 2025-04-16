using Microsoft.AspNetCore.Mvc;
using PlateSecure.Application.DTOs;
using PlateSecure.Application.Interfaces;
using LoginRequest = PlateSecure.Application.DTOs.LoginRequest;

namespace PlateSecure.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(IAuthService authService) : Controller
    {
        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginRequest loginRequest)
        {
            var token = await authService.LoginAsync(loginRequest.Username, loginRequest.Password);
            return Ok(new { Token = token });
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterRequest registerRequest)
        {
            var validRoles = new[] { "staff", "admin" };
            if (!validRoles.Contains(registerRequest.Role))
                return BadRequest("Invalid role value. Allowed values: staff, admin.");
            
            await authService.RegisterAsync(registerRequest.Username, registerRequest.Password, registerRequest.Role);
            return Ok("Registration successful!");
        }
    }
}