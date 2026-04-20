using CuttingOptimizer.Models.Entities;

namespace CuttingOptimizer.Repositories.Interfaces
{
    public interface IEdgeBandingRepository
    {
        Task<List<EdgeBanding>> GetAllAsync();
        Task<EdgeBanding?> GetByIdAsync(int id);
    }
}
