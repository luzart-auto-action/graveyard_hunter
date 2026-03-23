using System.Collections.Generic;
using GraveyardHunter.Core;
using UnityEngine;

namespace GraveyardHunter.Level
{
    public static class MazeGenerator
    {
        private static readonly int[] _dirX = { 0, 0, -1, 1 };
        private static readonly int[] _dirY = { -1, 1, 0, 0 };

        public static CellType[,] Generate(int width, int height, int seed = -1)
        {
            // Ensure odd dimensions for proper maze walls
            if (width % 2 == 0) width++;
            if (height % 2 == 0) height++;

            var grid = new CellType[width, height];

            // Fill everything with walls
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    grid[x, y] = CellType.Wall;
                }
            }

            // Seed the random state
            Random.State previousState = Random.state;
            if (seed >= 0)
            {
                Random.InitState(seed);
            }
            else
            {
                Random.InitState(System.Environment.TickCount);
            }

            // Carve the maze starting from (1,1)
            CarvePassage(grid, 1, 1, width, height);

            // Ensure border is all walls
            for (int x = 0; x < width; x++)
            {
                grid[x, 0] = CellType.Wall;
                grid[x, height - 1] = CellType.Wall;
            }
            for (int y = 0; y < height; y++)
            {
                grid[0, y] = CellType.Wall;
                grid[width - 1, y] = CellType.Wall;
            }

            // Set player spawn at (1,1)
            grid[1, 1] = CellType.PlayerSpawn;

            // Set exit gate at opposite corner area
            int exitX = width - 2;
            int exitY = height - 2;

            // Make sure exit position is odd (valid maze cell)
            if (exitX % 2 == 0) exitX--;
            if (exitY % 2 == 0) exitY--;

            grid[exitX, exitY] = CellType.ExitGate;

            // Restore random state
            Random.state = previousState;

            return grid;
        }

        public static List<Vector2Int> GetEmptyCells(CellType[,] grid)
        {
            var emptyCells = new List<Vector2Int>();
            int width = grid.GetLength(0);
            int height = grid.GetLength(1);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (grid[x, y] == CellType.Empty)
                    {
                        emptyCells.Add(new Vector2Int(x, y));
                    }
                }
            }

            return emptyCells;
        }

        private static void CarvePassage(CellType[,] grid, int x, int y, int width, int height)
        {
            grid[x, y] = CellType.Empty;

            // Shuffle directions
            int[] directions = { 0, 1, 2, 3 };
            for (int i = directions.Length - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (directions[i], directions[j]) = (directions[j], directions[i]);
            }

            for (int i = 0; i < 4; i++)
            {
                int dir = directions[i];

                // Move 2 cells in the chosen direction (thick wall pattern)
                int nx = x + _dirX[dir] * 2;
                int ny = y + _dirY[dir] * 2;

                // Check bounds (stay inside border)
                if (nx < 1 || nx >= width - 1 || ny < 1 || ny >= height - 1)
                    continue;

                if (grid[nx, ny] == CellType.Wall)
                {
                    // Carve the wall between current cell and next cell
                    int wallX = x + _dirX[dir];
                    int wallY = y + _dirY[dir];
                    grid[wallX, wallY] = CellType.Empty;

                    CarvePassage(grid, nx, ny, width, height);
                }
            }
        }
    }
}
