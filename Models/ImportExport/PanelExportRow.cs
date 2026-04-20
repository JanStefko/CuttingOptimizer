namespace CuttingOptimizer.Models.ImportExport
{
    public class PanelExportRow
    {
        public int SheetNumber { get; set; }

        public string Description { get; set; } = string.Empty;
        public string? Position { get; set; }
        public string? Note { get; set; }

        public double Length { get; set; }
        public double Width { get; set; }

        public double X { get; set; }
        public double Y { get; set; }

        public bool IsRotated { get; set; }

        public string? FrontEdgeName { get; set; }
        public string? BackEdgeName { get; set; }
        public string? LeftEdgeName { get; set; }
        public string? RightEdgeName { get; set; }
    }
}
