using Microsoft.AspNetCore.Mvc;
using MywebAPI.Controllers;
using MywebAPI.Dtos;
using MywebAPI.Services;
using Xunit;

namespace MywebAPI.Tests;

public class ProjectsControllerTests
{
    // ---------- GET /api/projects ----------

    [Fact]
    public void GetAll_MetTweeProjecten_GeeftBeideTerug()
    {
        var db = TestHelper.MaakDatabase();
        TestHelper.VoegProjectToe(db, "Project A");
        TestHelper.VoegProjectToe(db, "Project B");
        var controller = new ProjectsController(new ProjectService(db));

        var result = controller.GetAll();

        var projecten = result.Value!;
        Assert.Equal(2, projecten.Count);
        Assert.Contains(projecten, p => p.Titel == "Project A");
        Assert.Contains(projecten, p => p.Titel == "Project B");
    }

    [Fact]
    public void GetAll_LegeDatabase_GeeftLegeLijst()
    {
        var db = TestHelper.MaakDatabase();
        var controller = new ProjectsController(new ProjectService(db));

        var result = controller.GetAll();

        Assert.Empty(result.Value!);
    }

    // ---------- GET /api/projects/{id} ----------

    [Fact]
    public void GetById_BestaandId_GeeftJuisteProject()
    {
        var db = TestHelper.MaakDatabase();
        var bestaand = TestHelper.VoegProjectToe(db, "Mijn project");
        var controller = new ProjectsController(new ProjectService(db));

        var result = controller.GetById(bestaand.Id);

        var project = result.Value!;
        Assert.Equal(bestaand.Id, project.Id);
        Assert.Equal("Mijn project", project.Titel);
        Assert.Equal("Web", project.Categorie);
    }

    [Fact]
    public void GetById_OnbekendId_GeeftNotFound()
    {
        var db = TestHelper.MaakDatabase();
        var controller = new ProjectsController(new ProjectService(db));

        var result = controller.GetById(999);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    // ---------- POST /api/projects ----------

    [Fact]
    public void Create_GeldigProject_Geeft201EnSlaatOp()
    {
        // Arrange
        var db = TestHelper.MaakDatabase();
        var controller = new ProjectsController(new ProjectService(db));
        var dto = TestHelper.NieuwProject("Nieuw portfolio");

        // Act
        var result = controller.Create(dto);

        // Assert – antwoord
        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(201, created.StatusCode);
        Assert.Equal(nameof(ProjectsController.GetById), created.ActionName);
        var nieuw = Assert.IsType<ProjectDto>(created.Value);
        Assert.True(nieuw.Id > 0);
        Assert.Equal("Nieuw portfolio", nieuw.Titel);

        // Assert – effect: het staat echt in de database
        var inDatabase = db.Projects.Find(nieuw.Id);
        Assert.NotNull(inDatabase);
        Assert.Equal("Nieuw portfolio", inDatabase!.Titel);
    }

    // ---------- PUT /api/projects/{id} ----------

    [Fact]
    public void Update_BestaandId_Geeft204EnWijzigtData()
    {
        var db = TestHelper.MaakDatabase();
        var bestaand = TestHelper.VoegProjectToe(db, "Oude titel");
        var controller = new ProjectsController(new ProjectService(db));
        var dto = TestHelper.GewijzigdProject("Nieuwe titel");

        var result = controller.Update(bestaand.Id, dto);

        Assert.IsType<NoContentResult>(result);
        var inDatabase = db.Projects.Find(bestaand.Id)!;
        Assert.Equal("Nieuwe titel", inDatabase.Titel);
        Assert.Equal("Backend", inDatabase.Categorie);
    }

    [Fact]
    public void Update_OnbekendId_GeeftNotFoundEnVeranderdNiets()
    {
        var db = TestHelper.MaakDatabase();
        TestHelper.VoegProjectToe(db, "Blijft zo");
        var controller = new ProjectsController(new ProjectService(db));

        var result = controller.Update(999, TestHelper.GewijzigdProject());

        Assert.IsType<NotFoundResult>(result);
        Assert.Equal("Blijft zo", db.Projects.Single().Titel);
    }

    // ---------- DELETE /api/projects/{id} ----------

    [Fact]
    public void Delete_BestaandId_Geeft204EnVerwijdert()
    {
        var db = TestHelper.MaakDatabase();
        var bestaand = TestHelper.VoegProjectToe(db);
        var controller = new ProjectsController(new ProjectService(db));

        var result = controller.Delete(bestaand.Id);

        Assert.IsType<NoContentResult>(result);
        Assert.Null(db.Projects.Find(bestaand.Id));
    }

    [Fact]
    public void Delete_OnbekendId_GeeftNotFoundEnVerwijdertNiets()
    {
        var db = TestHelper.MaakDatabase();
        TestHelper.VoegProjectToe(db);
        var controller = new ProjectsController(new ProjectService(db));

        var result = controller.Delete(999);

        Assert.IsType<NotFoundResult>(result);
        Assert.Equal(1, db.Projects.Count());
    }
}