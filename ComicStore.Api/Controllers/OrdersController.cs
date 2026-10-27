using ComicStore.Api.Data;
using ComicStore.Api.Dtos;
using ComicStore.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ComicStore.Api.Controllers;

/// <summary>CRUD đơn hàng: GET / POST / PUT / DELETE.</summary>
[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly AppDbContext _db;

    public OrdersController(AppDbContext db) => _db = db;

    // GET: api/orders?userId=1&status=PENDING
    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderResponse>>> GetAll(
        [FromQuery] int? userId,
        [FromQuery] string? status)
    {
        IQueryable<Order> query = _db.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Comic)
            .AsNoTracking();

        if (userId.HasValue) query = query.Where(o => o.UserId == userId);
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<OrderStatus>(status, true, out var parsed))
            query = query.Where(o => o.Status == parsed);

        var orders = await query.OrderByDescending(o => o.CreatedAt).ToListAsync();
        return Ok(orders.Select(o => o.ToResponse()));
    }

    // GET: api/orders/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<OrderResponse>> GetById(int id)
    {
        var order = await _db.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .ThenInclude(i => i.Comic)
            .FirstOrDefaultAsync(o => o.Id == id);

        return order is null ? NotFound(new { message = $"Không tìm thấy đơn hàng id={id}" }) : Ok(order.ToResponse());
    }

    // POST: api/orders - server tự tính giá và tổng tiền từ database.
    [HttpPost]
    public async Task<ActionResult<OrderResponse>> Create([FromBody] OrderCreateRequest request)
    {
        if (request.Items is null || request.Items.Count == 0)
            return BadRequest(new { message = "Đơn hàng phải có ít nhất 1 sản phẩm" });

        var user = await _db.Users.FindAsync(request.UserId);
        if (user is null) return BadRequest(new { message = $"Không tồn tại người dùng id={request.UserId}" });

        var comicIds = request.Items.Select(i => i.ComicId).Distinct().ToList();
        var comics = await _db.Comics.Where(c => comicIds.Contains(c.Id)).ToDictionaryAsync(c => c.Id);

        var order = new Order
        {
            UserId = request.UserId,
            PaymentMethod = string.IsNullOrWhiteSpace(request.PaymentMethod) ? "COD" : request.PaymentMethod,
            ShippingName = request.ShippingName ?? user.Name,
            ShippingPhone = request.ShippingPhone ?? user.Phone,
            ShippingAddress = request.ShippingAddress ?? user.Address
        };

        foreach (var item in request.Items)
        {
            if (item.Quantity <= 0)
                return BadRequest(new { message = "Quantity phải lớn hơn 0" });

            if (!comics.TryGetValue(item.ComicId, out var comic))
                return BadRequest(new { message = $"Không tồn tại truyện id={item.ComicId}" });

            if (comic.Stock < item.Quantity)
                return Conflict(new { message = $"Truyện '{comic.Title}' chỉ còn {comic.Stock} bản" });

            order.Total += comic.Price * item.Quantity;
            order.Items.Add(new OrderItem
            {
                ComicId = comic.Id,
                Quantity = item.Quantity,
                Price = comic.Price
            });

            comic.Stock -= item.Quantity;
        }

        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = order.Id }, order.ToResponse());
    }

    // PUT: api/orders/5 - cập nhật trạng thái / thông tin giao hàng.
    [HttpPut("{id:int}")]
    public async Task<ActionResult<OrderResponse>> Update(int id, [FromBody] OrderUpdateRequest request)
    {
        var order = await _db.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Comic)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order is null) return NotFound(new { message = $"Không tìm thấy đơn hàng id={id}" });
        if (!Enum.TryParse<OrderStatus>(request.Status, true, out var status))
            return BadRequest(new { message = "Status không hợp lệ (Pending|Paid|Confirmed|Shipping|Delivered|ReturnRequested|Returned|Rejected|Cancelled)" });

        // Nếu đơn bị hủy thì cộng lại kho.
        if (status == OrderStatus.Cancelled && order.Status != OrderStatus.Cancelled)
        {
            foreach (var item in order.Items)
            {
                item.Comic.Stock += item.Quantity;
            }
        }

        order.Status = status;
        order.ShippingName = request.ShippingName ?? order.ShippingName;
        order.ShippingPhone = request.ShippingPhone ?? order.ShippingPhone;
        order.ShippingAddress = request.ShippingAddress ?? order.ShippingAddress;
        order.ReturnReason = request.ReturnReason;

        await _db.SaveChangesAsync();
        return Ok(order.ToResponse());
    }

    // DELETE: api/orders/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var order = await _db.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order is null) return NotFound(new { message = $"Không tìm thấy đơn hàng id={id}" });

        // Hoàn lại kho trước khi xóa (các OrderItem sẽ bị cascade xóa theo).
        foreach (var item in order.Items)
        {
            var comic = await _db.Comics.FindAsync(item.ComicId);
            if (comic is not null) comic.Stock += item.Quantity;
        }

        _db.Orders.Remove(order);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}