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
                _worldWidth = sr.bounds.size.x * transform.localScale.x;
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
                // If it's a background layer, loop it
                if (transform.parent != null && transform.parent.name == "Background")
                {
                    // Reposition to the right of its current position by exactly twice the width
                    transform.position += Vector3.right * (_worldWidth * 2f); 
                }
                else
                {
                    // It's a pooled obstacle/item
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
