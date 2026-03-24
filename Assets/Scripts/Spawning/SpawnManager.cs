using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Linksaurus.Core;

namespace Linksaurus.Spawning
{
    public class SpawnManager : MonoBehaviour
    {
        public static SpawnManager Instance { get; private set; }

        [Header("Prefabs")]
        [SerializeField] private GameObject _tiktokPrefab;
        [SerializeField] private GameObject _instagramPrefab;
        [SerializeField] private GameObject _snapchatPrefab;
        [SerializeField] private GameObject[] _connectionPrefabs; // 1, 2, 3
        [SerializeField] private GameObject[] _powerUpPrefabs; // Rocket, Coffee, Shield, Recruiter

        [Header("Spawn Settings")]
        private float _spawnX = 12f;
        private float _lastTikTokSpawnTime = -10f;
        private float _tiktokCooldown = 1.5f;

        private Coroutine _obstacleCoroutine;
        private Coroutine _connectionCoroutine;
        private Coroutine _powerUpCoroutine;

        private void Awake()
        {
            if (Instance == null) Instance = this;
        }

        private void OnEnable()
        {
            GameManager.OnGameStart += StartSpawning;
            GameManager.OnGameOver += StopSpawning;
        }

        private void OnDisable()
        {
            GameManager.OnGameStart -= StartSpawning;
            GameManager.OnGameOver -= StopSpawning;
        }

        private void Start()
        {
            // Pre-warm (Prefabs must be assigned in Inspector)
            if (ObjectPool.Instance != null)
            {
                if (_tiktokPrefab) ObjectPool.Instance.PreWarm(_tiktokPrefab, 10);
                if (_instagramPrefab) ObjectPool.Instance.PreWarm(_instagramPrefab, 10);
                if (_snapchatPrefab) ObjectPool.Instance.PreWarm(_snapchatPrefab, 10);
                foreach (var p in _connectionPrefabs) if (p) ObjectPool.Instance.PreWarm(p, 10);
                foreach (var p in _powerUpPrefabs) if (p) ObjectPool.Instance.PreWarm(p, 5);
            }
        }

        private void StartSpawning()
        {
            StopAllCoroutines();
            _obstacleCoroutine = StartCoroutine(ObstacleLoop());
            _connectionCoroutine = StartCoroutine(ConnectionLoop());
            _powerUpCoroutine = StartCoroutine(PowerUpLoop());
        }

        private void StopSpawning()
        {
            StopAllCoroutines();
        }

        private IEnumerator ObstacleLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(Random.Range(1.5f, 3.0f));
                if (GameManager.Instance.CurrentState != GameState.Playing) break;

                float roll = Random.value;
                GameObject prefab = null;

                if (roll < 0.30f)
                {
                    if (Time.time - _lastTikTokSpawnTime >= _tiktokCooldown)
                    {
                        prefab = _tiktokPrefab;
                        _lastTikTokSpawnTime = Time.time;
                    }
                }
                else if (roll < 0.55f)
                {
                    prefab = _instagramPrefab;
                }
                else if (roll < 0.75f)
                {
                    prefab = _snapchatPrefab;
                }

                if (prefab != null)
                {
                    // Ground surface is at -4.0 (Ground pos -4.5 + half scale 0.5)
                    // Obstacles are ~1.0 world units high with center pivot.
                    // Y = -3.5 puts the bottom exactly on the ground.
                    float y = (Random.value < 0.6f) ? -3.5f : -2.0f;
                    Spawn(prefab, y);
                }
            }
        }

        private IEnumerator ConnectionLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(Random.Range(1.5f, 3.5f));
                if (GameManager.Instance.CurrentState != GameState.Playing) break;

                GameObject prefab = _connectionPrefabs[Random.Range(0, _connectionPrefabs.Length)];
                // Heights adjusted for -4.0 ground surface
                float[] heights = { -3.5f, -2.0f, -0.5f }; // Ground, Air low, Air high
                float y = heights[Random.Range(0, heights.Length)];
                Spawn(prefab, y);
            }
        }

        private IEnumerator PowerUpLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(Random.Range(15f, 25f));
                if (GameManager.Instance.CurrentState != GameState.Playing) break;

                GameObject prefab = _powerUpPrefabs[Random.Range(0, _powerUpPrefabs.Length)];
                float y = -2.0f; // Default height for power-ups (Air low)
                Spawn(prefab, y);
            }
        }

        private void Spawn(GameObject prefab, float y)
        {
            if (prefab == null) return;
            GameObject obj = ObjectPool.Instance.GetFromPool(prefab);
            obj.transform.position = new Vector3(_spawnX, y, 0);
        }
    }
}
