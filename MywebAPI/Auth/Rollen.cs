using Microsoft.AspNetCore.Authorization;

namespace MywebAPI.Auth;

// Vaste namen: een typfout wordt zo een compileerfout in plaats van een bug
public static class Rollen
{
    public const string Gast = "Gast";
    public const string Editor = "Editor";
    public const string Admin = "Admin";
}

public static class Policies
{
    // Editor óf Admin mag schrijven (POST, PUT, DELETE)
    public const string KanSchrijven = "KanSchrijven";

    // de regel zelf, op één plek: gebruikt door Program.cs én door de tests
    public static void KanSchrijvenRegel(AuthorizationPolicyBuilder policy)
    {
        policy.RequireRole(Rollen.Editor, Rollen.Admin);
    }
}