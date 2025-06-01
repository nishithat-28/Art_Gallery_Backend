
// Controllers/OrderController.cs
using ArtGallery.API.Data;
using ArtGallery.API.DTOs;
using ArtGallery.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ArtGallery.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly ArtGalleryContext _context;

        public OrderController(ArtGalleryContext context)
        {
            _context = context;
        }

        // GET: api/Order
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderResponseDto>>> GetOrders()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            bool isAdmin = User.IsInRole("Admin");

            var ordersQuery = _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.ArtWork)
                .AsQueryable();

            // If not admin, only return user's own orders
            if (!isAdmin)
            {
                ordersQuery = ordersQuery.Where(o => o.UserId == userId);
            }

            var orders = await ordersQuery.ToListAsync();

            var orderDtos = orders.Select(o => new OrderResponseDto
            {
                Id = o.Id,
                UserId = o.UserId,
                Username = o.User.Username,
                OrderDate = o.OrderDate,
                TotalAmount = o.TotalAmount,
                Status = o.Status,
                ShippingAddress = o.ShippingAddress,
                PaymentMethod = o.PaymentMethod,
                InvoiceNumber = o.InvoiceNumber,
                OrderItems = o.OrderItems.Select(oi => new OrderItemResponseDto
                {
                    Id = oi.Id,
                    ArtWorkId = oi.ArtWorkId,
                    ArtWorkTitle = oi.ArtWork.Title,
                    ArtWorkArtist = oi.ArtWork.Artist,
                    Price = oi.Price,
                    Quantity = oi.Quantity,
                    Subtotal = oi.Price * oi.Quantity
                }).ToList()
            }).ToList();

            return Ok(orderDtos);
        }

        // GET: api/Order/5
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderResponseDto>> GetOrder(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            bool isAdmin = User.IsInRole("Admin");

            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.ArtWork)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            // Check if the user is authorized to view this order
            if (!isAdmin && order.UserId != userId)
            {
                return Unauthorized();
            }

            var orderDto = new OrderResponseDto
            {
                Id = order.Id,
                UserId = order.UserId,
                Username = order.User.Username,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                ShippingAddress = order.ShippingAddress,
                PaymentMethod = order.PaymentMethod,
                InvoiceNumber = order.InvoiceNumber,
                OrderItems = order.OrderItems.Select(oi => new OrderItemResponseDto
                {
                    Id = oi.Id,
                    ArtWorkId = oi.ArtWorkId,
                    ArtWorkTitle = oi.ArtWork.Title,
                    ArtWorkArtist = oi.ArtWork.Artist,
                    Price = oi.Price,
                    Quantity = oi.Quantity,
                    Subtotal = oi.Price * oi.Quantity
                }).ToList()
            };

            return Ok(orderDto);
        }

        // POST: api/Order
        [HttpPost]
        public async Task<ActionResult<OrderResponseDto>> CreateOrder(OrderCreateDto orderCreateDto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return BadRequest("User not found");
            }

            // Check if art works exist and are available
            var artWorkIds = orderCreateDto.OrderItems.Select(oi => oi.ArtWorkId).ToList();
            var artWorks = await _context.ArtWorks
                .Where(a => artWorkIds.Contains(a.Id) && a.IsAvailable)
                .ToListAsync();

            if (artWorks.Count != artWorkIds.Count)
            {
                return BadRequest("One or more art works are not available");
            }

            // Create order
            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.Now,
                Status = "Pending",
                ShippingAddress = orderCreateDto.ShippingAddress,
                PaymentMethod = orderCreateDto.PaymentMethod,
                InvoiceNumber = GenerateInvoiceNumber(),
                OrderItems = new List<OrderItem>()
            };

            // Add order items and calculate total
            decimal totalAmount = 0;
            foreach (var item in orderCreateDto.OrderItems)
            {
                var artWork = artWorks.First(a => a.Id == item.ArtWorkId);
                var orderItem = new OrderItem
                {
                    ArtWorkId = item.ArtWorkId,
                    Price = artWork.Price,
                    Quantity = item.Quantity
                };
                order.OrderItems.Add(orderItem);
                totalAmount += artWork.Price * item.Quantity;

                // Mark artwork as unavailable (since it's being purchased)
                artWork.IsAvailable = false;
            }

            order.TotalAmount = totalAmount;

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Return the created order with additional details
            var createdOrder = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.ArtWork)
                .FirstOrDefaultAsync(o => o.Id == order.Id);

            var orderDto = new OrderResponseDto
            {
                Id = createdOrder.Id,
                UserId = createdOrder.UserId,
                Username = createdOrder.User.Username,
                OrderDate = createdOrder.OrderDate,
                TotalAmount = createdOrder.TotalAmount,
                Status = createdOrder.Status,
                ShippingAddress = createdOrder.ShippingAddress,
                PaymentMethod = createdOrder.PaymentMethod,
                InvoiceNumber = createdOrder.InvoiceNumber,
                OrderItems = createdOrder.OrderItems.Select(oi => new OrderItemResponseDto
                {
                    Id = oi.Id,
                    ArtWorkId = oi.ArtWorkId,
                    ArtWorkTitle = oi.ArtWork.Title,
                    ArtWorkArtist = oi.ArtWork.Artist,
                    Price = oi.Price,
                    Quantity = oi.Quantity,
                    Subtotal = oi.Price * oi.Quantity
                }).ToList()
            };

            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, orderDto);
        }

        // GET: api/Order/invoice/5
        [HttpGet("invoice/{id}")]
        public async Task<ActionResult<InvoiceDto>> GetInvoice(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            bool isAdmin = User.IsInRole("Admin");

            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.ArtWork)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            // Check if the user is authorized to view this invoice
            if (!isAdmin && order.UserId != userId)
            {
                return Unauthorized();
            }

            decimal subtotal = order.OrderItems.Sum(oi => oi.Price * oi.Quantity);
            decimal taxRate = 0.08M; // 8% tax rate
            decimal tax = subtotal * taxRate;
            decimal total = subtotal + tax;

            var invoiceDto = new InvoiceDto
            {
                InvoiceNumber = order.InvoiceNumber,
                InvoiceDate = order.OrderDate,
                CustomerName = $"{order.User.FirstName} {order.User.LastName}",
                CustomerEmail = order.User.Email,
                ShippingAddress = order.ShippingAddress,
                PaymentMethod = order.PaymentMethod,
                Items = order.OrderItems.Select(oi => new OrderItemResponseDto
                {
                    Id = oi.Id,
                    ArtWorkId = oi.ArtWorkId,
                    ArtWorkTitle = oi.ArtWork.Title,
                    ArtWorkArtist = oi.ArtWork.Artist,
                    Price = oi.Price,
                    Quantity = oi.Quantity,
                    Subtotal = oi.Price * oi.Quantity
                }).ToList(),
                Subtotal = subtotal,
                Tax = tax,
                Total = total
            };

            return Ok(invoiceDto);
        }

        private string GenerateInvoiceNumber()
        {
            // Generate a unique invoice number (e.g. INV-20240426-001)
            var today = DateTime.Now;
            var prefix = $"INV-{today:yyyyMMdd}";
            
            // Get the latest invoice number with the same prefix
            var latestInvoice = _context.Orders
                .Where(o => o.InvoiceNumber.StartsWith(prefix))
                .OrderByDescending(o => o.InvoiceNumber)
                .FirstOrDefault();
            
            int number = 1;
            if (latestInvoice != null)
            {
                var parts = latestInvoice.InvoiceNumber.Split('-');
                if (parts.Length == 3 && int.TryParse(parts[2], out int lastNumber))
                {
                    number = lastNumber + 1;
                }
            }
            
            return $"{prefix}-{number:D3}";
        }
    }
}