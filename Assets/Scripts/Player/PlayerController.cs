// PlayerController.cs
// Handles movement, interaction input, and held ingredient state.

using UnityEngine;
using YesChef.Core;
using YesChef.Ingredients;
using YesChef.Interfaces;

namespace YesChef.Player
{
    public enum PlayerState
    {
        Idle,
        Moving,
        Interacting
    }

    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        private const float MovementInputThreshold = 0.01f;
        private const float GravityStrength = 9.81f;

        [Header("Configuration")]
        [SerializeField] private PlayerConfig playerConfig;

        [Header("Interaction")]
        [SerializeField] private LayerMask interactionMask;

        [Header("Fallback Tuning")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float interactionRadius = 1.5f;
        [SerializeField] private Vector3 holdOffset = new Vector3(0.5f, 0.8f, 0.5f);

        public PlayerState State { get; private set; } = PlayerState.Idle;
        public Ingredient HeldIngredient { get; private set; }

        private CharacterController _characterController;
        private Transform _cachedTransform;
        private PlayerInteractionSensor _interactionSensor;
        private IInteractable _nearestInteractable;
        private string _currentPrompt;
        private bool _isGameRunning;

        private float MoveSpeed => playerConfig != null ? playerConfig.moveSpeed : moveSpeed;
        private float InteractionRadius => playerConfig != null ? playerConfig.interactionRadius : interactionRadius;
        private Vector3 HoldOffset => playerConfig != null ? playerConfig.holdOffset : holdOffset;

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
            _cachedTransform = transform;
            _interactionSensor = new PlayerInteractionSensor(_cachedTransform, InteractionRadius, interactionMask);
            GameLogger.Verbose(GameLogCategory.Player, "Player controller initialised.", this);
        }

        private void Update()
        {
            if (!_isGameRunning)
            {
                return;
            }

            HandleMovement();
            HandleInteractionScan();
            HandleInteractionInput();
        }

        public void SetGameRunning(bool running)
        {
            if (_isGameRunning == running)
            {
                return;
            }

            _isGameRunning = running;
            GameLogger.Info(GameLogCategory.Player, $"Player input {(running ? "enabled" : "disabled")}.", this);

            if (!running)
            {
                ClearNearestInteractable();
                TransitionTo(PlayerState.Idle);
            }
            if (HeldIngredient != null)
            {
                ClearHeldIngredient();
            }
        }

        public void PickUp(Ingredient ingredient)
        {
            if (HeldIngredient != null || ingredient == null)
            {
                return;
            }

            HeldIngredient = ingredient;
            ingredient.transform.SetParent(_cachedTransform);
            ingredient.transform.localPosition = HoldOffset;
            ingredient.gameObject.SetActive(true);
            GameEvents.RaisePlayerPickedUpIngredient(ingredient);
            TransitionTo(PlayerState.Idle);
            GameLogger.Info(GameLogCategory.Player, $"Picked up {GameLogger.DescribeIngredient(ingredient)}.", this);
        }

        public void Drop()
        {
            if (HeldIngredient == null)
            {
                return;
            }

            Ingredient droppedIngredient = HeldIngredient;
            droppedIngredient.transform.SetParent(null);
            HeldIngredient = null;
            GameEvents.RaisePlayerDroppedIngredient();
            GameLogger.Info(GameLogCategory.Player, $"Dropped {GameLogger.DescribeIngredient(droppedIngredient)}.", this);
        }

        public void ClearHeldIngredient()
        {
            if (HeldIngredient == null)
            {
                return;
            }

            GameLogger.Info(GameLogCategory.Player, $"Cleared held ingredient {GameLogger.DescribeIngredient(HeldIngredient)}.", this);
            Destroy(HeldIngredient.gameObject);
            HeldIngredient = null;

        }

        private void HandleMovement()
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            Vector3 moveDirection = new Vector3(horizontal, 0f, vertical);

            if (moveDirection.sqrMagnitude > MovementInputThreshold)
            {
                moveDirection.Normalize();
                _cachedTransform.forward = moveDirection;
                TransitionTo(PlayerState.Moving);
            }
            else
            {
                TransitionTo(PlayerState.Idle);
            }

            Vector3 movement = (moveDirection * MoveSpeed) + (Vector3.down * GravityStrength);
            _characterController.Move(movement * Time.deltaTime);
        }

        private void HandleInteractionScan()
        {
            IInteractable nearestInteractable = _interactionSensor.FindNearestInteractable();
            if (nearestInteractable == null)
            {
                ClearNearestInteractable();
                return;
            }

            _nearestInteractable = nearestInteractable;
            string prompt = nearestInteractable.GetInteractionPrompt();
            if (prompt == _currentPrompt)
            {
                return;
            }

            _currentPrompt = prompt;
            GameEvents.RaiseInteractionPromptChanged(prompt);
        }

        private void HandleInteractionInput()
        {
            // Check if either E or Space was pressed this frame, and there's a valid interactable nearby
            if ((Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space)) && _nearestInteractable != null)
            {
                TransitionTo(PlayerState.Interacting);
                GameLogger.Verbose(GameLogCategory.Player, $"Interacting with {_nearestInteractable.GetType().Name}.", this);
                _nearestInteractable.Interact(this);
            }
        }

        private void ClearNearestInteractable()
        {
            if (_nearestInteractable == null && string.IsNullOrEmpty(_currentPrompt))
            {
                return;
            }

            _nearestInteractable = null;
            _currentPrompt = null;
            GameEvents.RaiseInteractionPromptCleared();
        }

        private void TransitionTo(PlayerState newState)
        {
            if (State == newState)
            {
                return;
            }

            GameLogger.Verbose(GameLogCategory.Player, $"State changed {State} -> {newState}.", this);
            State = newState;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, InteractionRadius);
        }
    }
}
