using Microsoft.AspNetCore.Identity;
using MywebAPI.Auth;
using MywebAPI.Data.Entities;

namespace MywebAPI.Data;

public static class GebruikerSeeder
{
    public static async Task SeedAsync(IServiceProvider services, IConfiguration config)
    {
        using var scope = services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

        foreach (var rol in new[] { Rollen.Editor, Rollen.Admin })
        {
            var email = config[$"SeedUsers:{rol}:Email"];
            var wachtwoord = config[$"SeedUsers:{rol}:Wachtwoord"];

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(wachtwoord))
            {
                continue;
            }

            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new User { UserName = email, Email = email };
                var result = await userManager.CreateAsync(user, wachtwoord);
                if (!result.Succeeded)
                {
                    var fouten = string.Join(", ", result.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Seed-gebruiker {email} niet aangemaakt: {fouten}");
                }
            }

            if (!await userManager.IsInRoleAsync(user, rol))
            {
                await userManager.AddToRoleAsync(user, rol);
            }
        }
    }
}