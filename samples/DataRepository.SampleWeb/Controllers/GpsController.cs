using DataRepository.Casing;
using DataRepository.Casing.Models;
using DataRepository.SampleWeb.Models;
using Microsoft.AspNetCore.Mvc;

namespace DataRepository.SampleWeb.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class GpsController : ControllerBase
    {
        private readonly NumberCalc numberCalc;

        public GpsController(NumberCalc numberCalc)
        {
            this.numberCalc = numberCalc;
        }

        [HttpGet]
        public async Task<IActionResult> Newest([FromQuery] string key)
        {
            return Ok(await numberCalc.NewestAsync(key));
        }

        [HttpGet]
        public async Task<IActionResult> TopN([FromQuery] string key)
        {
            return Ok(await numberCalc.TopNAsync(key));
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromQuery] string key, [FromBody] GpsPosition position)
        {
            await numberCalc.AddAsync(position);
            return Ok();
        }
    }
}
