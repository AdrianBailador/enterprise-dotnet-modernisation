using Legacy.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Legacy.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
    // ❌ No constructor injection — direct static call, impossible to test in isolation
    [HttpGet("{id}")]
    public IActionResult Get(int id)
    {
        // ❌ Synchronous — this thread is blocked until the DB responds
        var order = OrderService.GetOrder(id);

        if (order == null)
            return NotFound();

        return Ok(order);
    }

    [HttpPatch("{id}/status")]
    public IActionResult UpdateStatus(int id, [FromBody] int statusCode)
    {
        var updated = OrderService.UpdateStatus(id, statusCode);
        return updated ? NoContent() : NotFound();
    }
}
