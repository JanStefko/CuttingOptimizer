using CuttingOptimizer.Models.Optimization;

namespace CuttingOptimizer.Services.Cutting;

/// <summary>
/// Simple First-Fit cutting algorithm.
/// Panels are placed onto the first sheet where they fit; if none fits, a new sheet is opened.
/// Panels should be pre-sorted by the caller (typically descending by area).
/// </summary>
public class FirstFitCuttingAlgorithm : ICuttingAlgorithm
{
    public OptimizationResult Cut(
        double usableLength,
        double usableWidth,
        double kerf,
        List<OptimizationPanel> panels)
    {
        if (usableLength <= 0 || usableWidth <= 0)
            throw new InvalidOperationException("Usable sheet area must be positive.");

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
}