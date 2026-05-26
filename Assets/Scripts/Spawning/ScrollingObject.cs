using UnityEngine;
using Linksaurus.Core;

namespace Linksaurus.Spawning
{
    public class ScrollingObject : MonoBehaviour
    {
        [SerializeField] private float _speedMultiplier = 1f;
        private float _worldWidth;
        private float _screenHalfWidth;

        private void OnEnable()
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null && sr.sprite != null)
            {
                if (sr.drawMode == SpriteDrawMode.Tiled)
                {
                    _worldWidth = sr.size.x * transform.localScale.x;
                }
                else
                {
                    _worldWidth = sr.sprite.bounds.size.x * transform.localScale.x;
                }
            }
            else
            {
                _worldWidth = 1f;
            }

            if (Camera.main != null)
            {
                _screenHalfWidth = Camera.main.orthographicSize * Camera.main.aspect;
            }
        }

        private void Update()
        {
            if (GameManager.Instance.CurrentState != GameState.Playing) return;

            transform.position += Vector3.left * GameManager.Instance.ScrollSpeed * _speedMultiplier * Time.deltaTime;

            if (transform.position.x < -_screenHalfWidth - _worldWidth)
            {
                // Check if it's a background layer (parent named "Background")
                if (transform.parent != null && transform.parent.name == "Background")
                {
                    // Find total number of siblings to wrap correctly
                    int siblingCount = 0;
                    foreach (Transform child in transform.parent)
                    {
                        if (child.name.StartsWith(gameObject.name.Split('_')[0])) // Match "Layer" or "Road"
                        {
                            siblingCount++;
                        }
                    }
                    
                    if (siblingCount == 0) siblingCount = 2; // Fallback
                    
                    transform.position += Vector3.right * (_worldWidth * siblingCount);
                }
                else
                {
                    // It's a pooled obstacle/item, return to pool
                    ObjectPool.Instance.ReturnToPool(gameObject);
                }
            }
        }

        public void SetSpeedMultiplier(float multiplier)
        {
            _speedMultiplier = multiplier;
        }
    }
}
