using Microsoft.AspNetCore.Mvc;

namespace DataRepository.SampleWeb.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class NumberController : ControllerBase
    {
        private readonly NumberService numberService;

        public NumberController(NumberService numberService)
        {
            this.numberService = numberService;
        }

        [HttpGet]
        public async Task<IActionResult> Page([FromQuery] int pageIndex, [FromQuery] int pageSize)
        {
            var datas = await numberService.PageAsync(pageIndex,pageSize);

            return Ok(datas);
        }

        [HttpGet]
        public async Task<IActionResult> Insert([FromQuery] int id, [FromQuery] int value)
        {
            var datas = await numberService.InsertAsync(id, value);

            return Ok(datas);
        }

        [HttpGet]
        public async Task<IActionResult> Update([FromQuery]int id, [FromQuery] int value)
        {
            var datas = await numberService.UpdateAsync(id, value);

            return Ok(datas);
        }

        [HttpGet]
        public async Task<IActionResult> Delete([FromQuery] int id)
        {
            var datas = await numberService.DeleteAsync(id);

            return Ok(datas);
        }
    }
}
