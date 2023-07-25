using Microsoft.AspNetCore.Mvc;
using MuMoo.Models.Dtos;
using MuMoo.Services;

namespace MuMoo.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class MuMooController : ControllerBase
    {
        private readonly MuMooService _service;
        public MuMooController(MuMooService service)
        {
            _service = service;
        }
        [HttpPost]
        public IActionResult GetClass(GetClassDto param)
        {
            var result = _service.GetClass(param.sql, param.className, param?.caseString?.ToLower(), param?.database?.ToLower(), param.connectionString);
            return Ok(result);
            //return Ok(param);
        }
        [HttpPost]
        public IActionResult GetMapping(GetMappingDto param)
        {
            var result = _service.GetMapping(param.tableName, param.caseString?.ToLower(), param.database?.ToLower(), param.connectionString, param.dotNet?.ToLower(), param.mapTpye);
            return Ok(result);
            //return Ok("");

        }
        [HttpGet]
        public IActionResult ParameterGuide()
        {
            var result = _service.GetParameterGuide();
            return Ok(result);
        }
    }
}