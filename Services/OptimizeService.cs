using CuttingOptimizer.Models.Calculation;
using CuttingOptimizer.Models.DTOs;
using CuttingOptimizer.Models.Entities;
using CuttingOptimizer.Models.Optimization;
using CuttingOptimizer.Repositories.Interfaces;
using CuttingOptimizer.Services.Interfaces;

namespace CuttingOptimizer.Services;

public class OptimizeService : IOptimizeService
{
    private readonly ISheetMaterialRepository _sheetMaterialRepository;

    public OptimizeService(ISheetMaterialRepository sheetMaterialRepository)
    {
        _sheetMaterialRepository = sheetMaterialRepository;
    }

    public async Task<OptimizeCutPlanResponse> OptimizeAsync(OptimizeCutPlanRequest request)
    {
        ValidateRequest(request);

        var sheetMaterial = await GetSheetMaterialAsync(request.SheetMaterialId);
        var edgeMappings = BuildEdgeMappings(request);
        var panels = ExpandAndPreparePanels(request, edgeMappings, sheetMaterial);
        var result = PackPanels(sheetMaterial, request.Kerf, request.TrimMargin, panels);

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

    private static OptimizationResult PackPanels(
        SheetMaterial sheetMaterial,
        double kerf,
        double trimMargin,
        List<OptimizationPanel> panels)
    {
        var usableLength = sheetMaterial.Length - (2 * trimMargin);
        var usableWidth = sheetMaterial.Width - (2 * trimMargin);

        if (usableLength <= 0 || usableWidth <= 0)
            throw new InvalidOperationException("Trim margin is too large for selected sheet material.");

        var result = new OptimizationResult();

        foreach (var panel in panels)
        {
            var placed = false;

            foreach (var sheet in result.Sheets)
            {
                if (TryPlacePanelOnSheet(sheet, panel, kerf))
                {
                    placed = true;
                    break;
                }
            }

            if (placed)
                continue;

            var newSheet = CreateEmptySheet(result.Sheets.Count + 1, usableLength, usableWidth);

            if (TryPlacePanelOnSheet(newSheet, panel, kerf))
            {
                result.Sheets.Add(newSheet);
            }
            else
            {
                result.UnplacedPanels.Add(panel);
            }
        }

        return result;
    }

    private static OptimizationSheet CreateEmptySheet(int sheetNumber, double usableLength, double usableWidth)
    {
        return new OptimizationSheet
        {
            SheetNumber = sheetNumber,
            UsableLength = usableLength,
            UsableWidth = usableWidth,
            FreeRectangles =
            [
                new FreeRectangle
                {
                    X = 0,
                    Y = 0,
                    Length = usableLength,
                    Width = usableWidth
                }
            ]
        };
    }

    private static bool TryPlacePanelOnSheet(OptimizationSheet sheet, OptimizationPanel panel, double kerf)
    {
        for (var i = 0; i < sheet.FreeRectangles.Count; i++)
        {
            var freeRect = sheet.FreeRectangles[i];

            if (freeRect.CanFit(panel.CutLength, panel.CutWidth))
            {
                PlacePanel(sheet, i, panel, panel.CutLength, panel.CutWidth, false, kerf);
                return true;
            }

            if (panel.CanRotate && freeRect.CanFit(panel.CutWidth, panel.CutLength))
            {
                PlacePanel(sheet, i, panel, panel.CutWidth, panel.CutLength, true, kerf);
                return true;
            }
        }

        return false;
    }

    private static void PlacePanel(
        OptimizationSheet sheet,
        int freeRectIndex,
        OptimizationPanel panel,
        double placedLength,
        double placedWidth,
        bool isRotated,
        double kerf)
    {
        var freeRect = sheet.FreeRectangles[freeRectIndex];

        sheet.Placements.Add(new PanelPlacement
        {
            SheetNumber = sheet.SheetNumber,
            PanelIndex = panel.Index,
            X = freeRect.X,
            Y = freeRect.Y,
            Length = placedLength,
            Width = placedWidth,
            IsRotated = isRotated,
            Panel = panel
        });

        sheet.FreeRectangles.RemoveAt(freeRectIndex);

        var rightLength = freeRect.Length - placedLength - kerf;
        var bottomWidth = freeRect.Width - placedWidth - kerf;

        if (rightLength > 0)
        {
            sheet.FreeRectangles.Add(new FreeRectangle
            {
                X = freeRect.X + placedLength + kerf,
                Y = freeRect.Y,
                Length = rightLength,
                Width = placedWidth
            });
        }

        if (bottomWidth > 0)
        {
            sheet.FreeRectangles.Add(new FreeRectangle
            {
                X = freeRect.X,
                Y = freeRect.Y + placedWidth + kerf,
                Length = freeRect.Length,
                Width = bottomWidth
            });
        }
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