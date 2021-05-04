using FrozenForge.Apis.Tests.Integrations;
using FrozenForge.Apis.Tests.Integrations.Models;
using Microsoft.AspNetCore.Mvc;

namespace FrozenForge.Apis.Tests.Controllers
{
    [ApiController]
    [Route("/api/[controller]/[action]")]
    public class TestController : ControllerBase
    {
        public TestController()
        {

        }


        [HttpPost]
        public IActionResult OkWithParameters(TestParameters parameters)
        {
            return Ok(new TestResult
            {
                Answer = $"Hello {parameters.Name}"
            });
        }

        [HttpPost]
        public IActionResult OkWithNoParameters()
        {
            return Ok(new TestResult
            {
                Answer = $"Hello anonymous person!"
            });
        }

        [HttpPost]
        public IActionResult BadRequestWithParameters(TestParameters parameters)
        {
            return BadRequest(new TestResult
            {
                Answer = $"Hello {parameters.Name}"
            });
        }
    }
}
