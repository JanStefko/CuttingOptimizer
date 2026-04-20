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

        public int FrontEdgeCode { get; set; }
        public int BackEdgeCode { get; set; }
        public int LeftEdgeCode { get; set; }
        public int RightEdgeCode { get; set; }
    }
}