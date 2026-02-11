using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PentestHub.API.Data.DTOs;
using PentestHub.API.Services;
using System.Security.Claims;

namespace PentestHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetUserById(int id)
    {
        var userDto = await _userService.GetUserByIdAsync(id);
        if (userDto == null)
        {
            return NotFound();
        }
        return Ok(userDto);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var userDto = await _userService.GetUserByIdAsync(userId);
        if (userDto == null)
        {
            return NotFound();
        }
        return Ok(userDto);
    }

    [HttpPost("update-photo")]
    [Authorize]
    public async Task<IActionResult> UpdateProfilePhoto([FromBody] UpdatePhotoDto dto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var success = await _userService.UpdateProfilePhotoAsync(userId, dto.PhotoUrl);
        if (!success)
        {
            return BadRequest(new { message = "Failed to update photo" });
        }
        return Ok(new { message = "Profile photo updated successfully" });
    }

    [HttpPost("delete-photo")]
    [Authorize]
    public async Task<IActionResult> DeleteProfilePhoto()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var success = await _userService.DeleteProfilePhotoAsync(userId);
        if (!success)
        {
            return BadRequest(new { message = "Failed to delete photo" });
        }
        return Ok(new { message = "Profile photo deleted successfully" });
    }

    [HttpPost("update-bio")]
    [Authorize]
    public async Task<IActionResult> UpdateBio([FromBody] UpdateBioDto dto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var success = await _userService.UpdateBioAsync(userId, dto.Bio);
        if (!success)
        {
            return BadRequest(new { message = "Failed to update bio" });
        }
        return Ok(new { message = "Bio updated successfully" });
    }

    [HttpPost("update-name")]
    [Authorize]
    public async Task<IActionResult> UpdateName([FromBody] UpdateNameDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            return BadRequest(new { message = "Name cannot be empty" });
        }

        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var success = await _userService.UpdateNameAsync(userId, dto.Name);
        if (!success)
        {
            return BadRequest(new { message = "Failed to update name" });
        }
        return Ok(new { message = "Name updated successfully" });
    }

    [HttpDelete("delete-account")]
    [Authorize]
    public async Task<IActionResult> DeleteAccount()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        
        var role = User.FindFirstValue(ClaimTypes.Role);
        if (role == "Admin")
        {
             return BadRequest(new { message = "Admin accounts cannot be deleted." });
        }

        var success = await _userService.DeleteAccountAsync(userId);
        if (!success)
        {
            return BadRequest(new { message = "Failed to delete account" });
        }
        return Ok(new { message = "Account deleted successfully" });
    }
}

public class UpdateBioDto
{
    public string Bio { get; set; } = string.Empty;
}

public class UpdatePhotoDto
{
    public string PhotoUrl { get; set; } = string.Empty;
}

public class UpdateNameDto
{
    public string Name { get; set; } = string.Empty;
}
