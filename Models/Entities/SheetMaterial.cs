namespace CuttingOptimizer.Models.Entities
{
    public class SheetMaterial
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? Manufacturer { get; set; }
        public double Length { get; set; }
        public double Width { get; set; }
        public double Thickness { get; set; }
        public bool HasGrain { get; set; }
    }
}
