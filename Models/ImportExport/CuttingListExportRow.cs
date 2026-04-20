namespace CuttingOptimizer.Models.ImportExport
{
    public class CuttingListExportRow
    {
        public string? Position { get; set; }

        public string Description { get; set; } = string.Empty;

        public double Length { get; set; }
        public double Width { get; set; }

        public int Quantity { get; set; }

        public bool Rotatable { get; set; }

        public string? Note { get; set; }

        public string? FrontEdgeName { get; set; }
        public string? BackEdgeName { get; set; }
        public string? LeftEdgeName { get; set; }
        public string? RightEdgeName { get; set; }
    }
}
