using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using Linksaurus.Core;
using Linksaurus.Spawning;

namespace Linksaurus.Player
{
    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D), typeof(Animator))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Jump Settings")]
        [SerializeField] private float _jumpForce = 9.5f; // Slightly more powerful for better responsiveness
        [SerializeField] private LayerMask _groundLayer;
        [SerializeField] private float _groundCheckRadius = 0.15f;
        [SerializeField] private Vector3 _groundCheckOffset = new Vector3(0, 0.1f, 0);

        private Rigidbody2D _rb;
        private Animator _animator;
        private bool _isGrounded;
        private bool _shieldActive;
        private BoxCollider2D _collider;

        // Hash IDs for animator parameters
        private static readonly int IsRunningHash = Animator.StringToHash("IsRunning");
        private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");

        [Header("Audio")]
        [SerializeField] private AudioClip _jumpSound;
        private AudioSource _audioSource;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.gravityScale = 3f;
            _collider = GetComponent<BoxCollider2D>();
            _animator = GetComponent<Animator>();
            _audioSource = GetComponent<AudioSource>();
        }

        private void Update()
        {
            bool isPlaying = GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Playing;
            
            // Update animator parameters
            if (_animator != null)
            {
                _animator.SetBool(IsRunningHash, isPlaying);
                _animator.SetBool(IsGroundedHash, _isGrounded);
            }

            if (!isPlaying) return;

            // Use an OverlapCircle for grounded check since pivot is at feet
            _isGrounded = Physics2D.OverlapCircle(transform.position + _groundCheckOffset, _groundCheckRadius, _groundLayer);

            // Using Pointer.current or Mouse/Touch for jump
            if (Pointer.current != null && Pointer.current.press.wasPressedThisFrame)
            {
                // Ensure we are not clicking on a UI button
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                {
                    // Check if it's touch
                    if (Pointer.current is Touchscreen)
                    {
                        if (Touchscreen.current != null && EventSystem.current.IsPointerOverGameObject(Touchscreen.current.primaryTouch.touchId.ReadValue()))
                        {
                            return;
                        }
                    }
                    else
                    {
                        return;
                    }
                }

                if (_isGrounded)
                {
                    Jump();
                }
                }
                }

        private void Jump()
        {
            // Reset Y velocity for consistent jump height
            _rb.linearVelocity = new Vector2(0, _jumpForce);
            if (_audioSource != null && _jumpSound != null)
            {
                _audioSource.PlayOneShot(_jumpSound);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position + _groundCheckOffset, _groundCheckRadius);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (GameManager.Instance.CurrentState != GameState.Playing) return;

            switch (other.tag)
            {
                case "TikTok":
                    if (_shieldActive)
                    {
                        DeactivateShield();
                    }
                    else
                    {
                        GameManager.Instance.TriggerGameOver();
                    }
                    break;

                case "Instagram":
                    GameManager.Instance.AddConnections(-3);
                    other.gameObject.SetActive(false);
                    break;

                case "Snapchat":
                    GameManager.Instance.AddConnections(-5);
                    other.gameObject.SetActive(false);
                    break;

                case "Connection":
                    Connection connection = other.GetComponent<Connection>();
                    if (connection != null)
                    {
                        GameManager.Instance.AddConnections(connection.ConnectionValue);
                    }
                    // Hide the object
                    other.gameObject.SetActive(false);
                    break;

                case "PowerUp":
                    PowerUpItem powerUpItem = other.GetComponent<PowerUpItem>();
                    if (powerUpItem != null)
                    {
                        PowerUpManager.Instance.ActivatePowerUp(powerUpItem.Type);
                    }
                    other.gameObject.SetActive(false);
                    break;
            }
        }

        public void RevivePlayer()
        {
            // Reset velocity
            _rb.linearVelocity = Vector2.zero;
            
            // Give temporary shield
            ActivateShield();
            Invoke(nameof(DeactivateShield), 3f); // 3 seconds of invulnerability

            // Resume game
            GameManager.Instance.CurrentState = GameState.Playing;
            Time.timeScale = 1f;
        }

        public void ActivateShield()
        {
            _shieldActive = true;
            // Visual feedback (if there were a shield object, we'd enable it here)
        }

        private void DeactivateShield()
        {
            _shieldActive = false;
            PowerUpManager.Instance.DeactivateShield();
        }
    }
}
