using ComicStore.Api.Data;
using ComicStore.Api.Dtos;
using ComicStore.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ComicStore.Api.Controllers;

/// <summary>Giỏ hàng: GET / POST / PUT / DELETE.</summary>
[ApiController]
[Route("api/cart")]
public class CartController : ControllerBase
{
    private readonly AppDbContext _db;

    public CartController(AppDbContext db) => _db = db;

    // GET: api/cart?userId=1
    [HttpGet]
    public async Task<ActionResult<object>> GetByUser([FromQuery] int userId)
    {
        if (userId <= 0) return BadRequest(new { message = "userId là bắt buộc" });

        var items = await _db.CartItems
            .AsNoTracking()
            .Include(ci => ci.Comic)
            .Where(ci => ci.UserId == userId)
            .OrderBy(ci => ci.Id)
            .ToListAsync();

        var responses = items.Select(ci => ci.ToResponse()).ToList();
        return Ok(new
        {
            userId,
            items = responses,
            totalQuantity = responses.Sum(r => r.Quantity),
            total = responses.Sum(r => r.Subtotal)
        });
    }

    // POST: api/cart — thêm (hoặc cộng dồn) một truyện vào giỏ.
    [HttpPost]
    public async Task<ActionResult<CartItemResponse>> Add([FromBody] CartAddRequest request)
    {
        if (request.Quantity <= 0) return BadRequest(new { message = "Quantity phải lớn hơn 0" });

        var userExists = await _db.Users.AnyAsync(u => u.Id == request.UserId);
        if (!userExists) return BadRequest(new { message = $"Không tồn tại người dùng id={request.UserId}" });

        var comic = await _db.Comics.FindAsync(request.ComicId);
        if (comic is null) return BadRequest(new { message = $"Không tồn tại truyện id={request.ComicId}" });

        var existing = await _db.CartItems
            .Include(ci => ci.Comic)
            .FirstOrDefaultAsync(ci => ci.UserId == request.UserId && ci.ComicId == request.ComicId);

        if (existing is null)
        {
            existing = new CartItem
            {
                UserId = request.UserId,
                ComicId = request.ComicId,
                Quantity = request.Quantity
            };
            _db.CartItems.Add(existing);
        }
        else
        {
            existing.Quantity += request.Quantity;
        }

        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetByUser), new { userId = request.UserId }, existing.ToResponse());
    }

    // PUT: api/cart/5 — cập nhật số lượng.
    [HttpPut("{id:int}")]
    public async Task<ActionResult<CartItemResponse>> Update(int id, [FromBody] CartUpdateRequest request)
    {
        if (request.Quantity <= 0) return BadRequest(new { message = "Quantity phải lớn hơn 0" });

        var item = await _db.CartItems
            .Include(ci => ci.Comic)
            .FirstOrDefaultAsync(ci => ci.Id == id);

        if (item is null) return NotFound(new { message = $"Không tìm thấy mục giỏ hàng id={id}" });

        item.Quantity = request.Quantity;
        await _db.SaveChangesAsync();

        return Ok(item.ToResponse());
    }

    // DELETE: api/cart/5 — xóa một mục trong giỏ.
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _db.CartItems.FindAsync(id);
        if (item is null) return NotFound(new { message = $"Không tìm thấy mục giỏ hàng id={id}" });

        _db.CartItems.Remove(item);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/cart?userId=1 — xóa sạch giỏ của một người dùng.
    [HttpDelete]
    public async Task<IActionResult> Clear([FromQuery] int userId)
    {
        if (userId <= 0) return BadRequest(new { message = "userId là bắt buộc" });

        var items = await _db.CartItems.Where(ci => ci.UserId == userId).ToListAsync();
        _db.CartItems.RemoveRange(items);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}