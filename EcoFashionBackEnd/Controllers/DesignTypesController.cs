using EcoFashionBackEnd.Entities;
using EcoFashionBackEnd.Repositories;
using EcoFashionBackEnd.Services;
using Microsoft.AspNetCore.Mvc;

namespace EcoFashionBackEnd.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DesignTypesController : ControllerBase
    {
        private readonly DesignTypeService _designTypeService;

        public DesignTypesController(DesignTypeService designTypeService)
        {
            _designTypeService = designTypeService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _designTypeService.GetAllItemTypesAsync();
            return Ok(result);
        }
    }

}
