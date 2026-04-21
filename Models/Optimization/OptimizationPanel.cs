using CuttingOptimizer.Models.DTOs;

namespace CuttingOptimizer.Models.Optimization;

public class OptimizationPanel
{
    public int Index { get; init; }
    public string Description { get; init; } = string.Empty;
    public string? Position { get; init; }
    public string? Note { get; init; }

    public double FinalLength { get; init; }
    public double FinalWidth { get; init; }

    public double CutLength { get; init; }
    public double CutWidth { get; init; }

    public bool CanRotate { get; init; }

    public int FrontEdgeCode { get; init; }
    public int BackEdgeCode { get; init; }
    public int LeftEdgeCode { get; init; }
    public int RightEdgeCode { get; init; }

    public double FrontPremill { get; init; }
    public double BackPremill { get; init; }
    public double LeftPremill { get; init; }
    public double RightPremill { get; init; }

    public double CutArea => CutLength * CutWidth;

    public static OptimizationPanel Create(
        PanelInputDto dto,
        Dictionary<int, EdgeCodeMappingDto> edgeMappings,
        bool sheetHasGrain,
        int index)
    {
        var frontPremill = GetPremill(dto.FrontEdgeCode, edgeMappings);
        var backPremill = GetPremill(dto.BackEdgeCode, edgeMappings);
        var leftPremill = GetPremill(dto.LeftEdgeCode, edgeMappings);
        var rightPremill = GetPremill(dto.RightEdgeCode, edgeMappings);

        return new OptimizationPanel
        {
            Index = index,
            Description = dto.Description,
            Position = dto.Position,
            Note = dto.Note,
            FinalLength = dto.Length,
            FinalWidth = dto.Width,
            CutLength = dto.Length + leftPremill + rightPremill,
            CutWidth = dto.Width + frontPremill + backPremill,
            CanRotate = !sheetHasGrain || dto.Rotatable,
            FrontEdgeCode = dto.FrontEdgeCode,
            BackEdgeCode = dto.BackEdgeCode,
            LeftEdgeCode = dto.LeftEdgeCode,
            RightEdgeCode = dto.RightEdgeCode,
            FrontPremill = frontPremill,
            BackPremill = backPremill,
            LeftPremill = leftPremill,
            RightPremill = rightPremill
        };
    }

    private static double GetPremill(int code, Dictionary<int, EdgeCodeMappingDto> edgeMappings)
    {
        if (code == 0)
            return 0;

        if (!edgeMappings.TryGetValue(code, out var mapping))
            throw new InvalidOperationException($"Missing edge mapping for code {code}.");

        return mapping.Premill;
    }
}