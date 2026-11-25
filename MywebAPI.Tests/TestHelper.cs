using Microsoft.EntityFrameworkCore;
using MywebAPI.Data;
using MywebAPI.Data.Entities;
using MywebAPI.Dtos;

namespace MywebAPI.Tests;

public static class TestHelper
{
    // elke aanroep = een nieuwe, lege database met een unieke naam
    public static PortfolioDbContext MaakDatabase()
    {
        var options = new DbContextOptionsBuilder<PortfolioDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new PortfolioDbContext(options);
    }

    // zet direct een project in de database (voor de Arrange-stap)
    public static Project VoegProjectToe(PortfolioDbContext db, string titel = "Bestaand project")
    {
        var project = new Project
        {
            Titel = titel,
            Beschrijving = "Beschrijving van het project",
            Categorie = "Web",
            GitHubUrl = "https://github.com/MMesto/test",
            Datum = new DateTime(2026, 11, 1)
        };

        db.Projects.Add(project);
        db.SaveChanges();
        return project;
    }

    // geldige invoer voor POST
    public static CreateProjectDto NieuwProject(string titel = "Nieuw project") => new()
    {
        Titel = titel,
        Beschrijving = "Een nieuw project",
        Categorie = "Web",
        GitHubUrl = "https://github.com/MMesto/nieuw",
        Datum = new DateTime(2026, 11, 1)
    };

    // geldige invoer voor PUT
    public static UpdateProjectDto GewijzigdProject(string titel = "Gewijzigd project") => new()
    {
        Titel = titel,
        Beschrijving = "Aangepaste beschrijving",
        Categorie = "Backend",
        GitHubUrl = "https://github.com/MMesto/gewijzigd",
        Datum = new DateTime(2026, 11, 15)
    };
}