using UnityEngine;

namespace Linksaurus.Core
{
    public static class SaveManager
    {
        public static void Save()
        {
            PlayerPrefs.SetInt("HighScore", GameManager.Instance.HighScore);
            // Other data: "GamesPlayed", "PlayerLevel", "TotalConnections", "CurrentSkinIndex", "CurrentBgIndex", "DailyRewardDate"
            // For now, only HighScore is tracked.
            PlayerPrefs.Save();
        }

        public static void Load()
        {
            GameManager.Instance.HighScore = PlayerPrefs.GetInt("HighScore", 0);
        }
    }
}
