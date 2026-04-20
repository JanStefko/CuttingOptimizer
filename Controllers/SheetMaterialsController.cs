using CuttingOptimizer.Models.DTOs;
using CuttingOptimizer.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CuttingOptimizer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SheetMaterialsController : ControllerBase
    {
        private readonly ISheetMaterialService _sheetMaterialService;

        public SheetMaterialsController(ISheetMaterialService sheetMaterialService)
        {
            _sheetMaterialService = sheetMaterialService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SheetMaterialResponse>>> GetAll()
        {
            var response = await _sheetMaterialService.GetAllAsync();
            return Ok(response);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<SheetMaterialResponse>> GetById(int id)
        {
            var response = await _sheetMaterialService.GetByIdAsync(id);

            if (response is null)
            {
                return NotFound();
            }

            return Ok(response);
        }
    }
}