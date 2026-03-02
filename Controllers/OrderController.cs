namespace EComAPI.Controllers
{
    using EComAPI.Services.Interfaces;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using System.Security.Claims;

    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _service;

        public OrderController(IOrderService service)
        {
            _service = service;
        }

        private Guid GetUserId()
        {
            return Guid.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier));
        }

        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout()
        {
            var orderId = await _service.CheckoutAsync(GetUserId());
            return Ok(orderId);
        }

        [HttpGet("my-orders")]
        public async Task<IActionResult> MyOrders()
        {
            var orders = await _service.GetMyOrdersAsync(GetUserId());
            return Ok(orders);
        }
    }
}
