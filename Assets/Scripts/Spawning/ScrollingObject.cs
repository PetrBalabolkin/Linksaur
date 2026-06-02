using UnityEngine;
using Linksaurus.Core;

namespace Linksaurus.Spawning
{
    public class ScrollingObject : MonoBehaviour
    {
        [SerializeField] private float _speedMultiplier = 1f;
        private float _worldWidth;
        private float _screenHalfWidth;
        private int _groupCount = 1;

        private float _accumulatedDistance = 0f;
        private Vector3 _startPosition;
        private bool _isBackground;
        private bool _initialized;

        private void OnEnable()
        {
            _initialized = false;
            _accumulatedDistance = 0f;

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

            _isBackground = transform.parent != null && transform.parent.name == "Background";
            
            // For background objects, we can calculate group count immediately if they aren't spawned dynamically
            if (_isBackground)
            {
                CalculateGroupCount();
            }
        }

        private void CalculateGroupCount()
        {
            if (transform.parent == null)
            {
                _groupCount = 1;
                return;
            }

            string prefix = GetGroupPrefix();
            int count = 0;
            foreach (Transform child in transform.parent)
            {
                if (child.name.StartsWith(prefix))
                    count++;
            }
            _groupCount = count;
        }

        private void LateUpdate()
        {
            if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing) return;

            // Capture start position on the first frame of movement.
            // This is crucial for pooled objects whose position is set AFTER OnEnable.
            if (!_initialized)
            {
                _startPosition = transform.position;
                _initialized = true;
            }

            // Use smoothDeltaTime for background layers (parallax) to reduce jitter, 
            // but use regular deltaTime for foreground (multiplier >= 1) to keep gameplay sync.
            float dt = _speedMultiplier < 1f ? Time.smoothDeltaTime : Time.deltaTime;
            _accumulatedDistance += GameManager.Instance.ScrollSpeed * _speedMultiplier * dt;

            if (_isBackground)
            {
                float totalWidth = _worldWidth * _groupCount;
                float currentX = _startPosition.x - (_accumulatedDistance % totalWidth);

                // If the calculated position is too far left, wrap it around the group width
                if (currentX < -_screenHalfWidth - _worldWidth)
                {
                    currentX += totalWidth;
                }

                transform.position = new Vector3(currentX, _startPosition.y, _startPosition.z);
            }
            else
            {
                // Linear movement for items and obstacles
                transform.position = _startPosition + Vector3.left * _accumulatedDistance;

                // Return to pool when off-screen
                if (transform.position.x < -_screenHalfWidth - _worldWidth)
                {
                    if (ObjectPool.Instance != null)
                    {
                        ObjectPool.Instance.ReturnToPool(gameObject);
                    }
                    else
                    {
                        gameObject.SetActive(false);
                    }
                }
            }
        }

        // Layer_0_0 → "Layer_0" (groups the 2 copies of each parallax layer)
        // Road_-3   → "Road"    (groups all road tiles together)
        private string GetGroupPrefix()
        {
            string[] parts = gameObject.name.Split('_');
            if (parts.Length >= 3)
                return parts[0] + "_" + parts[1];
            return parts[0];
        }

        public void SetSpeedMultiplier(float multiplier)
        {
            _speedMultiplier = multiplier;
        }
    }
}
