using System;
using System.IO;

namespace MazeEscapeGame.Core
{
    public static class HighScoreManager
    {
        private const string FileName = "highscore.txt";

        public static int HighScore { get; private set; }

        public static void Load()
        {
            try
            {
                if (File.Exists(FileName) &&
                    int.TryParse(File.ReadAllText(FileName).Trim(), out int saved))
                    HighScore = saved;
            }
            catch { HighScore = 0; }
        }

        public static bool TrySave(int score)
        {
            if (score <= HighScore) return false;
            HighScore = score;
            try { File.WriteAllText(FileName, score.ToString()); }
            catch { }
            return true;
        }
    }
}
