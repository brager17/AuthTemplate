using AuthProject.WorkflowTest;
using Microsoft.AspNetCore.Mvc;

namespace AuthProject.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        [HttpPost]
        public IActionResult Index([FromBody] WorkFlow.InputDto inputDto)
        {
            return Ok(inputDto);
        }
    }
}