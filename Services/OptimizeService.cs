using CuttingOptimizer.Models.Calculation;
using CuttingOptimizer.Models.DTOs;
using CuttingOptimizer.Models.Entities;
using CuttingOptimizer.Models.Optimization;
using CuttingOptimizer.Repositories.Interfaces;
using CuttingOptimizer.Services.Cutting;
using CuttingOptimizer.Services.Interfaces;

namespace CuttingOptimizer.Services;

public class OptimizeService : IOptimizeService
{
    private readonly ISheetMaterialRepository _sheetMaterialRepository;
    private readonly ICuttingAlgorithm _cuttingAlgorithm;

    public OptimizeService(
        ISheetMaterialRepository sheetMaterialRepository,
        ICuttingAlgorithm cuttingAlgorithm)
    {
        _sheetMaterialRepository = sheetMaterialRepository;
        _cuttingAlgorithm = cuttingAlgorithm;
    }

    public async Task<OptimizeCutPlanResponse> OptimizeAsync(OptimizeCutPlanRequest request)
    {
        ValidateRequest(request);

        var sheetMaterial = await GetSheetMaterialAsync(request.SheetMaterialId);
        var edgeMappings = BuildEdgeMappings(request);
        var panels = ExpandAndPreparePanels(request, edgeMappings, sheetMaterial);

        var (usableLength, usableWidth) = GetUsableSheetDimensions(sheetMaterial, request.TrimMargin);
        var result = _cuttingAlgorithm.Cut(usableLength, usableWidth, request.Kerf, panels);

        return BuildResponse(sheetMaterial, result, request.Kerf, request.TrimMargin, panels.Count);
    }

    private static void ValidateRequest(OptimizeCutPlanRequest request)
    {
        if (request.Panels is null || request.Panels.Count == 0)
            throw new InvalidOperationException("At least one panel is required.");
    }

    private async Task<SheetMaterial> GetSheetMaterialAsync(int sheetMaterialId)
    {
        var material = await _sheetMaterialRepository.GetByIdAsync(sheetMaterialId);

        if (material is null)
            throw new KeyNotFoundException($"Sheet material {sheetMaterialId} was not found.");

        return material;
    }

    private static Dictionary<int, EdgeCodeMappingDto> BuildEdgeMappings(OptimizeCutPlanRequest request)
    {
        var result = request.EdgeCodes?.ToDictionary(x => x.Code) ?? [];
        result[0] = new EdgeCodeMappingDto { Code = 0, EdgeBandingId = null, Premill = 0 };
        return result;
    }

    private static List<OptimizationPanel> ExpandAndPreparePanels(
        OptimizeCutPlanRequest request,
        Dictionary<int, EdgeCodeMappingDto> edgeMappings,
        SheetMaterial sheetMaterial)
    {
        var result = new List<OptimizationPanel>();
        var panelIndex = 1;

        foreach (var panel in request.Panels)
        {
            for (var i = 0; i < panel.Quantity; i++)
            {
                result.Add(OptimizationPanel.Create(
                    panel,
                    edgeMappings,
                    sheetMaterial.HasGrain,
                    panelIndex++));
            }
        }

        return result
            .OrderByDescending(x => x.CutArea)
            .ThenByDescending(x => Math.Max(x.CutLength, x.CutWidth))
            .ToList();
    }

    private static (double UsableLength, double UsableWidth) GetUsableSheetDimensions(
        SheetMaterial sheetMaterial,
        double trimMargin)
    {
        var usableLength = sheetMaterial.Length - (2 * trimMargin);
        var usableWidth = sheetMaterial.Width - (2 * trimMargin);

        if (usableLength <= 0 || usableWidth <= 0)
            throw new InvalidOperationException("Trim margin is too large for selected sheet material.");

        return (usableLength, usableWidth);
    }

    private static OptimizeCutPlanResponse BuildResponse(
        SheetMaterial sheetMaterial,
        OptimizationResult result,
        double kerf,
        double trimMargin,
        int totalPanelsCount)
    {
        var plan = new CutPlan
        {
            SheetMaterialId = sheetMaterial.Id,
            SheetLength = sheetMaterial.Length,
            SheetWidth = sheetMaterial.Width,
            Kerf = kerf,
            TrimMargin = trimMargin,
            TotalPanelsCount = totalPanelsCount,
            UsedSheetsCount = result.Sheets.Count,
            Sheets = result.Sheets.Select(sheet => BuildSheet(sheet)).ToList()
        };

        plan.TotalUsedArea = plan.Sheets.Sum(x => x.UsedArea);
        plan.TotalWasteArea = plan.Sheets.Sum(x => x.WasteArea);

        var totalSheetArea = plan.UsedSheetsCount * plan.SheetLength * plan.SheetWidth;
        plan.WastePercentage = totalSheetArea > 0
            ? (plan.TotalWasteArea / totalSheetArea) * 100
            : 0;

        return new OptimizeCutPlanResponse
        {
            CutPlan = plan
        };
    }

    private static CutPlanSheet BuildSheet(OptimizationSheet sheet)
    {
        var usedArea = sheet.Placements.Sum(x => x.Length * x.Width);
        var totalArea = sheet.UsableLength * sheet.UsableWidth;
        var wasteArea = totalArea - usedArea;

        return new CutPlanSheet
        {
            SheetNumber = sheet.SheetNumber,
            Length = sheet.UsableLength,
            Width = sheet.UsableWidth,
            UsedArea = usedArea,
            WasteArea = wasteArea,
            WastePercentage = totalArea > 0 ? (wasteArea / totalArea) * 100 : 0,
            Items = sheet.Placements.Select(BuildItem).ToList()
        };
    }

    private static CutPlanItem BuildItem(PanelPlacement placement)
    {
        return new CutPlanItem
        {
            PanelId = placement.Panel.Index,
            Description = placement.Panel.Description,
            Position = placement.Panel.Position,
            Note = placement.Panel.Note,
            X = Math.Round(placement.X, 2),
            Y = Math.Round(placement.Y, 2),
            FinalLength = Math.Round(placement.Panel.FinalLength, 2),
            FinalWidth = Math.Round(placement.Panel.FinalWidth, 2),
            CutLength = Math.Round(placement.Length, 2),
            CutWidth = Math.Round(placement.Width, 2),
            IsRotated = placement.IsRotated,
            FrontEdgeCode = placement.Panel.FrontEdgeCode,
            BackEdgeCode = placement.Panel.BackEdgeCode,
            LeftEdgeCode = placement.Panel.LeftEdgeCode,
            RightEdgeCode = placement.Panel.RightEdgeCode
        };
    }
}