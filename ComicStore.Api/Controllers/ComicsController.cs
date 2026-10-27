using ComicStore.Api.Data;
using ComicStore.Api.Dtos;
using ComicStore.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ComicStore.Api.Controllers;

/// <summary>CRUD truyện tranh: GET / POST / PUT / DELETE.</summary>
[ApiController]
[Route("api/comics")]
public class ComicsController : ControllerBase
{
    private readonly AppDbContext _db;

    public ComicsController(AppDbContext db) => _db = db;

    // GET: api/comics?status=Approved&search=iron
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ComicResponse>>> GetAll(
        [FromQuery] string? status,
        [FromQuery] string? search,
        [FromQuery] int? createdById)
    {
        IQueryable<Comic> query = _db.Comics
            .Include(c => c.Characters)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(status) &&
            Enum.TryParse<ComicStatus>(status, true, out var parsedStatus))
        {
            query = query.Where(c => c.Status == parsedStatus);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var keyword = search.Trim().ToLower();
            query = query.Where(c =>
                c.Title.ToLower().Contains(keyword) ||
                (c.Issue != null && c.Issue.ToLower().Contains(keyword)) ||
                (c.Description != null && c.Description.ToLower().Contains(keyword)));
        }

        if (createdById.HasValue)
        {
            query = query.Where(c => c.CreatedById == createdById);
        }

        var comics = await query.OrderByDescending(c => c.CreatedAt).ToListAsync();
        return Ok(comics.Select(c => c.ToResponse()));
    }

    // GET: api/comics/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ComicResponse>> GetById(int id)
    {
        var comic = await _db.Comics
            .AsNoTracking()
            .Include(c => c.Characters)
            .FirstOrDefaultAsync(c => c.Id == id);

        return comic is null ? NotFound(new { message = $"Không tìm thấy truyện id={id}" }) : Ok(comic.ToResponse());
    }

    // POST: api/comics
    [HttpPost]
    public async Task<ActionResult<ComicResponse>> Create([FromBody] ComicCreateRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        if (string.IsNullOrWhiteSpace(request.Title))
            return BadRequest(new { message = "Title là bắt buộc" });
        if (request.Price < 0) return BadRequest(new { message = "Price không được âm" });

        var comic = new Comic
        {
            Title = request.Title.Trim(),
            Issue = request.Issue,
            Price = request.Price,
            OldPrice = request.OldPrice,
            Description = request.Description,
            Preview = request.Preview,
            ImageUrl = request.ImageUrl,
            Color = request.Color,
            Accent = request.Accent,
            Mark = request.Mark,
            Badge = request.Badge,
            Stock = request.Stock,
            PublishYear = request.PublishYear,
            CreatedById = request.CreatedById,
            Status = ComicStatus.Pending
        };

        AttachCharacters(comic, request.Characters);

        _db.Comics.Add(comic);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = comic.Id }, comic.ToResponse());
    }

    // PUT: api/comics/5
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ComicResponse>> Update(int id, [FromBody] ComicUpdateRequest request)
    {
        var comic = await _db.Comics
            .Include(c => c.Characters)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (comic is null) return NotFound(new { message = $"Không tìm thấy truyện id={id}" });
        if (string.IsNullOrWhiteSpace(request.Title)) return BadRequest(new { message = "Title là bắt buộc" });
        if (request.Price < 0) return BadRequest(new { message = "Price không được âm" });
        if (!Enum.TryParse<ComicStatus>(request.Status, true, out var status))
            return BadRequest(new { message = "Status không hợp lệ (Draft|Pending|Approved|Rejected)" });

        comic.Title = request.Title.Trim();
        comic.Issue = request.Issue;
        comic.Price = request.Price;
        comic.OldPrice = request.OldPrice;
        comic.Description = request.Description;
        comic.Preview = request.Preview;
        comic.ImageUrl = request.ImageUrl;
        comic.Color = request.Color;
        comic.Accent = request.Accent;
        comic.Mark = request.Mark;
        comic.Badge = request.Badge;
        comic.Stock = request.Stock;
        comic.PublishYear = request.PublishYear;
        comic.Status = status;
        comic.RejectReason = request.RejectReason;
        comic.UpdatedAt = DateTime.UtcNow;

        // Cập nhật lại danh sách nhân vật (xóa cũ, thêm mới).
        var oldCharacters = await _db.ComicCharacters
            .Where(cc => cc.ComicId == id)
            .ToListAsync();
        _db.ComicCharacters.RemoveRange(oldCharacters);
        AttachCharacters(comic, request.Characters);

        await _db.SaveChangesAsync();
        return Ok(comic.ToResponse());
    }

    // DELETE: api/comics/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var comic = await _db.Comics.FindAsync(id);
        if (comic is null) return NotFound(new { message = $"Không tìm thấy truyện id={id}" });

        var inCart = await _db.CartItems.AnyAsync(ci => ci.ComicId == id);
        var inOrder = await _db.OrderItems.AnyAsync(oi => oi.ComicId == id);
        if (inCart || inOrder)
            return Conflict(new { message = "Truyện đã tồn tại trong giỏ hàng/đơn hàng, không thể xóa" });

        var characters = await _db.ComicCharacters.Where(cc => cc.ComicId == id).ToListAsync();
        _db.ComicCharacters.RemoveRange(characters);

        _db.Comics.Remove(comic);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    private static void AttachCharacters(Comic comic, List<string>? characters)
    {
        if (characters is null) return;

        foreach (var name in characters.Where(c => !string.IsNullOrWhiteSpace(c)).Select(c => c.Trim()).Distinct())
        {
            comic.Characters.Add(new ComicCharacter { CharacterName = name });
        }
    }
}