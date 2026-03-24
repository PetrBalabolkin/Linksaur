using UnityEngine;
using System;
using System.Collections;
using Linksaurus.Core;
using Linksaurus.Spawning;

namespace Linksaurus.Player
{
    public enum PowerUpType { Rocket, CoffeeBreak, Shield, RecruiterMode }

    public class PowerUpManager : MonoBehaviour
    {
        public static PowerUpManager Instance { get; private set; }

        public static event Action<PowerUpType?, float> OnPowerUpChanged;

        public PowerUpType? ActivePowerUp { get; private set; }
        public float RemainingDuration { get; private set; }

        [Header("Player References")]
        [SerializeField] private PlayerController _player;
        [SerializeField] private SpriteRenderer _playerRenderer;
        [SerializeField] private GameObject _rocketParticles;
        [SerializeField] private GameObject _shieldCircle;
        [SerializeField] private GameObject _recruiterGlow;

        private Coroutine _activePowerUpCoroutine;

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

        public void ActivatePowerUp(PowerUpType type)
        {
            if (_activePowerUpCoroutine != null)
            {
                StopCoroutine(_activePowerUpCoroutine);
                DeactivateCurrent();
            }

            ActivePowerUp = type;
            switch (type)
            {
                case PowerUpType.Rocket:
                    _activePowerUpCoroutine = StartCoroutine(RocketRoutine());
                    break;
                case PowerUpType.CoffeeBreak:
                    _activePowerUpCoroutine = StartCoroutine(CoffeeBreakRoutine());
                    break;
                case PowerUpType.Shield:
                    ActivateShield();
                    break;
                case PowerUpType.RecruiterMode:
                    _activePowerUpCoroutine = StartCoroutine(RecruiterModeRoutine());
                    break;
            }
        }

        private void DeactivateCurrent()
        {
            if (ActivePowerUp == PowerUpType.Rocket)
            {
                GameManager.Instance.ScrollSpeed /= 2f;
                if (_rocketParticles) _rocketParticles.SetActive(false);
            }
            else if (ActivePowerUp == PowerUpType.CoffeeBreak)
            {
                GameManager.Instance.ScrollSpeed /= 0.5f;
                if (_playerRenderer) _playerRenderer.color = Color.white;
            }
            else if (ActivePowerUp == PowerUpType.Shield)
            {
                if (_shieldCircle) _shieldCircle.SetActive(false);
            }
            else if (ActivePowerUp == PowerUpType.RecruiterMode)
            {
                if (_recruiterGlow) _recruiterGlow.SetActive(false);
            }

            ActivePowerUp = null;
            RemainingDuration = 0;
            OnPowerUpChanged?.Invoke(null, 0);
        }

        private IEnumerator RocketRoutine()
        {
            float duration = 5f;
            RemainingDuration = duration;
            GameManager.Instance.ScrollSpeed *= 2f;
            if (_rocketParticles) _rocketParticles.SetActive(true);
            OnPowerUpChanged?.Invoke(PowerUpType.Rocket, duration);

            while (RemainingDuration > 0)
            {
                RemainingDuration -= Time.deltaTime;
                yield return null;
            }

            DeactivateCurrent();
        }

        private IEnumerator CoffeeBreakRoutine()
        {
            float duration = 8f;
            RemainingDuration = duration;
            GameManager.Instance.ScrollSpeed *= 0.5f;
            if (_playerRenderer) _playerRenderer.color = new Color32(0x8B, 0x45, 0x13, 0xFF); // #8B4513
            OnPowerUpChanged?.Invoke(PowerUpType.CoffeeBreak, duration);

            while (RemainingDuration > 0)
            {
                RemainingDuration -= Time.deltaTime;
                yield return null;
            }

            DeactivateCurrent();
        }

        private void ActivateShield()
        {
            if (_shieldCircle) _shieldCircle.SetActive(true);
            if (_player) _player.ActivateShield();
            OnPowerUpChanged?.Invoke(PowerUpType.Shield, -1); // -1 for infinite until hit
        }

        public void DeactivateShield()
        {
            if (ActivePowerUp == PowerUpType.Shield)
            {
                DeactivateCurrent();
            }
        }

        private IEnumerator RecruiterModeRoutine()
        {
            float duration = 6f;
            RemainingDuration = duration;
            if (_recruiterGlow) _recruiterGlow.SetActive(true);
            OnPowerUpChanged?.Invoke(PowerUpType.RecruiterMode, duration);

            while (RemainingDuration > 0)
            {
                RemainingDuration -= Time.deltaTime;
                
                // Attraction logic
                Collider2D[] connections = Physics2D.OverlapCircleAll(_player.transform.position, 5f);
                foreach (var col in connections)
                {
                    if (col.CompareTag("Connection"))
                    {
                        col.transform.position = Vector2.MoveTowards(col.transform.position, _player.transform.position, 8f * Time.deltaTime);
                    }
                }

                yield return null;
            }

            DeactivateCurrent();
        }
    }
}
