using System.Collections.Generic;
using System.Linq;

public sealed class ExplosionResolution
{
    public HashSet<Coordinate> DestroyedCoordinates { get; } = new();
    public HashSet<Coordinate> DetonatedBombCoordinates { get; } = new();
}

public static class ExplosionResolver
{
    public static ExplosionResolution Resolve(
        IEnumerable<Coordinate> occupiedCoordinates,
        IEnumerable<Coordinate> bombCoordinates,
        IEnumerable<Coordinate> seedBombCoordinates,
        int radius,
        bool chainExplosion)
    {
        var occupied = new HashSet<Coordinate>(occupiedCoordinates);
        var bombs = new HashSet<Coordinate>(bombCoordinates);
        var pendingBombs = new Queue<Coordinate>(seedBombCoordinates.Where(bombs.Contains));
        var result = new ExplosionResolution();
        int clampedRadius = System.Math.Max(1, radius);

        while (pendingBombs.Count > 0)
        {
            Coordinate bomb = pendingBombs.Dequeue();
            if (!result.DetonatedBombCoordinates.Add(bomb))
                continue;

            foreach (Coordinate target in occupied)
            {
                if (HexDistance(bomb, target) > clampedRadius)
                    continue;

                result.DestroyedCoordinates.Add(target);
                if (chainExplosion && bombs.Contains(target) &&
                    !result.DetonatedBombCoordinates.Contains(target))
                {
                    pendingBombs.Enqueue(target);
                }
            }
        }

        return result;
    }

    public static int HexDistance(Coordinate a, Coordinate b)
    {
        return (a - b).CircleRadius;
    }
}
