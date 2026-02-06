using Microsoft.AspNetCore.Mvc;

namespace DirectService.Presentation;

[ApiController]
[Route("[controller]")]
public class DepartmentController : ControllerBase
{
    [HttpGet]
    public Task<IActionResult> Test()
    {
        return Task.FromResult<IActionResult>(Ok());
    }
}