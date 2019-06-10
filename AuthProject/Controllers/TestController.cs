using System.Net;
using AuthProject.WorkflowTest;
using Force;
using Microsoft.AspNetCore.Mvc;

namespace AuthProject.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        [HttpGet]
        public IActionResult Index([FromServices] IHandler<InputDto, CreateUserDto> hander)
        {
            
            return Ok();
        }
    }
}