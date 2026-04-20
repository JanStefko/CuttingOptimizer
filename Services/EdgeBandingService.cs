using CuttingOptimizer.Models.DTOs;
using CuttingOptimizer.Repositories.Interfaces;
using CuttingOptimizer.Services.Interfaces;

namespace CuttingOptimizer.Services
{
    public class EdgeBandingService : IEdgeBandingService
    {
        private readonly IEdgeBandingRepository _edgeBandingRepository;

        public EdgeBandingService(IEdgeBandingRepository edgeBandingRepository)
        {
            _edgeBandingRepository = edgeBandingRepository;
        }

        public async Task<List<EdgeBandingResponse>> GetAllAsync()
        {
            var edgeBandings = await _edgeBandingRepository.GetAllAsync();

            return edgeBandings.Select(e => new EdgeBandingResponse
            {
                Id = e.Id,
                Name = e.Name,
                Thickness = e.Thickness,
                Width = e.Width
            }).ToList();
        }

        public async Task<EdgeBandingResponse?> GetByIdAsync(int id)
        {
            var edgeBanding = await _edgeBandingRepository.GetByIdAsync(id);

            if (edgeBanding is null)
            {
                return null;
            }

            return new EdgeBandingResponse
            {
                Id = edgeBanding.Id,
                Name = edgeBanding.Name,
                Thickness = edgeBanding.Thickness,
                Width = edgeBanding.Width
            };
        }
    }
}
