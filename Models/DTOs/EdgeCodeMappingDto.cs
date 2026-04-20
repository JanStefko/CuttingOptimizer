using System.ComponentModel.DataAnnotations;

namespace CuttingOptimizer.Models.DTOs
{
    public class EdgeCodeMappingDto
    {
        [Range(0, int.MaxValue)]
        public int Code { get; set; }

        public int? EdgeBandingId { get; set; }

        [Range(0, 10)]
        public double Premill { get; set; }
    }
}
