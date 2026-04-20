using CuttingOptimizer.Data;
using CuttingOptimizer.Models.Entities;
using CuttingOptimizer.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CuttingOptimizer.Repositories
{
    public class EdgeBandingRepository: IEdgeBandingRepository
    {
        private readonly AppDbContext _context;

        public EdgeBandingRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<List<EdgeBanding>> GetAllAsync()
        {
            return await _context.EdgeBandings
                .AsNoTracking()
                .OrderBy(x => x.Name)
                .ToListAsync();
        }

        public async Task<EdgeBanding?> GetByIdAsync(int id)
        {
            return await _context.EdgeBandings
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);
        }
    }
}
