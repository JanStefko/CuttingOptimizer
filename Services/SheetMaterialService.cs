using CuttingOptimizer.Models.DTOs;
using CuttingOptimizer.Repositories.Interfaces;
using CuttingOptimizer.Services.Interfaces;

namespace CuttingOptimizer.Services
{
    public class SheetMaterialService: ISheetMaterialService
    {
        private readonly ISheetMaterialRepository _sheetMaterialRepository;

        public SheetMaterialService(ISheetMaterialRepository sheetMaterialRepository)
        {
            _sheetMaterialRepository = sheetMaterialRepository;
        }

        public async Task<List<SheetMaterialResponse>> GetAllAsync()
        {
            var materials = await _sheetMaterialRepository.GetAllAsync();

            return materials.Select(m => new SheetMaterialResponse
            {
                Id = m.Id,
                Name = m.Name
            }).ToList();
        }

        public async Task<SheetMaterialResponse?> GetByIdAsync(int id)
        {
            var material = await _sheetMaterialRepository.GetByIdAsync(id);

            if (material is null)
            {
                return null;
            }

            return new SheetMaterialResponse
            {
                Id = material.Id,
                Name = material.Name
            };
        }

    }
}
