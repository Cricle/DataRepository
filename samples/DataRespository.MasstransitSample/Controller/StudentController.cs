using DataRepository.Masstransit;
using Microsoft.AspNetCore.Mvc;

namespace DataRespository.MasstransitSample.Controller
{
    [Route("api/[controller]")]
    public class StudentController:ControllerBase
    {
        private readonly IStackingDataService<Student> stackingDataService;

        public StudentController(IStackingDataService<Student> stackingDataService)
        {
            this.stackingDataService = stackingDataService;
        }

        [HttpGet("add")]
        public async Task<IActionResult> Add(string name)
        {
            await stackingDataService.AddAsync(new Student { Name = name });
            return Ok();
        }

        [HttpGet("delete")]
        public async Task<IActionResult> Delete(int id)
        {
            await stackingDataService.DeleteAsync(stackingDataService.DataRespository.Where(x => x.Id == id), new List<string> { id.ToString() });
            return Ok();
        }

        [HttpGet("update")]
        public async Task<IActionResult> Update(int id,string newName)
        {
            await stackingDataService.UpdateAsync(stackingDataService.DataRespository.Where(x => x.Id == id), 
                x=>x.SetProperty(y=>y.Name,newName),
                new List<string> { id.ToString(),newName });
            return Ok();
        }
    }
}
