using UnityEngine;

namespace Linksaurus.Data
{
    public enum ChallengeType { Collect, Survive, NoPowerup }

    [CreateAssetMenu(fileName = "NewChallenge", menuName = "Linksaurus/Challenge")]
    public class ChallengeData : ScriptableObject
    {
        public string description;
        public int targetValue;
        public ChallengeType type;
    }
}
