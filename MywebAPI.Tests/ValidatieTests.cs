using System.ComponentModel.DataAnnotations;
using MywebAPI.Dtos;

namespace MywebAPI.Tests;

public class ValidatieTests
{
    // voert dezelfde regels uit als [ApiController] doet bij een echt verzoek
    private static List<ValidationResult> Valideer(object dto)
    {
        var fouten = new List<ValidationResult>();
        Validator.TryValidateObject(dto, new ValidationContext(dto), fouten, validateAllProperties: true);
        return fouten;
    }

    // ---------- Projecten ----------

    [Fact]
    public void Project_GeldigeInvoer_GeeftGeenFouten()
    {
        var dto = TestHelper.NieuwProject();

        var fouten = Valideer(dto);

        Assert.Empty(fouten);
    }

    [Fact]
    public void Project_LegeTitel_GeeftFoutOpTitel()
    {
        var dto = TestHelper.NieuwProject("");

        var fouten = Valideer(dto);

        Assert.Contains(fouten, f => f.MemberNames.Contains("Titel"));
    }

    [Fact]
    public void Project_TitelLangerDan100Tekens_GeeftFoutOpTitel()
    {
        var dto = TestHelper.NieuwProject(new string('a', 101));

        var fouten = Valideer(dto);

        Assert.Contains(fouten, f => f.MemberNames.Contains("Titel"));
    }

    [Theory]
    [InlineData("geen-url")]
    [InlineData("www.github.com")]
    public void Project_OngeldigeGitHubUrl_GeeftFoutOpGitHubUrl(string url)
    {
        var dto = TestHelper.NieuwProject();
        dto.GitHubUrl = url;

        var fouten = Valideer(dto);

        Assert.Contains(fouten, f => f.MemberNames.Contains("GitHubUrl"));
    }

    [Fact]
    public void ProjectUpdate_LegeCategorie_GeeftFoutOpCategorie()
    {
        var dto = TestHelper.GewijzigdProject();
        dto.Categorie = "";

        var fouten = Valideer(dto);

        Assert.Contains(fouten, f => f.MemberNames.Contains("Categorie"));
    }

    // ---------- Blogposts ----------

    [Fact]
    public void Blogpost_GeldigeInvoer_GeeftGeenFouten()
    {
        var dto = TestHelper.NieuweBlogpost();

        var fouten = Valideer(dto);

        Assert.Empty(fouten);
    }

    [Fact]
    public void Blogpost_LegeInhoud_GeeftFoutOpInhoud()
    {
        var dto = TestHelper.NieuweBlogpost();
        dto.Inhoud = "";

        var fouten = Valideer(dto);

        Assert.Contains(fouten, f => f.MemberNames.Contains("Inhoud"));
    }

    [Fact]
    public void Blogpost_TitelLangerDan150Tekens_GeeftFoutOpTitel()
    {
        var dto = TestHelper.NieuweBlogpost(new string('a', 151));

        var fouten = Valideer(dto);

        Assert.Contains(fouten, f => f.MemberNames.Contains("Titel"));
    }

    // ---------- Registratie ----------

    [Theory]
    [InlineData("geen-email")]
    [InlineData("test@")]
    public void Register_OngeldigEmail_GeeftFoutOpEmail(string email)
    {
        var dto = new RegisterDto { Email = email, Wachtwoord = "Test123!" };

        var fouten = Valideer(dto);

        Assert.Contains(fouten, f => f.MemberNames.Contains("Email"));
    }

    [Fact]
    public void Register_LeegWachtwoord_GeeftFoutOpWachtwoord()
    {
        var dto = new RegisterDto { Email = "test@test.nl", Wachtwoord = "" };

        var fouten = Valideer(dto);

        Assert.Contains(fouten, f => f.MemberNames.Contains("Wachtwoord"));
    }
}