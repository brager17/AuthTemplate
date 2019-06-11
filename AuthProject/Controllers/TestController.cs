using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AuthProject.WorkflowTest;
using Force;
using Microsoft.AspNetCore.Mvc;

namespace AuthProject.Controllers
{
    public class Dto
    {
        public Dto()
        {
        }

        public string Name { get; set; }
    }

    [Route("api/[controller]/[action]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        [HttpPost]
        public async Task<ActionResult<AddClaimsInputDto>> Index(
            [WorkFlowAttribute(typeof(TestWorkflow))]
            IAsyncHandler<CreateNewUserInputDto, AddClaimsInputDto> handler,
            CreateNewUserInputDto dto)
        {
            var s = (double) 12 / 0;
            CancellationToken ct = CancellationToken.None;
            return await handler.Handle(dto, ct);
        }
    }
}