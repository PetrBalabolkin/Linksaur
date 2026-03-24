using UnityEngine;
using Linksaurus.Data;
using Linksaurus.Core;

namespace Linksaurus.Systems
{
    public class SkinManager : MonoBehaviour
    {
        public static SkinManager Instance { get; private set; }

        public SkinData[] allSkins;
        public int currentSkinIndex;

        [SerializeField] private SpriteRenderer _playerRenderer;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                currentSkinIndex = PlayerPrefs.GetInt("CurrentSkinIndex", 0);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            GameManager.OnGameStart += ApplySkin;
        }

        private void OnDestroy()
        {
            GameManager.OnGameStart -= ApplySkin;
        }

        public SkinData GetCurrentSkin()
        {
            if (currentSkinIndex >= 0 && currentSkinIndex < allSkins.Length)
                return allSkins[currentSkinIndex];
            return null;
        }

        public void UnlockSkin(int index)
        {
            // Placeholder logic
            PlayerPrefs.SetInt($"SkinUnlocked_{index}", 1);
            PlayerPrefs.Save();
        }

        private void ApplySkin()
        {
            SkinData skin = GetCurrentSkin();
            if (skin != null && _playerRenderer != null)
            {
                _playerRenderer.color = skin.tintColor;
                if (skin.skinSprite != null) _playerRenderer.sprite = skin.skinSprite;
            }
        }
    }
}
