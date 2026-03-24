using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using Linksaurus.Core;
using Linksaurus.Spawning;

namespace Linksaurus.Player
{
    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Jump Settings")]
        [SerializeField] private float _jumpForce = 9.5f; // Slightly more powerful for better responsiveness
        [SerializeField] private LayerMask _groundLayer;
        [SerializeField] private float _groundCheckRadius = 0.15f;
        [SerializeField] private Vector3 _groundCheckOffset = new Vector3(0, -0.4f, 0);

        private Rigidbody2D _rb;
        private bool _isGrounded;
        private bool _shieldActive;
        private BoxCollider2D _collider;

        [Header("Audio")]
        [SerializeField] private AudioClip _jumpSound;
        private AudioSource _audioSource;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.gravityScale = 3f;
            _collider = GetComponent<BoxCollider2D>();
            _audioSource = GetComponent<AudioSource>();
        }

        private void Update()
        {
            if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing) return;

            // Use a Raycast for robust ground detection.
            // Surface is at -4.0. Player center is ~-3.2 to -3.5.
            // Casting downward from player center to find the ground.
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 1.5f, _groundLayer);
            _isGrounded = hit.collider != null;

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
                else
                {
                    // Final attempt: check if we are just slightly off the ground
                    if (Physics2D.OverlapCircle(transform.position + new Vector3(0, -0.8f, 0), 0.5f, _groundLayer))
                    {
                        Jump();
                    }
                    else
                    {
                        Debug.Log($"Jump blocked: Not Grounded. PlayerY: {transform.position.y:F3}, Hit: {(hit.collider != null ? hit.collider.name : "None")}");
                    }
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
                    break;

                case "Snapchat":
                    GameManager.Instance.AddConnections(-5);
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
