using CuttingOptimizer.Models.Entities;

namespace CuttingOptimizer.Repositories.Interfaces
{
    public interface ISheetMaterialRepository
    {
        Task<List<SheetMaterial>> GetAllAsync();
        Task<SheetMaterial?> GetByIdAsync(int id);
    }
}
