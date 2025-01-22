using Dapper;
using DataRepository.SampleWeb.Models;
using Microsoft.AspNetCore.Mvc;

namespace DataRepository.SampleWeb.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class NumberController(NumberService numberService,IDataRespositoryCreator dataRespositoryCreator) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Page([FromQuery] int pageIndex, [FromQuery] int pageSize) => Ok(await numberService.PageAsync(pageIndex, pageSize));

        [HttpGet]
        public async Task<IActionResult> Data([FromQuery] int id) 
            => Ok(await dataRespositoryCreator.CreateScope().GetConnection().QueryFirstOrDefaultAsync<Number>("SELECT * FROM numbers WHERE id = @id LIMIT 1", new {id}));

        [HttpGet]
        public async Task<IActionResult> Insert([FromQuery] int id, [FromQuery] int value) => Ok(await numberService.InsertAsync(id, value));

        [HttpGet]
        public async Task<IActionResult> Increase([FromQuery] int id, [FromQuery] int value) => Ok(await numberService.IncreaseAsync(id, value));

        [HttpGet]
        public async Task<IActionResult> Update([FromQuery] int id, [FromQuery] int value) => Ok(await numberService.UpdateAsync(id, value));

        [HttpGet]
        public async Task<IActionResult> Delete([FromQuery] int id) => Ok(await numberService.DeleteAsync(id));
    }
}
