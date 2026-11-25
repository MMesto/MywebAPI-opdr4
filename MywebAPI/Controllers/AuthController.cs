using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MywebAPI.Auth;
using MywebAPI.Data.Entities;
using MywebAPI.Dtos;

namespace MywebAPI.Controllers;

[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;

    public AuthController(UserManager<User> userManager, SignInManager<User> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [HttpPost("register")]
    public async Task<ActionResult<GebruikerDto>> Register(RegisterDto dto)
    {
        var user = new User { UserName = dto.Email, Email = dto.Email };

        var result = await _userManager.CreateAsync(user, dto.Wachtwoord);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(error.Code, error.Description);
            }
            return ValidationProblem(ModelState);
        }

        await _userManager.AddToRoleAsync(user, Rollen.Gast);

        var gebruiker = new GebruikerDto { Email = user.Email!, Rollen = [Rollen.Gast] };
        return StatusCode(StatusCodes.Status201Created, gebruiker);
    }

    [HttpPost("login")]
    public async Task<ActionResult<GebruikerDto>> Login(LoginDto dto)
    {
        var result = await _signInManager.PasswordSignInAsync(
            dto.Email, dto.Wachtwoord, isPersistent: false, lockoutOnFailure: false);

        if (!result.Succeeded)
        {
            return Unauthorized();
        }

        var user = await _userManager.FindByEmailAsync(dto.Email);
        var rollen = await _userManager.GetRolesAsync(user!);
        return new GebruikerDto { Email = user!.Email!, Rollen = rollen.ToList() };
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return NoContent();
    }
}