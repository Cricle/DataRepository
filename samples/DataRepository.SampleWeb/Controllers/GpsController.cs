using DataRepository.Casing;
using DataRepository.Casing.Models;
using DataRepository.SampleWeb.Models;
using Microsoft.AspNetCore.Mvc;

namespace DataRepository.SampleWeb.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class GpsController:ControllerBase
    {
        private readonly INewest<GpsPosition> positionNewest;

        public GpsController(INewest<GpsPosition> positionNewest)
        {
            this.positionNewest = positionNewest;
        }

        [HttpGet]
        public async Task<IActionResult> Newest([FromQuery]string key)
        {
            return Ok(await positionNewest.GetAsync(key));
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromQuery]string key, [FromBody]GpsPosition position)
        {
            await positionNewest.AddAsync(key, new NewestResult<GpsPosition>(position.Time, position));
            return Ok();
        }
    }
}
