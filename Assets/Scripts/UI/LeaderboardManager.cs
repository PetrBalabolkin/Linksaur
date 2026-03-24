using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Linksaurus.UI
{
    [Serializable]
    public class LeaderboardData
    {
        public List<int> scores = new List<int>();
    }

    public class LeaderboardManager : MonoBehaviour
    {
        public static LeaderboardManager Instance { get; private set; }

        private const string KEY = "TopScores";
        private LeaderboardData _data;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                LoadScores();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void TryAddScore(int score)
        {
            _data.scores.Add(score);
            _data.scores.Sort((a, b) => b.CompareTo(a)); // Descending
            if (_data.scores.Count > 10)
            {
                _data.scores.RemoveRange(10, _data.scores.Count - 10);
            }
            SaveScores();
        }

        public List<int> GetTopScores()
        {
            return _data.scores;
        }

        private void LoadScores()
        {
            string json = PlayerPrefs.GetString(KEY, "");
            if (string.IsNullOrEmpty(json))
            {
                _data = new LeaderboardData();
            }
            else
            {
                _data = JsonUtility.FromJson<LeaderboardData>(json);
            }
        }

        private void SaveScores()
        {
            string json = JsonUtility.ToJson(_data);
            PlayerPrefs.SetString(KEY, json);
            PlayerPrefs.Save();
        }
    }
}
