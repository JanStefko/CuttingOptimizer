using CuttingOptimizer.Models.DTOs;

namespace CuttingOptimizer.Services.Interfaces
{
    public interface ISheetMaterialService
    {
        Task<List<SheetMaterialResponse>> GetAllAsync();
        Task<SheetMaterialResponse?> GetByIdAsync(int id);
    }
}
