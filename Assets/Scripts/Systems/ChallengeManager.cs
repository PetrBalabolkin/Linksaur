using UnityEngine;
using Linksaurus.Data;
using Linksaurus.Core;
using System;

namespace Linksaurus.Systems
{
    public class ChallengeManager : MonoBehaviour
    {
        public static ChallengeManager Instance { get; private set; }

        public ChallengeData[] allChallenges;
        private ChallengeData _currentChallenge;
        private int _currentProgress;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                LoadDailyChallenge();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            GameManager.OnGameOver += CheckChallenge;
        }

        private void OnDestroy()
        {
            GameManager.OnGameOver -= CheckChallenge;
        }

        private void LoadDailyChallenge()
        {
            // Rotate daily based on date
            int dayOfYear = DateTime.Now.DayOfYear;
            if (allChallenges.Length > 0)
            {
                _currentChallenge = allChallenges[dayOfYear % allChallenges.Length];
            }
        }

        private void CheckChallenge()
        {
            if (_currentChallenge == null) return;

            // Simple check logic based on types
            bool complete = false;
            switch (_currentChallenge.type)
            {
                case ChallengeType.Collect:
                    if (GameManager.Instance.CurrentScore >= _currentChallenge.targetValue) complete = true;
                    break;
                case ChallengeType.Survive:
                    // Would need a timer in GameManager or here
                    break;
                case ChallengeType.NoPowerup:
                    // Would need tracker in GameManager
                    break;
            }

            if (complete)
            {
                Debug.Log($"CHALLENGE COMPLETE: {_currentChallenge.description}");
                GameManager.Instance.AddConnections(50); // Bonus
            }
        }
    }
}
