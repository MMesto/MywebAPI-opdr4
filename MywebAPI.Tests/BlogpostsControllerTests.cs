using Microsoft.AspNetCore.Mvc;
using MywebAPI.Controllers;
using MywebAPI.Dtos;
using MywebAPI.Services;

namespace MywebAPI.Tests;

public class BlogpostsControllerTests
{
    // ---------- GET /api/blogposts ----------

    [Fact]
    public void GetAll_MetTweeBlogposts_GeeftBeideTerug()
    {
        var db = TestHelper.MaakDatabase();
        TestHelper.VoegBlogpostToe(db, "Blog A");
        TestHelper.VoegBlogpostToe(db, "Blog B");
        var controller = new BlogpostsController(new BlogpostService(db));

        var result = controller.GetAll();

        var blogposts = result.Value!;
        Assert.Equal(2, blogposts.Count);
        Assert.Contains(blogposts, b => b.Titel == "Blog A");
        Assert.Contains(blogposts, b => b.Titel == "Blog B");
    }

    [Fact]
    public void GetAll_LegeDatabase_GeeftLegeLijst()
    {
        var db = TestHelper.MaakDatabase();
        var controller = new BlogpostsController(new BlogpostService(db));

        var result = controller.GetAll();

        Assert.Empty(result.Value!);
    }

    // ---------- GET /api/blogposts/{id} ----------

    [Fact]
    public void GetById_BestaandId_GeeftJuisteBlogpost()
    {
        var db = TestHelper.MaakDatabase();
        var bestaand = TestHelper.VoegBlogpostToe(db, "Mijn blog");
        var controller = new BlogpostsController(new BlogpostService(db));

        var result = controller.GetById(bestaand.Id);

        var blogpost = result.Value!;
        Assert.Equal(bestaand.Id, blogpost.Id);
        Assert.Equal("Mijn blog", blogpost.Titel);
        Assert.Equal("Inhoud van de blogpost", blogpost.Inhoud);
    }

    [Fact]
    public void GetById_OnbekendId_GeeftNotFound()
    {
        var db = TestHelper.MaakDatabase();
        var controller = new BlogpostsController(new BlogpostService(db));

        var result = controller.GetById(999);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    // ---------- POST /api/blogposts ----------

    [Fact]
    public void Create_GeldigeBlogpost_Geeft201EnSlaatOp()
    {
        var db = TestHelper.MaakDatabase();
        var controller = new BlogpostsController(new BlogpostService(db));
        var dto = TestHelper.NieuweBlogpost("Eerste blog");

        var result = controller.Create(dto);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(201, created.StatusCode);
        Assert.Equal(nameof(BlogpostsController.GetById), created.ActionName);
        var nieuw = Assert.IsType<BlogpostDto>(created.Value);
        Assert.True(nieuw.Id > 0);
        Assert.Equal("Eerste blog", nieuw.Titel);

        var inDatabase = db.Blogposts.Find(nieuw.Id);
        Assert.NotNull(inDatabase);
        Assert.Equal("Inhoud van de nieuwe blogpost", inDatabase!.Inhoud);
    }

    // ---------- PUT /api/blogposts/{id} ----------

    [Fact]
    public void Update_BestaandId_Geeft204EnWijzigtData()
    {
        var db = TestHelper.MaakDatabase();
        var bestaand = TestHelper.VoegBlogpostToe(db, "Oude titel");
        var controller = new BlogpostsController(new BlogpostService(db));
        var dto = TestHelper.GewijzigdeBlogpost("Nieuwe titel");

        var result = controller.Update(bestaand.Id, dto);

        Assert.IsType<NoContentResult>(result);
        var inDatabase = db.Blogposts.Find(bestaand.Id)!;
        Assert.Equal("Nieuwe titel", inDatabase.Titel);
        Assert.Equal("Aangepaste inhoud", inDatabase.Inhoud);
    }

    [Fact]
    public void Update_OnbekendId_GeeftNotFoundEnVeranderdNiets()
    {
        var db = TestHelper.MaakDatabase();
        TestHelper.VoegBlogpostToe(db, "Blijft zo");
        var controller = new BlogpostsController(new BlogpostService(db));

        var result = controller.Update(999, TestHelper.GewijzigdeBlogpost());

        Assert.IsType<NotFoundResult>(result);
        Assert.Equal("Blijft zo", db.Blogposts.Single().Titel);
    }

    // ---------- DELETE /api/blogposts/{id} ----------

    [Fact]
    public void Delete_BestaandId_Geeft204EnVerwijdert()
    {
        var db = TestHelper.MaakDatabase();
        var bestaand = TestHelper.VoegBlogpostToe(db);
        var controller = new BlogpostsController(new BlogpostService(db));

        var result = controller.Delete(bestaand.Id);

        Assert.IsType<NoContentResult>(result);
        Assert.Null(db.Blogposts.Find(bestaand.Id));
    }

    [Fact]
    public void Delete_OnbekendId_GeeftNotFoundEnVerwijdertNiets()
    {
        var db = TestHelper.MaakDatabase();
        TestHelper.VoegBlogpostToe(db);
        var controller = new BlogpostsController(new BlogpostService(db));

        var result = controller.Delete(999);

        Assert.IsType<NotFoundResult>(result);
        Assert.Equal(1, db.Blogposts.Count());
    }
}