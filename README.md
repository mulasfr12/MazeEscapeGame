# Maze Escape Game

A 2D top-down maze escape game built with **MonoGame (.NET 8)**. Every run generates a brand-new maze using a procedural algorithm. Explore the darkness, find the key, dodge enemies and traps, and reach the exit before time runs out.

---

## Gameplay

Each level drops you into a randomly generated maze. You must:

1. Explore the maze under **fog of war** — only the area around you is visible
2. Find the **cyan key** tile to unlock the exit
3. Reach the **yellow exit** tile to advance to the next level
4. Avoid **purple traps** (cost 10 seconds each) and **red enemies**
5. Collect **gold coins** for bonus score

If the timer hits zero or an enemy catches you, it's game over.

---

## Tile Legend

| Colour | Tile | Effect |
|---|---|---|
| Blue | Player | You |
| Green | Start | Where you spawn |
| Yellow (pulsing) | Exit | Reach this to escape — requires key |
| Orange | Locked Exit | Exit before collecting the key |
| Cyan (shimmering) | Key | Collect to unlock the exit |
| Purple | Trap | −10 seconds on contact |
| Gold | Coin | +50 score on collection |
| Red | Enemy | Game over on contact |
| Dark navy | Wall | Impassable |
| Warm grey | Path | Walkable floor |

---

## Controls

| Key | Action |
|---|---|
| `W` / `↑` | Move up |
| `S` / `↓` | Move down |
| `A` / `←` | Move left |
| `D` / `→` | Move right |
| `H` | Toggle in-game legend |
| `ESC` | Quit |

---

## Scoring

| Source | Points |
|---|---|
| Time remaining on exit | `seconds × 10` |
| Level completion bonus | `level × 100` |
| Coin collected | `+50` |

Score accumulates across levels. Your best run is saved to `highscore.txt` beside the executable.

---

## Level Progression

| Stat | Formula |
|---|---|
| Maze size | `(2n+1) × (2n+1)` where `n = 6 + level × 2` |
| Time limit | `45 + level × 15` seconds |
| Traps | `level + 1` |
| Enemies | `min(level − 1, 3)` — none on level 1 |
| Coins | 3 per level |

The maze grows each level. From level 3 onward the camera follows the player as the maze exceeds the window size.

---

## Project Structure

```
MazeEscapeGame/
├── Content/
│   ├── Content.mgcb          # MonoGame content pipeline
│   └── DefaultFont.spritefont
├── Core/
│   ├── Enemy.cs              # Roaming enemy (random walk)
│   ├── HighScoreManager.cs   # Persist best score to disk
│   ├── InputHandler.cs       # Keyboard state (pressed vs held)
│   ├── LevelManager.cs       # Orchestrates maze, player, items, timer, score
│   ├── MazeGenerator.cs      # Iterative DFS backtracker + BFS exit placement
│   ├── MazeGrid.cs           # Tile array, fog-of-war reveal, walkability
│   └── Player.cs             # Grid-aligned movement with wall collision
├── Enums/
│   └── Direction.cs
├── Models/
│   ├── GameSettings.cs       # Window size, tile size, fog radius constants
│   ├── GameState.cs          # Start / Playing / LevelComplete / GameOver
│   ├── Position.cs           # Value-type (X, Y) with operator overloads
│   └── TileType.cs           # Wall, Path, Start, Exit, LockedExit, Key, Trap, Coin
├── Rendering/
│   └── MazeRenderer.cs       # Fog of war, animated tiles, camera, 1px tile gap
├── Game1.cs                  # Game loop, state machine, HUD, screen shake
└── Program.cs
```

---

## Maze Generation

The maze is built with **iterative randomised depth-first search** (backtracking):

1. Start with a grid of all walls
2. Pick a starting cell, mark it visited, push to stack
3. While the stack is not empty:
   - If the current cell has unvisited neighbours, carve through the wall to a random one and push it
   - Otherwise pop the stack (backtrack)
4. Place the **start** at cell `(0, 0)`
5. Run a **BFS** from start — the last node reached is the furthest reachable tile; place the **exit** there

This guarantees every cell is reachable and the exit is always as far from the start as possible.

---

## Tech Stack

- **Language:** C# 12 / .NET 8
- **Framework:** MonoGame 3.8 (DesktopGL)
- **IDE:** Visual Studio 2022

---

## Building & Running

```bash
# Restore tools (MonoGame content pipeline)
dotnet tool restore

# Build and run
dotnet run --project MazeEscapeGame/MazeEscapeGame/MazeEscapeGame.csproj
```

Or open `MazeEscapeGame/MazeEscapeGame.sln` in Visual Studio and press **F5**.
