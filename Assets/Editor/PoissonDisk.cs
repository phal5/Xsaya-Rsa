using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Bridson 알고리즘 기반 Poisson Disk Sampling. 최소 간격을 지키면서 자연스러운 분포를 만든다.
///
/// EnvironmentPlacerWindow가 같은 알고리즘을 자기 안에 들고 있다. 그쪽은 잘 돌고 있어 건드리지 않았다.
/// 나중에 정리한다면 이 클래스를 부르도록 바꾸면 된다.
/// </summary>
public static class PoissonDisk
{
    /// <summary>
    /// 사각 영역에 최소 간격 minDist를 지키는 점들을 뿌린다.
    /// </summary>
    /// <param name="shouldCancel">
    /// 256회마다 불린다. true를 돌려주면 빈 목록을 반환하고 즉시 빠져나온다.
    /// 언제 끝날지 모르는 루프라 취소 통로를 열어둔다. 필요 없으면 null.
    /// </param>
    public static List<Vector2> Sample(
        Vector2 regionMin,
        Vector2 regionMax,
        float minDist,
        int maxAttempts,
        System.Random rng,
        Func<int, bool> shouldCancel = null)
    {
        List<Vector2> points = new List<Vector2>();

        if (minDist <= 0f || rng == null) return points;

        Vector2 regionSize = regionMax - regionMin;
        if (regionSize.x <= 0f || regionSize.y <= 0f) return points;

        float cellSize = minDist / Mathf.Sqrt(2f);

        int gridWidth = Mathf.CeilToInt(regionSize.x / cellSize);
        int gridHeight = Mathf.CeilToInt(regionSize.y / cellSize);
        if (gridWidth <= 0 || gridHeight <= 0) return points;

        int[,] grid = new int[gridWidth, gridHeight];
        for (int x = 0; x < gridWidth; x++)
            for (int y = 0; y < gridHeight; y++)
                grid[x, y] = -1;

        List<int> activeList = new List<int>();

        Vector2 startPoint = new Vector2(
            regionMin.x + (float)rng.NextDouble() * regionSize.x,
            regionMin.y + (float)rng.NextDouble() * regionSize.y
        );

        AddPoint(startPoint, points, activeList, grid, regionMin, cellSize);

        int sinceLastPoll = 0;

        while (activeList.Count > 0)
        {
            if (shouldCancel != null && ++sinceLastPoll >= 256)
            {
                sinceLastPoll = 0;
                if (shouldCancel(points.Count)) return new List<Vector2>();
            }

            int activeIndex = rng.Next(activeList.Count);
            Vector2 center = points[activeList[activeIndex]];
            bool found = false;

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                float angle = (float)rng.NextDouble() * Mathf.PI * 2f;
                float radius = minDist + (float)rng.NextDouble() * minDist;
                Vector2 candidate = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;

                if (candidate.x < regionMin.x || candidate.x >= regionMax.x ||
                    candidate.y < regionMin.y || candidate.y >= regionMax.y)
                    continue;

                if (IsValidPoint(candidate, points, grid, regionMin, cellSize, minDist, gridWidth, gridHeight))
                {
                    AddPoint(candidate, points, activeList, grid, regionMin, cellSize);
                    found = true;
                    break;
                }
            }

            if (!found) activeList.RemoveAt(activeIndex);
        }

        return points;
    }

    static void AddPoint(Vector2 point, List<Vector2> points, List<int> activeList, int[,] grid, Vector2 regionMin, float cellSize)
    {
        int gridX = Mathf.FloorToInt((point.x - regionMin.x) / cellSize);
        int gridY = Mathf.FloorToInt((point.y - regionMin.y) / cellSize);

        if (gridX >= 0 && gridX < grid.GetLength(0) && gridY >= 0 && gridY < grid.GetLength(1))
        {
            grid[gridX, gridY] = points.Count;
        }

        activeList.Add(points.Count);
        points.Add(point);
    }

    static bool IsValidPoint(Vector2 candidate, List<Vector2> points, int[,] grid, Vector2 regionMin, float cellSize, float minDist, int gridWidth, int gridHeight)
    {
        int gridX = Mathf.FloorToInt((candidate.x - regionMin.x) / cellSize);
        int gridY = Mathf.FloorToInt((candidate.y - regionMin.y) / cellSize);

        int startX = Mathf.Max(0, gridX - 2);
        int endX = Mathf.Min(gridWidth - 1, gridX + 2);
        int startY = Mathf.Max(0, gridY - 2);
        int endY = Mathf.Min(gridHeight - 1, gridY + 2);

        float minDistSqr = minDist * minDist;

        for (int x = startX; x <= endX; x++)
        {
            for (int y = startY; y <= endY; y++)
            {
                int neighbor = grid[x, y];
                if (neighbor < 0) continue;

                if ((candidate - points[neighbor]).sqrMagnitude < minDistSqr) return false;
            }
        }

        return true;
    }
}
