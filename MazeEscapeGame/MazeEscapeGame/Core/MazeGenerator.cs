using MazeEscapeGame.Models;
using System;
using System.Collections.Generic;

namespace MazeEscapeGame.Core
{
    // Generates a perfect maze using iterative randomised depth-first search
    // (iterative backtracking), avoiding recursive stack overflows on large grids.
    //
    // Algorithm operates in "cell space" (n x n) and carves into the tile grid
    // ((2n+1) x (2n+1)):
    //   Cell (cx, cy)  →  tile (2*cx+1, 2*cy+1)
    //   Wall between (cx,cy) and neighbour (nx,ny) → tile at the midpoint
    public class MazeGenerator
    {
        private readonly Random _random;

        private static readonly (int dx, int dy)[] Directions =
        {
            ( 0, -1), // up
            ( 0,  1), // down
            (-1,  0), // left
            ( 1,  0), // right
        };

        public MazeGenerator(Random random = null)
        {
            _random = random ?? new Random();
        }

        public void Generate(MazeGrid grid)
        {
            int n = (grid.Width - 1) / 2;
            var visited = new bool[n, n];

            // --- Iterative DFS backtracking ---
            var stack = new Stack<(int cx, int cy)>();

            CarveCell(grid, 0, 0);
            visited[0, 0] = true;
            stack.Push((0, 0));

            while (stack.Count > 0)
            {
                var (cx, cy) = stack.Peek();
                var neighbours = GetUnvisitedNeighbours(cx, cy, n, visited);

                if (neighbours.Count == 0)
                {
                    stack.Pop();
                    continue;
                }

                var (nx, ny) = neighbours[_random.Next(neighbours.Count)];

                // Carve the wall tile that sits between the two cells
                int wallTileX = 2 * cx + 1 + (nx - cx); // midpoint x in tile space
                int wallTileY = 2 * cy + 1 + (ny - cy); // midpoint y in tile space
                grid.SetTile(wallTileX, wallTileY, TileType.Path);

                CarveCell(grid, nx, ny);
                visited[nx, ny] = true;
                stack.Push((nx, ny));
            }

            // --- Place start at top-left cell ---
            var startPos = new Position(1, 1);
            grid.SetTile(startPos, TileType.Start);
            grid.StartPosition = startPos;

            // --- Place exit at the cell furthest from start (BFS) ---
            var exitPos = FindFurthestWalkable(grid, startPos);
            grid.SetTile(exitPos, TileType.Exit);
            grid.ExitPosition = exitPos;
        }

        // -------------------------------------------------------------------------

        private static void CarveCell(MazeGrid grid, int cx, int cy)
        {
            grid.SetTile(2 * cx + 1, 2 * cy + 1, TileType.Path);
        }

        private static List<(int cx, int cy)> GetUnvisitedNeighbours(
            int cx, int cy, int n, bool[,] visited)
        {
            var list = new List<(int, int)>(4);
            foreach (var (dx, dy) in Directions)
            {
                int nx = cx + dx;
                int ny = cy + dy;
                if (nx >= 0 && nx < n && ny >= 0 && ny < n && !visited[nx, ny])
                    list.Add((nx, ny));
            }
            return list;
        }

        // BFS from start; the last node dequeued is one of the furthest reachable
        // tiles — guaranteed because BFS visits nodes in non-decreasing distance order.
        private static Position FindFurthestWalkable(MazeGrid grid, Position start)
        {
            var queue   = new Queue<Position>();
            var visited = new HashSet<Position>();

            queue.Enqueue(start);
            visited.Add(start);

            Position furthest = start;

            while (queue.Count > 0)
            {
                var pos = queue.Dequeue();
                furthest = pos;

                foreach (var (dx, dy) in Directions)
                {
                    var next = new Position(pos.X + dx, pos.Y + dy);
                    if (!visited.Contains(next) && grid.IsWalkable(next))
                    {
                        visited.Add(next);
                        queue.Enqueue(next);
                    }
                }
            }

            return furthest;
        }
    }
}
