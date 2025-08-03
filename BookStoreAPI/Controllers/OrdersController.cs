using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookStoreAPI.Data;
using BookStoreAPI.Models;
using BookStoreAPI.DTOs;

namespace BookStoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly BookStoreDbContext _context;

        public OrdersController(BookStoreDbContext context)
        {
            _context = context;
        }

        // GET: api/Orders
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetOrders()
        {
            return await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Book)
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new
                {
                    Id = o.Id,
                    UserId = o.UserId,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status,
                    OrderDate = o.OrderDate,
                    ShippingAddress = o.ShippingAddress,
                    User = new
                    {
                        Id = o.User.Id,
                        FirstName = o.User.FirstName,
                        LastName = o.User.LastName,
                        Email = o.User.Email
                    },
                    OrderItems = o.OrderItems.Select(oi => new
                    {
                        Id = oi.Id,
                        BookId = oi.BookId,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice,
                        TotalPrice = oi.TotalPrice,
                        Book = new
                        {
                            Id = oi.Book.Id,
                            Title = oi.Book.Title,
                            Author = oi.Book.Author,
                            ImageUrl = oi.Book.ImageUrl
                        }
                    }).ToList()
                })
                .ToListAsync();
        }

        // GET: api/Orders/5
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetOrder(int id)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Book)
                .Where(o => o.Id == id)
                .Select(o => new
                {
                    Id = o.Id,
                    UserId = o.UserId,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status,
                    OrderDate = o.OrderDate,
                    ShippingAddress = o.ShippingAddress,
                    User = new
                    {
                        Id = o.User.Id,
                        FirstName = o.User.FirstName,
                        LastName = o.User.LastName,
                        Email = o.User.Email,
                        Phone = o.User.Phone,
                        Address = o.User.Address
                    },
                    OrderItems = o.OrderItems.Select(oi => new
                    {
                        Id = oi.Id,
                        BookId = oi.BookId,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice,
                        TotalPrice = oi.TotalPrice,
                        Book = new
                        {
                            Id = oi.Book.Id,
                            Title = oi.Book.Title,
                            Author = oi.Book.Author,
                            ImageUrl = oi.Book.ImageUrl,
                            CategoryId = oi.Book.CategoryId
                        }
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (order == null)
            {
                return NotFound();
            }

            return order;
        }

        // GET: api/Orders/user/5
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetOrdersByUser(int userId)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Book)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new
                {
                    Id = o.Id,
                    UserId = o.UserId,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status,
                    OrderDate = o.OrderDate,
                    ShippingAddress = o.ShippingAddress,
                    OrderItems = o.OrderItems.Select(oi => new
                    {
                        Id = oi.Id,
                        BookId = oi.BookId,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice,
                        TotalPrice = oi.TotalPrice,
                        Book = new
                        {
                            Id = oi.Book.Id,
                            Title = oi.Book.Title,
                            Author = oi.Book.Author,
                            ImageUrl = oi.Book.ImageUrl
                        }
                    }).ToList()
                })
                .ToListAsync();
        }

        // PUT: api/Orders/5/status
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] string status)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            var validStatuses = new[] { "Pending", "Processing", "Shipped", "Delivered", "Cancelled" };
            if (!validStatuses.Contains(status))
            {
                return BadRequest("Invalid status. Valid statuses: " + string.Join(", ", validStatuses));
            }

            order.Status = status;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Order status updated successfully", status = status });
        }

        // POST: api/Orders
        [HttpPost]
        public async Task<ActionResult<object>> PostOrder([FromBody] OrderCreateDto orderDto)
        {
            try
            {
                // Validation
                if (orderDto.UserId <= 0)
                {
                    return BadRequest("Valid UserId is required.");
                }

                if (orderDto.OrderItems == null || !orderDto.OrderItems.Any())
                {
                    return BadRequest("At least one order item is required.");
                }

                // Check if user exists (optional - you can skip this if user management is not implemented)
                // var userExists = await _context.Users.AnyAsync(u => u.Id == orderDto.UserId);
                // if (!userExists)
                // {
                //     return BadRequest("Invalid UserId. User does not exist.");
                // }

                var order = new Order
                {
                    UserId = orderDto.UserId,
                    ShippingAddress = orderDto.ShippingAddress,
                    OrderDate = DateTime.Now,
                    Status = "Pending",
                    OrderItems = new List<OrderItem>()
                };

                decimal totalAmount = 0;

                // Process order items
                foreach (var itemDto in orderDto.OrderItems)
                {
                    var book = await _context.Books.FindAsync(itemDto.BookId);
                    if (book == null)
                    {
                        return BadRequest($"Book with ID {itemDto.BookId} not found");
                    }

                    if (book.Stock < itemDto.Quantity)
                    {
                        return BadRequest($"Insufficient stock for book: {book.Title}. Available: {book.Stock}, Requested: {itemDto.Quantity}");
                    }

                    var orderItem = new OrderItem
                    {
                        BookId = itemDto.BookId,
                        Quantity = itemDto.Quantity,
                        UnitPrice = book.Price,
                        TotalPrice = book.Price * itemDto.Quantity
                    };

                    order.OrderItems.Add(orderItem);
                    totalAmount += orderItem.TotalPrice;

                    // Update stock
                    book.Stock -= itemDto.Quantity;
                }

                order.TotalAmount = totalAmount;

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetOrder", new { id = order.Id }, new
                {
                    id = order.Id,
                    totalAmount = order.TotalAmount,
                    status = order.Status,
                    orderDate = order.OrderDate,
                    message = "Order created successfully"
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error creating order: {ex.Message}");
            }
        }

        // DELETE: api/Orders/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            // Restore stock for cancelled orders
            foreach (var item in order.OrderItems)
            {
                var book = await _context.Books.FindAsync(item.BookId);
                if (book != null)
                {
                    book.Stock += item.Quantity;
                }
            }

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Order deleted successfully" });
        }

        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.Id == id);
        }
    }
}