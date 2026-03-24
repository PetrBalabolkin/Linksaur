using UnityEngine;
using System;

namespace Linksaurus.Core
{
    public class AdManager : MonoBehaviour
    {
        public static AdManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void ShowInterstitialAd()
        {
            Debug.Log(">>> INTERSTITIAL AD <<<");
        }

        public void ShowRewardedAd(Action onComplete)
        {
            // Simulated delay for ad
            Debug.Log(">>> REWARDED AD — reward granted <<<");
            onComplete?.Invoke();
        }
    }
}
