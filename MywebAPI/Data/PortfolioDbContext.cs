using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MywebAPI.Auth;
using MywebAPI.Data.Entities;

namespace MywebAPI.Data;

public class PortfolioDbContext : IdentityDbContext<User>
{
    public PortfolioDbContext(DbContextOptions<PortfolioDbContext> options)
        : base(options)
    {
    }

    // elke DbSet wordt een tabel
    public DbSet<Project> Projects { get; set; }
    public DbSet<Blogpost> Blogposts { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // eerst Identity zijn eigen tabellen laten configureren
        base.OnModelCreating(builder);

        // de drie rollen staan vast in de database (komen mee in de migratie)
        builder.Entity<IdentityRole>().HasData(
            new IdentityRole { Id = "rol-gast",   Name = Rollen.Gast,   NormalizedName = "GAST",   ConcurrencyStamp = "rol-gast" },
            new IdentityRole { Id = "rol-editor", Name = Rollen.Editor, NormalizedName = "EDITOR", ConcurrencyStamp = "rol-editor" },
            new IdentityRole { Id = "rol-admin",  Name = Rollen.Admin,  NormalizedName = "ADMIN",  ConcurrencyStamp = "rol-admin" }
        );
    }
}