using ComicStore.Api.Data;
using ComicStore.Api.Dtos;
using ComicStore.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ComicStore.Api.Controllers;

/// <summary>CRUD người dùng: GET / POST / PUT / DELETE.</summary>
[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _db;

    public UsersController(AppDbContext db) => _db = db;

    // GET: api/users?role=BUYER
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserResponse>>> GetAll([FromQuery] string? role)
    {
        IQueryable<User> query = _db.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(role) && Enum.TryParse<UserRole>(role, true, out var parsedRole))
        {
            query = query.Where(u => u.Role == parsedRole);
        }

        var users = await query.OrderBy(u => u.Id).ToListAsync();
        return Ok(users.Select(u => u.ToResponse()));
    }

    // GET: api/users/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<UserResponse>> GetById(int id)
    {
        var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
        return user is null ? NotFound(new { message = $"Không tìm thấy người dùng id={id}" }) : Ok(user.ToResponse());
    }

    // POST: api/users
    [HttpPost]
    public async Task<ActionResult<UserResponse>> Create([FromBody] UserCreateRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { message = "Email và Name là bắt buộc" });

        if (!Enum.TryParse<UserRole>(request.Role, true, out var role))
            return BadRequest(new { message = "Role không hợp lệ (SystemAdmin|Manager|Publisher|Buyer)" });

        if (await _db.Users.AnyAsync(u => u.Email == request.Email))
            return Conflict(new { message = "Email đã tồn tại" });

        var user = new User
        {
            Email = request.Email.Trim().ToLower(),
            Name = request.Name.Trim(),
            PasswordHash = request.Password,
            Role = role,
            Phone = request.Phone,
            Address = request.Address
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = user.Id }, user.ToResponse());
    }

    // PUT: api/users/5
    [HttpPut("{id:int}")]
    public async Task<ActionResult<UserResponse>> Update(int id, [FromBody] UserUpdateRequest request)
    {
        var user = await _db.Users.FindAsync(id);
        if (user is null) return NotFound(new { message = $"Không tìm thấy người dùng id={id}" });

        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { message = "Email và Name là bắt buộc" });

        if (!Enum.TryParse<UserRole>(request.Role, true, out var role))
            return BadRequest(new { message = "Role không hợp lệ (SystemAdmin|Manager|Publisher|Buyer)" });

        var emailTaken = await _db.Users.AnyAsync(u =>
            u.Email == request.Email.Trim().ToLower() && u.Id != id);
        if (emailTaken) return Conflict(new { message = "Email đã tồn tại" });

        user.Email = request.Email.Trim().ToLower();
        user.Name = request.Name.Trim();
        user.Role = role;
        user.Phone = request.Phone;
        user.Address = request.Address;
        user.Avatar = request.Avatar;
        user.IsVerified = request.IsVerified;

        await _db.SaveChangesAsync();
        return Ok(user.ToResponse());
    }

    // DELETE: api/users/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user is null) return NotFound(new { message = $"Không tìm thấy người dùng id={id}" });

        var hasOrders = await _db.Orders.AnyAsync(o => o.UserId == id);
        if (hasOrders)
            return Conflict(new { message = "Người dùng đã có đơn hàng, không thể xóa" });

        _db.Users.Remove(user);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}