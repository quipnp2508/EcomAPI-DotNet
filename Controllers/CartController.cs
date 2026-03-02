namespace EComAPI.Controllers
{
    using EComAPI.DTOs.Cart;
    using EComAPI.Services.Interfaces;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using System.Security.Claims;

    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CartController : ControllerBase
    {
        private readonly ICartService _service;

        public CartController(ICartService service)
        {
            _service = service;
        }

        private Guid GetUserId()
        {
            return Guid.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier));
        }

        [HttpPost]
        public async Task<IActionResult> Add(AddToCartRequest request)
        {
            await _service.AddToCartAsync(GetUserId(), request);
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> GetMyCart()
        {
            var cart = await _service.GetMyCartAsync(GetUserId());
            return Ok(cart);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Remove(Guid id)
        {
            await _service.RemoveAsync(GetUserId(), id);
            return NoContent();
        }
    }
}
