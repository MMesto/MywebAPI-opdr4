using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using MywebAPI.Auth;
using MywebAPI.Controllers;

namespace MywebAPI.Tests;

public class AutorisatieTests
{
    // ---------- 1. Staan de attributen op de juiste routes? ----------

    [Theory]
    [InlineData(typeof(ProjectsController), nameof(ProjectsController.Create))]
    [InlineData(typeof(ProjectsController), nameof(ProjectsController.Update))]
    [InlineData(typeof(ProjectsController), nameof(ProjectsController.Delete))]
    [InlineData(typeof(BlogpostsController), nameof(BlogpostsController.Create))]
    [InlineData(typeof(BlogpostsController), nameof(BlogpostsController.Update))]
    [InlineData(typeof(BlogpostsController), nameof(BlogpostsController.Delete))]
    public void SchrijfRoute_VereistPolicyKanSchrijven(Type controller, string methode)
    {
        var method = controller.GetMethod(methode)!;

        var attribuut = method.GetCustomAttribute<AuthorizeAttribute>();

        Assert.NotNull(attribuut);
        Assert.Equal(Policies.KanSchrijven, attribuut!.Policy);
    }

    [Theory]
    [InlineData(typeof(ProjectsController), nameof(ProjectsController.GetAll))]
    [InlineData(typeof(ProjectsController), nameof(ProjectsController.GetById))]
    [InlineData(typeof(BlogpostsController), nameof(BlogpostsController.GetAll))]
    [InlineData(typeof(BlogpostsController), nameof(BlogpostsController.GetById))]
    public void LeesRoute_IsPubliek(Type controller, string methode)
    {
        var method = controller.GetMethod(methode)!;

        var publiek = method.GetCustomAttribute<AllowAnonymousAttribute>();
        var beveiligd = method.GetCustomAttribute<AuthorizeAttribute>();

        Assert.NotNull(publiek);
        Assert.Null(beveiligd);
    }

    // ---------- 2. Laat de policy de juiste rollen toe? ----------

    // een mini-DI-container met alleen de echte policy uit Rollen.cs
    private static IAuthorizationService MaakAutorisatieService()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAuthorizationBuilder()
            .AddPolicy(Policies.KanSchrijven, Policies.KanSchrijvenRegel);

        return services.BuildServiceProvider().GetRequiredService<IAuthorizationService>();
    }

    // een nep-gebruiker die is ingelogd met één rol
    private static ClaimsPrincipal IngelogdeGebruiker(string rol)
    {
        var claims = new[] { new Claim(ClaimTypes.Role, rol) };
        var identity = new ClaimsIdentity(claims, authenticationType: "Test");
        return new ClaimsPrincipal(identity);
    }

    [Theory]
    [InlineData(Rollen.Editor, true)]
    [InlineData(Rollen.Admin, true)]
    [InlineData(Rollen.Gast, false)]
    public async Task KanSchrijven_PerRol_GeeftJuisteUitkomst(string rol, bool verwacht)
    {
        var autorisatie = MaakAutorisatieService();
        var gebruiker = IngelogdeGebruiker(rol);

        var resultaat = await autorisatie.AuthorizeAsync(gebruiker, Policies.KanSchrijven);

        Assert.Equal(verwacht, resultaat.Succeeded);
    }

    [Fact]
    public async Task KanSchrijven_NietIngelogd_WordtGeweigerd()
    {
        var autorisatie = MaakAutorisatieService();
        var anoniem = new ClaimsPrincipal(new ClaimsIdentity()); // geen claims, niet ingelogd

        var resultaat = await autorisatie.AuthorizeAsync(anoniem, Policies.KanSchrijven);

        Assert.False(resultaat.Succeeded);
        Assert.False(anoniem.Identity!.IsAuthenticated); // → middleware maakt hier 401 van
    }
}