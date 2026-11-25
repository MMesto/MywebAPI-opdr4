using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MywebAPI.Auth;
using MywebAPI.Dtos;
using MywebAPI.Services;

namespace MywebAPI.Controllers;

[Route("api/projects")]
[ApiController]
public class ProjectsController : ControllerBase
{
    private readonly ProjectService _service;

    public ProjectsController(ProjectService service)
    {
        _service = service;
    }

    [HttpGet]
    [AllowAnonymous] 
    public ActionResult<List<ProjectDto>> GetAll()
    {
        return _service.GetAll();
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous] 
    public ActionResult<ProjectDto> GetById(int id)
    {
        var project = _service.GetById(id);
        if (project == null)
        {
            return NotFound();
        }
        return project;
    }

    [HttpPost]
    [Authorize(Policy = Policies.KanSchrijven)] 
    public ActionResult<ProjectDto> Create(CreateProjectDto dto)
    {
        var created = _service.Create(dto);
        // 201 + Location-header naar het nieuwe item
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = Policies.KanSchrijven)] 
    public IActionResult Update(int id, UpdateProjectDto dto)
    {
        var gelukt = _service.Update(id, dto);
        if (!gelukt)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = Policies.KanSchrijven)]
    public IActionResult Delete(int id)
    {
        var gelukt = _service.Delete(id);
        if (!gelukt)
        {
            return NotFound();
        }
        return NoContent();
    }
}
