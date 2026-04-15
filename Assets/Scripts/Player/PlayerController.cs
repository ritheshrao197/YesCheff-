// PlayerController.cs
// Handles WASD movement, E interaction, and item holding.
// Uses a lightweight FSM: Idle, Moving, Interacting.

using System;
using UnityEngine;
using YesChef.Core;
using YesChef.Ingredients;
using YesChef.Interfaces;

namespace YesChef.Player
{
    // ── Simple Player FSM ─────────────────────────────────────────────────
    public enum PlayerState { Idle, Moving, Interacting }

    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        private const float MovementInputThreshold = 0.01f;
        private const float GravityStrength = 9.81f;

        // ── Events ────────────────────────────────────────────────────────
        public static event Action<Ingredient> OnPickedUp;
        public static event Action             OnDropped;
        public static event Action<string>     OnNearInteractable; // prompt text
        public static event Action             OnLeftInteractable;

        // ── Inspector ─────────────────────────────────────────────────────
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 5f;

        [Header("Interaction")]
        [SerializeField] private float interactionRadius = 1.5f;
        [SerializeField] private LayerMask interactionMask;

        [Header("Item Hold Offset")]
        [SerializeField] private Vector3 holdOffset = new Vector3(0.5f, 0.8f, 0.5f);

        // ── State ─────────────────────────────────────────────────────────
        public PlayerState State { get; private set; } = PlayerState.Idle;
        public Ingredient HeldIngredient { get; private set; }

        private CharacterController _cc;
        private IInteractable _nearestInteractable;
        private bool _isGameRunning = false;

        // ── Unity lifecycle ───────────────────────────────────────────────
        private void Awake()
        {
            _cc = GetComponent<CharacterController>();
            GameLogger.Verbose(GameLogCategory.Player, "Player controller initialised.", this);
        }

        private void Update()
        {
            if (!_isGameRunning) return;

            HandleMovement();
            HandleInteractionScan();
            HandleInteractionInput();
            UpdateHeldItemPosition();
        }

        // ── Public API ────────────────────────────────────────────────────
        public void SetGameRunning(bool running)
        {
            GameLogger.Info(GameLogCategory.Player, $"Player input {(running ? "enabled" : "disabled")}.", this);

            if (_isGameRunning == running) return;

            _isGameRunning = running;
        }

        public void PickUp(Ingredient ingredient)
        {
            if (HeldIngredient != null) return;  // Can't hold two things

            HeldIngredient = ingredient;
            ingredient.transform.SetParent(transform);
            ingredient.transform.localPosition = holdOffset;
            ingredient.gameObject.SetActive(true);
            OnPickedUp?.Invoke(ingredient);
            TransitionTo(PlayerState.Idle);
            GameLogger.Info(GameLogCategory.Player, $"Picked up {GameLogger.DescribeIngredient(ingredient)}.", this);
        }

        /// <summary>Removes the held item reference WITHOUT destroying the object.</summary>
        public void Drop()
        {
            if (HeldIngredient == null) return;

            HeldIngredient.transform.SetParent(null);
            GameLogger.Info(GameLogCategory.Player, $"Dropped {GameLogger.DescribeIngredient(HeldIngredient)}.", this);
            HeldIngredient = null;
            OnDropped?.Invoke();
        }

        // ── Private helpers ───────────────────────────────────────────────
        private void HandleMovement()
        {
            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");
            Vector3 dir = new Vector3(h, 0f, v).normalized;

            if (dir.sqrMagnitude > MovementInputThreshold)
            {
                _cc.Move(dir * moveSpeed * Time.deltaTime);
                transform.forward = dir;   // Face movement direction
                TransitionTo(PlayerState.Moving);
            }
            else
            {
                TransitionTo(PlayerState.Idle);
            }

            // Gravity
            _cc.Move(Vector3.down * GravityStrength * Time.deltaTime);
        }

        private void HandleInteractionScan()
        {
            // Find all colliders within radius on the interaction layer
            Collider[] hits = Physics.OverlapSphere(transform.position, interactionRadius, interactionMask);

            IInteractable closest = null;
            float closestDist = float.MaxValue;

            foreach (var hit in hits)
            {
                var interactable = hit.GetComponent<IInteractable>();
                if (interactable == null) continue;

                float dist = Vector3.Distance(transform.position, hit.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = interactable;
                }
            }

            if (closest != _nearestInteractable)
            {
                _nearestInteractable = closest;
                if (_nearestInteractable != null)
                    OnNearInteractable?.Invoke(_nearestInteractable.GetInteractionPrompt());
                else
                    OnLeftInteractable?.Invoke();
            }
            else if (_nearestInteractable != null)
            {
                // Refresh prompt text (it can change dynamically, e.g. stove slot state)
                OnNearInteractable?.Invoke(_nearestInteractable.GetInteractionPrompt());
            }
        }

        private void HandleInteractionInput()
        {
            if (Input.GetKeyDown(KeyCode.E) && _nearestInteractable != null)
            {
                TransitionTo(PlayerState.Interacting);
                GameLogger.Verbose(GameLogCategory.Player, $"Interacting with {_nearestInteractable.GetType().Name}.", this);
                _nearestInteractable.Interact(this);
            }
        }

        private void UpdateHeldItemPosition()
        {
            if (HeldIngredient != null)
                HeldIngredient.transform.localPosition = holdOffset;
        }

        private void TransitionTo(PlayerState newState)
        {
            if (State == newState) return;
            GameLogger.Verbose(GameLogCategory.Player, $"State changed {State} -> {newState}.", this);
            State = newState;
            // Hook: animation or VFX could be triggered here
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, interactionRadius);
        }
    }
}
