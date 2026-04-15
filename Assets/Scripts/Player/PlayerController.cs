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
    public enum PlayerState { Idle, Moving, Interacting }

    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        private const float MovementInputThreshold = 0.01f;
        private const float GravityStrength = 9.81f;
        private const int MaxInteractionHits = 16;

        public static event Action<Ingredient> OnPickedUp;
        public static event Action OnDropped;
        public static event Action<string> OnNearInteractable;
        public static event Action OnLeftInteractable;

        [Header("Movement")]
        [SerializeField] private float moveSpeed = 5f;

        [Header("Interaction")]
        [SerializeField] private float interactionRadius = 1.5f;
        [SerializeField] private LayerMask interactionMask;

        [Header("Item Hold Offset")]
        [SerializeField] private Vector3 holdOffset = new Vector3(0.5f, 0.8f, 0.5f);

        public PlayerState State { get; private set; } = PlayerState.Idle;
        public Ingredient HeldIngredient { get; private set; }

        private readonly Collider[] _interactionHits = new Collider[MaxInteractionHits];

        private CharacterController _cc;
        private Transform _cachedTransform;
        private IInteractable _nearestInteractable;
        private string _currentPrompt;
        private bool _isGameRunning;

        private void Awake()
        {
            _cc = GetComponent<CharacterController>();
            _cachedTransform = transform;
            GameLogger.Verbose(GameLogCategory.Player, "Player controller initialised.", this);
        }

        private void Update()
        {
            if (!_isGameRunning) return;

            HandleMovement();
            HandleInteractionScan();
            HandleInteractionInput();
        }

        public void SetGameRunning(bool running)
        {
            if (_isGameRunning == running) return;

            _isGameRunning = running;
            GameLogger.Info(GameLogCategory.Player, $"Player input {(running ? "enabled" : "disabled")}.", this);

            if (!running)
            {
                ClearNearestInteractable();
                TransitionTo(PlayerState.Idle);
            }
        }

        public void PickUp(Ingredient ingredient)
        {
            if (HeldIngredient != null || ingredient == null) return;

            HeldIngredient = ingredient;
            ingredient.transform.SetParent(_cachedTransform);
            ingredient.transform.localPosition = holdOffset;
            ingredient.gameObject.SetActive(true);
            OnPickedUp?.Invoke(ingredient);
            TransitionTo(PlayerState.Idle);
            GameLogger.Info(GameLogCategory.Player, $"Picked up {GameLogger.DescribeIngredient(ingredient)}.", this);
        }

        public void Drop()
        {
            if (HeldIngredient == null) return;

            Ingredient droppedIngredient = HeldIngredient;
            droppedIngredient.transform.SetParent(null);
            HeldIngredient = null;
            OnDropped?.Invoke();
            GameLogger.Info(GameLogCategory.Player, $"Dropped {GameLogger.DescribeIngredient(droppedIngredient)}.", this);
        }

        private void HandleMovement()
        {
            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");
            Vector3 moveDirection = new Vector3(h, 0f, v).normalized;

            if (moveDirection.sqrMagnitude > MovementInputThreshold)
            {
                _cachedTransform.forward = moveDirection;
                TransitionTo(PlayerState.Moving);
            }
            else
            {
                TransitionTo(PlayerState.Idle);
            }

            Vector3 movement = (moveDirection * moveSpeed) + (Vector3.down * GravityStrength);
            _cc.Move(movement * Time.deltaTime);
        }

        private void HandleInteractionScan()
        {
            int hitCount = Physics.OverlapSphereNonAlloc(
                _cachedTransform.position,
                interactionRadius,
                _interactionHits,
                interactionMask);

            IInteractable closest = null;
            float closestSqrDistance = float.MaxValue;
            Vector3 origin = _cachedTransform.position;

            for (int i = 0; i < hitCount; i++)
            {
                Collider hit = _interactionHits[i];
                if (hit == null) continue;

                IInteractable interactable = hit.GetComponent<IInteractable>();
                if (interactable == null) continue;

                float sqrDistance = (hit.transform.position - origin).sqrMagnitude;
                if (sqrDistance < closestSqrDistance)
                {
                    closestSqrDistance = sqrDistance;
                    closest = interactable;
                }

                _interactionHits[i] = null;
            }

            if (closest == null)
            {
                ClearNearestInteractable();
                return;
            }

            _nearestInteractable = closest;
            string prompt = closest.GetInteractionPrompt();

            if (prompt != _currentPrompt)
            {
                _currentPrompt = prompt;
                OnNearInteractable?.Invoke(prompt);
            }
        }

        private void HandleInteractionInput()
        {
            if (!Input.GetKeyDown(KeyCode.E) || _nearestInteractable == null)
            {
                return;
            }

            TransitionTo(PlayerState.Interacting);
            GameLogger.Verbose(GameLogCategory.Player, $"Interacting with {_nearestInteractable.GetType().Name}.", this);
            _nearestInteractable.Interact(this);
        }

        private void ClearNearestInteractable()
        {
            if (_nearestInteractable == null && string.IsNullOrEmpty(_currentPrompt))
            {
                return;
            }

            _nearestInteractable = null;
            _currentPrompt = null;
            OnLeftInteractable?.Invoke();
        }

        private void TransitionTo(PlayerState newState)
        {
            if (State == newState) return;

            GameLogger.Verbose(GameLogCategory.Player, $"State changed {State} -> {newState}.", this);
            State = newState;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, interactionRadius);
        }
    }
}
