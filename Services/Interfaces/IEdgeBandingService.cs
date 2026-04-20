using CuttingOptimizer.Models.DTOs;

namespace CuttingOptimizer.Services.Interfaces
{
    public interface IEdgeBandingService
    {
        Task<List<EdgeBandingResponse>> GetAllAsync();
        Task<EdgeBandingResponse?> GetByIdAsync(int id);
    }
}
