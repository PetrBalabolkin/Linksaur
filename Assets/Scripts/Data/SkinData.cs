using UnityEngine;

namespace Linksaurus.Data
{
    [CreateAssetMenu(fileName = "NewSkin", menuName = "Linksaurus/Skin")]
    public class SkinData : ScriptableObject
    {
        public string skinName;
        public Sprite skinSprite;
        public Color tintColor;
        public int unlockLevel;
        public bool isPremium;
    }
}
