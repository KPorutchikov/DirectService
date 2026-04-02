using Microsoft.AspNetCore.Mvc;

namespace DirectService.Presentation.Controllers.Departments;

[ApiController]
[Route("[controller]")]
public class DepartmentController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Test()
    {
        return Ok(await Task.FromResult("Hello World!"));    
    }
}