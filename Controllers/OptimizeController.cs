using CuttingOptimizer.Models.DTOs;
using CuttingOptimizer.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CuttingOptimizer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OptimizeController : ControllerBase
    {
        private readonly IOptimizeService _optimizeService;

        public OptimizeController(IOptimizeService optimizeService)
        {
            _optimizeService = optimizeService;
        }

        [HttpPost]
        public async Task<ActionResult<OptimizeCutPlanResponse>> Optimize(
            [FromBody] OptimizeCutPlanRequest request)
        {
            try
            {
                var response = await _optimizeService.OptimizeAsync(request);
                return Ok(response);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}