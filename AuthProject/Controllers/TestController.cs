using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AuthProject.WorkflowTest;
using Force;
using Force.Extensions;
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
        public async Task<ActionResult<ConfirmInfoDto>> Index(
            [WorkFlowAttribute(typeof(TestWorkflow))]
            IAsyncHandler<CreateNewUserInputDto, ActionResult<ConfirmInfoDto>> handler,
            CreateNewUserInputDto dto)
        {
            var s = (double) 12 / 0;
            var ct = CancellationToken.None;
            return await handler.Handle(dto, ct);
        }
    }
}