using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MywebAPI.Auth;
using MywebAPI.Controllers;
using MywebAPI.Data;
using MywebAPI.Data.Entities;
using MywebAPI.Dtos;

namespace MywebAPI.Tests;

public class AuthControllerTests
{
    private const string Wachtwoord = "Test123!";

    private readonly UserManager<User> _userManager;
    private readonly DefaultHttpContext _httpContext;
    private readonly AuthController _controller;

    // xUnit maakt voor ELKE test een nieuw object → elke test krijgt een verse opzet
    public AuthControllerTests()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDataProtection().UseEphemeralDataProtectionProvider();
        services.AddDbContext<PortfolioDbContext>(options =>
            options.UseInMemoryDatabase(Guid.NewGuid().ToString()));
        services.AddIdentity<User, IdentityRole>(options =>
            {
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<PortfolioDbContext>();

        var provider = services.BuildServiceProvider();

        // tabellen + de rollen Gast/Editor/Admin (HasData) aanmaken
        provider.GetRequiredService<PortfolioDbContext>().Database.EnsureCreated();

        // nep-HTTP-omgeving, zodat SignInManager een cookie kan zetten
        _httpContext = new DefaultHttpContext { RequestServices = provider };
        provider.GetRequiredService<IHttpContextAccessor>().HttpContext = _httpContext;

        _userManager = provider.GetRequiredService<UserManager<User>>();
        _controller = new AuthController(_userManager, provider.GetRequiredService<SignInManager<User>>())
        {
            ControllerContext = new ControllerContext { HttpContext = _httpContext }
        };
    }

    // ---------- POST /api/auth/register ----------

    [Fact]
    public async Task Register_GeldigeGegevens_Geeft201EnRolGast()
    {
        // Arrange
        var dto = new RegisterDto { Email = "nieuw@test.nl", Wachtwoord = Wachtwoord };

        // Act
        var result = await _controller.Register(dto);

        // Assert – antwoord
        var created = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(201, created.StatusCode);
        var gebruiker = Assert.IsType<GebruikerDto>(created.Value);
        Assert.Equal(new[] { Rollen.Gast }, gebruiker.Rollen);

        // Assert – effect: gebruiker staat in de database, met alleen Gast, en een gehasht wachtwoord
        var user = await _userManager.FindByEmailAsync("nieuw@test.nl");
        Assert.NotNull(user);
        Assert.Equal(new[] { Rollen.Gast }, await _userManager.GetRolesAsync(user!));
        Assert.NotEqual(Wachtwoord, user!.PasswordHash);
    }

    [Fact]
    public void RegisterDto_HeeftGeenRolVeld()
    {
        // Arrange
        var velden = typeof(RegisterDto).GetProperties();

        // Act
        var rolVelden = velden.Where(v => v.Name.Contains("Rol"));

        // Assert – de client kán dus geen rol meesturen
        Assert.Empty(rolVelden);
    }

    [Fact]
    public async Task Register_ZwakWachtwoord_Geeft400EnMaaktGeenGebruiker()
    {
        var dto = new RegisterDto { Email = "zwak@test.nl", Wachtwoord = "abc" };

        var result = await _controller.Register(dto);

        Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Null(await _userManager.FindByEmailAsync("zwak@test.nl"));
    }

    [Fact]
    public async Task Register_BestaandEmail_Geeft400()
    {
        var dto = new RegisterDto { Email = "dubbel@test.nl", Wachtwoord = Wachtwoord };
        await _controller.Register(dto);

        var result = await _controller.Register(dto);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    // ---------- POST /api/auth/login ----------

    [Fact]
    public async Task Login_JuisteGegevens_GeeftRollenEnCookie()
    {
        // Arrange – een Editor, net als lokaal met de seeder
        var editor = new User { UserName = "editor@test.nl", Email = "editor@test.nl" };
        await _userManager.CreateAsync(editor, Wachtwoord);
        await _userManager.AddToRoleAsync(editor, Rollen.Editor);

        // Act
        var result = await _controller.Login(new LoginDto { Email = "editor@test.nl", Wachtwoord = Wachtwoord });

        // Assert – antwoord
        var gebruiker = result.Value!;
        Assert.Contains(Rollen.Editor, gebruiker.Rollen);

        // Assert – effect: er staat een authenticatiecookie in de response
        var cookie = _httpContext.Response.Headers["Set-Cookie"].ToString();
        Assert.Contains(".AspNetCore.Identity.Application", cookie);
    }

    [Fact]
    public async Task Login_FoutWachtwoord_Geeft401ZonderCookie()
    {
        await _controller.Register(new RegisterDto { Email = "gast@test.nl", Wachtwoord = Wachtwoord });

        var result = await _controller.Login(new LoginDto { Email = "gast@test.nl", Wachtwoord = "Fout123!" });

        Assert.IsType<UnauthorizedResult>(result.Result);
        Assert.Empty(_httpContext.Response.Headers["Set-Cookie"].ToString());
    }

    // ---------- POST /api/auth/logout ----------

    [Fact]
    public async Task Logout_Geeft204()
    {
        await _controller.Register(new RegisterDto { Email = "uit@test.nl", Wachtwoord = Wachtwoord });
        await _controller.Login(new LoginDto { Email = "uit@test.nl", Wachtwoord = Wachtwoord });

        var result = await _controller.Logout();

        Assert.IsType<NoContentResult>(result);
    }
}