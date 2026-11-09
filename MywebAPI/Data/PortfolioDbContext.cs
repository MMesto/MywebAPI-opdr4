using Microsoft.EntityFrameworkCore;
using MywebAPI.Data.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
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
}
