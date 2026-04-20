using CuttingOptimizer.Models.ImportExport;
using System.ComponentModel.DataAnnotations;

namespace CuttingOptimizer.Models.DTOs
{
    public class OptimizeCutPlanRequest
    {
        [Required]
        public int SheetMaterialId { get; set; }

        [Range(0, 20)]
        public double Kerf { get; set; }

        [Range(0, 100)]
        public double TrimMargin { get; set; }

        [Required]
        public List<EdgeCodeMappingDto> EdgeCodes { get; set; } = [];

        [Required]
        public List<PanelInputDto> Panels { get; set; } = [];
    }
}
