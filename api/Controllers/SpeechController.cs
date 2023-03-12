using api.Services;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers;

[ApiController]
[Produces("application/json")]
[Route("api/[controller]")]
public class SpeechController : Controller
{
    [HttpPost("")]
    public async Task<IActionResult> Post(IFormFile file, [FromServices] IKaldiAdapter kaldiAdapter)
    {
        var result = await kaldiAdapter.Recognize(file);
        return Ok(result);
    }
}
