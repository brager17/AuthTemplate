using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace AuthProject
{
    public class AgeRequirement : IAuthorizationRequirement
    {
        public readonly int _age;

        public AgeRequirement(int age)
        {
            _age = age;
        }
    }

    // register in startup
    public class MinAgeHandler : AuthorizationHandler<AgeRequirement>
    {
        private readonly IHttpContextAccessor _accessor;

        public MinAgeHandler(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AgeRequirement requirement)
        {
            var age = int.Parse(context.User.Claims.ToList().Single(x => x.Type == "Age").Value);
            if (age < requirement._age)
            {
                context.Fail();
            }

            return Task.CompletedTask;
        }
    }
}