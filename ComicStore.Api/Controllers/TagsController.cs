using ComicStore.Api.Data;
using ComicStore.Api.Dtos;
using ComicStore.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ComicStore.Api.Controllers;

/// <summary>Thẻ nhân vật Marvel (Wiki): GET / POST / PUT / DELETE.</summary>
[ApiController]
[Route("api/tags")]
public class TagsController : ControllerBase
{
    private readonly AppDbContext _db;

    public TagsController(AppDbContext db) => _db = db;

    // GET: api/tags?status=APPROVED
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TagResponse>>> GetAll([FromQuery] string? status)
    {
        IQueryable<CharacterTag> query = _db.CharacterTags.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<ComicStatus>(status, true, out var parsed))
            query = query.Where(t => t.Status == parsed);

        var tags = await query.OrderBy(t => t.Name).ToListAsync();
        return Ok(tags.Select(t => t.ToResponse()));
    }

    // GET: api/tags/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<TagResponse>> GetById(int id)
    {
        var tag = await _db.CharacterTags.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
        return tag is null ? NotFound(new { message = $"Không tìm thấy tag id={id}" }) : Ok(tag.ToResponse());
    }

    // POST: api/tags
    [HttpPost]
    public async Task<ActionResult<TagResponse>> Create([FromBody] TagCreateRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { message = "Name là bắt buộc" });

        var name = request.Name.Trim();
        if (await _db.CharacterTags.AnyAsync(t => t.Name == name))
            return Conflict(new { message = $"Tag '{name}' đã tồn tại" });

        var tag = new CharacterTag
        {
            Name = name,
            WikiUrl = request.WikiUrl,
            CreatedById = request.CreatedById,
            Status = ComicStatus.Pending
        };

        _db.CharacterTags.Add(tag);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = tag.Id }, tag.ToResponse());
    }

    // PUT: api/tags/5
    [HttpPut("{id:int}")]
    public async Task<ActionResult<TagResponse>> Update(int id, [FromBody] TagUpdateRequest request)
    {
        var tag = await _db.CharacterTags.FindAsync(id);
        if (tag is null) return NotFound(new { message = $"Không tìm thấy tag id={id}" });

        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { message = "Name là bắt buộc" });

        if (!Enum.TryParse<ComicStatus>(request.Status, true, out var status))
            return BadRequest(new { message = "Status không hợp lệ (Draft|Pending|Approved|Rejected)" });

        var name = request.Name.Trim();
        if (await _db.CharacterTags.AnyAsync(t => t.Name == name && t.Id != id))
            return Conflict(new { message = $"Tag '{name}' đã tồn tại" });

        tag.Name = name;
        tag.WikiUrl = request.WikiUrl;
        tag.Status = status;

        await _db.SaveChangesAsync();
        return Ok(tag.ToResponse());
    }

    // DELETE: api/tags/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var tag = await _db.CharacterTags.FindAsync(id);
        if (tag is null) return NotFound(new { message = $"Không tìm thấy tag id={id}" });

        _db.CharacterTags.Remove(tag);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}