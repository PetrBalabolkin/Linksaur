using UnityEngine;
using Linksaurus.Core;

namespace Linksaurus.Spawning
{
    public class ScrollingObject : MonoBehaviour
    {
        [SerializeField] private float _speedMultiplier = 1f;
        private float _worldWidth;
        private float _screenHalfWidth;

        private void Start()
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null && sr.sprite != null)
            {
                // Use sprite.bounds.size (not sr.bounds which is in world space!)
                _worldWidth = sr.sprite.bounds.size.x * transform.localScale.x;
            }
            else
            {
                _worldWidth = 1f;
            }

            _screenHalfWidth = Camera.main.orthographicSize * Camera.main.aspect;
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
                    // For background tiles, wrap by 2x the width to account for paired tile
                    transform.position += Vector3.right * (_worldWidth * 2f);
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
