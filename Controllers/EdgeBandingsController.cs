using CuttingOptimizer.Models.DTOs;
using CuttingOptimizer.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CuttingOptimizer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EdgeBandingsController : ControllerBase
    {
        private readonly IEdgeBandingService _edgeBandingService;

        public EdgeBandingsController(IEdgeBandingService edgeBandingService)
        {
            _edgeBandingService = edgeBandingService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<EdgeBandingResponse>>> GetAll()
        {
            var response = await _edgeBandingService.GetAllAsync();
            return Ok(response);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<EdgeBandingResponse>> GetById(int id)
        {
            var response = await _edgeBandingService.GetByIdAsync(id);

            if (response is null)
            {
                return NotFound();
            }

            return Ok(response);
        }
    }
}