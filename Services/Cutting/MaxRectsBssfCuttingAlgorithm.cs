using CuttingOptimizer.Models.Optimization;

namespace CuttingOptimizer.Services.Cutting;

/// <summary>
/// MaxRects algorithm with Best Short Side Fit (BSSF) heuristic.
/// 
/// Maintains all maximal free rectangles (which may overlap each other) and for each panel
/// picks the placement that leaves the smallest leftover on the shorter side.
/// 
/// Reference: Jukka Jylänki, "A Thousand Ways to Pack the Bin" (2010).
/// </summary>
public class MaxRectsBssfCuttingAlgorithm : ICuttingAlgorithm
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
            var placed = TryPlaceOnExistingSheets(result.Sheets, panel, kerf);

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

    private static bool TryPlaceOnExistingSheets(
        List<OptimizationSheet> sheets,
        OptimizationPanel panel,
        double kerf)
    {
        foreach (var sheet in sheets)
        {
            if (TryPlacePanelOnSheet(sheet, panel, kerf))
                return true;
        }

        return false;
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
        var bestChoice = FindBestPlacement(sheet.FreeRectangles, panel);

        if (bestChoice is null)
            return false;

        PlacePanel(sheet, panel, bestChoice.Value, kerf);
        return true;
    }

    /// <summary>
    /// Goes through all free rectangles and finds the one where the panel fits with
    /// the smallest leftover on its shorter side. Tries both orientations when allowed.
    /// </summary>
    private static PlacementChoice? FindBestPlacement(
        List<FreeRectangle> freeRectangles,
        OptimizationPanel panel)
    {
        PlacementChoice? best = null;
        var bestShortSide = double.MaxValue;
        var bestLongSide = double.MaxValue;

        foreach (var freeRect in freeRectangles)
        {
            EvaluateFit(freeRect, panel.CutLength, panel.CutWidth, false,
                ref best, ref bestShortSide, ref bestLongSide);

            if (panel.CanRotate)
            {
                EvaluateFit(freeRect, panel.CutWidth, panel.CutLength, true,
                    ref best, ref bestShortSide, ref bestLongSide);
            }
        }

        return best;
    }

    private static void EvaluateFit(
        FreeRectangle freeRect,
        double placedLength,
        double placedWidth,
        bool isRotated,
        ref PlacementChoice? best,
        ref double bestShortSide,
        ref double bestLongSide)
    {
        if (!freeRect.CanFit(placedLength, placedWidth))
            return;

        var leftoverHorizontal = freeRect.Length - placedLength;
        var leftoverVertical = freeRect.Width - placedWidth;

        var shortSide = Math.Min(leftoverHorizontal, leftoverVertical);
        var longSide = Math.Max(leftoverHorizontal, leftoverVertical);

        // BSSF: pick the placement with smallest short-side leftover.
        // Tie-break by long-side leftover (BLSF as secondary).
        if (shortSide < bestShortSide || (shortSide == bestShortSide && longSide < bestLongSide))
        {
            best = new PlacementChoice
            {
                FreeRect = freeRect,
                PlacedLength = placedLength,
                PlacedWidth = placedWidth,
                IsRotated = isRotated
            };
            bestShortSide = shortSide;
            bestLongSide = longSide;
        }
    }

    private static void PlacePanel(
        OptimizationSheet sheet,
        OptimizationPanel panel,
        PlacementChoice choice,
        double kerf)
    {
        var placedX = choice.FreeRect.X;
        var placedY = choice.FreeRect.Y;

        sheet.Placements.Add(new PanelPlacement
        {
            SheetNumber = sheet.SheetNumber,
            PanelIndex = panel.Index,
            X = placedX,
            Y = placedY,
            Length = choice.PlacedLength,
            Width = choice.PlacedWidth,
            IsRotated = choice.IsRotated,
            Panel = panel
        });

        UpdateFreeRectangles(sheet, placedX, placedY, choice.PlacedLength, choice.PlacedWidth, kerf);
    }

    /// <summary>
    /// Core MaxRects step: split every free rectangle that intersects with the placed panel
    /// (plus kerf) into up to 4 new maximal rectangles, then prune any that are fully
    /// contained within another.
    /// </summary>
    private static void UpdateFreeRectangles(
        OptimizationSheet sheet,
        double placedX,
        double placedY,
        double placedLength,
        double placedWidth,
        double kerf)
    {
        // The occupied area includes kerf on the right and bottom so no other panel
        // can be placed touching the cut edge.
        var occupiedRight = placedX + placedLength + kerf;
        var occupiedBottom = placedY + placedWidth + kerf;

        var newRectangles = new List<FreeRectangle>();

        foreach (var freeRect in sheet.FreeRectangles)
        {
            if (!IntersectsOccupied(freeRect, placedX, placedY, occupiedRight, occupiedBottom))
            {
                // No intersection — keep the rectangle as-is.
                newRectangles.Add(freeRect);
                continue;
            }

            AddSplitRectangles(newRectangles, freeRect, placedX, placedY, occupiedRight, occupiedBottom);
        }

        sheet.FreeRectangles = PruneContainedRectangles(newRectangles);
    }

    private static bool IntersectsOccupied(
        FreeRectangle freeRect,
        double occupiedLeft,
        double occupiedTop,
        double occupiedRight,
        double occupiedBottom)
    {
        var freeRight = freeRect.X + freeRect.Length;
        var freeBottom = freeRect.Y + freeRect.Width;

        // Standard AABB intersection: NOT (one is fully to the side of the other).
        return !(occupiedLeft >= freeRight
              || occupiedRight <= freeRect.X
              || occupiedTop >= freeBottom
              || occupiedBottom <= freeRect.Y);
    }

    /// <summary>
    /// Splits a free rectangle around the occupied area into up to four maximal pieces:
    /// strip above, strip below, strip left, strip right. Each piece spans the full
    /// extent of the original free rectangle in the orthogonal direction.
    /// </summary>
    private static void AddSplitRectangles(
        List<FreeRectangle> output,
        FreeRectangle freeRect,
        double occupiedLeft,
        double occupiedTop,
        double occupiedRight,
        double occupiedBottom)
    {
        // Strip above the occupied area.
        if (occupiedTop > freeRect.Y)
        {
            output.Add(new FreeRectangle
            {
                X = freeRect.X,
                Y = freeRect.Y,
                Length = freeRect.Length,
                Width = occupiedTop - freeRect.Y
            });
        }

        // Strip below the occupied area.
        var freeBottom = freeRect.Y + freeRect.Width;
        if (occupiedBottom < freeBottom)
        {
            output.Add(new FreeRectangle
            {
                X = freeRect.X,
                Y = occupiedBottom,
                Length = freeRect.Length,
                Width = freeBottom - occupiedBottom
            });
        }

        // Strip to the left of the occupied area.
        if (occupiedLeft > freeRect.X)
        {
            output.Add(new FreeRectangle
            {
                X = freeRect.X,
                Y = freeRect.Y,
                Length = occupiedLeft - freeRect.X,
                Width = freeRect.Width
            });
        }

        // Strip to the right of the occupied area.
        var freeRight = freeRect.X + freeRect.Length;
        if (occupiedRight < freeRight)
        {
            output.Add(new FreeRectangle
            {
                X = occupiedRight,
                Y = freeRect.Y,
                Length = freeRight - occupiedRight,
                Width = freeRect.Width
            });
        }
    }

    /// <summary>
    /// Removes any free rectangle that is fully contained within another, keeping
    /// only the maximal ones. Without this step the list would grow unboundedly.
    /// </summary>
    private static List<FreeRectangle> PruneContainedRectangles(List<FreeRectangle> rectangles)
    {
        var result = new List<FreeRectangle>();

        for (var i = 0; i < rectangles.Count; i++)
        {
            var isContained = false;

            for (var j = 0; j < rectangles.Count; j++)
            {
                if (i == j)
                    continue;

                if (IsContainedIn(rectangles[i], rectangles[j]))
                {
                    isContained = true;
                    break;
                }
            }

            if (!isContained)
                result.Add(rectangles[i]);
        }

        return result;
    }

    private static bool IsContainedIn(FreeRectangle inner, FreeRectangle outer)
    {
        return inner.X >= outer.X
            && inner.Y >= outer.Y
            && inner.X + inner.Length <= outer.X + outer.Length
            && inner.Y + inner.Width <= outer.Y + outer.Width;
    }

    private readonly struct PlacementChoice
    {
        public FreeRectangle FreeRect { get; init; }
        public double PlacedLength { get; init; }
        public double PlacedWidth { get; init; }
        public bool IsRotated { get; init; }
    }
}