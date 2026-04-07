namespace MazeEscapeGame.Models
{
    public enum TileType
    {
        Wall,
        Path,
        Start,
        Exit,
        LockedExit,   // exit before key is collected
        Key,          // unlocks the exit
        Trap,         // costs the player 10 seconds
        Coin          // adds 50 bonus score
    }
}
