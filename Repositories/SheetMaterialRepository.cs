using CuttingOptimizer.Data;
using CuttingOptimizer.Models.Entities;
using CuttingOptimizer.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CuttingOptimizer.Repositories
{
    public class SheetMaterialRepository : ISheetMaterialRepository
    {
        private readonly AppDbContext _context;

        public SheetMaterialRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<SheetMaterial>> GetAllAsync()
        {
            return await _context.SheetMaterials
               .AsNoTracking()
               .OrderBy(x => x.Name)
               .ToListAsync();
        }

        public async Task<SheetMaterial?> GetByIdAsync(int id)
        {
            return await _context.SheetMaterials
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);
        }
    }
}
