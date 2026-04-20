using System.ComponentModel.DataAnnotations;

namespace CuttingOptimizer.Models.ImportExport
{
    public class PanelImportRow
    {
        public string? Position { get; set; }

        [Required]
        public string Description { get; set; } = string.Empty;

        [Range(0.1, 10000)]
        public double Length { get; set; }

        [Range(0.1, 10000)]
        public double Width { get; set; }

        [Range(1, 1000)]
        public int Quantity { get; set; } = 1;

        public bool Rotatable { get; set; }

        public string? Note { get; set; }

        [Range(0, int.MaxValue)]
        public int FrontEdgeCode { get; set; } = 0;

        [Range(0, int.MaxValue)]
        public int BackEdgeCode { get; set; } = 0;

        [Range(0, int.MaxValue)]
        public int LeftEdgeCode { get; set; } = 0;

        [Range(0, int.MaxValue)]
        public int RightEdgeCode { get; set; } = 0;
    }
}