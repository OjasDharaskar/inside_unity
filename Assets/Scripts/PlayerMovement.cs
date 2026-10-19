using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Bidirectional (left/right) 2D player movement controller using Unity's New Input System.
/// Supports InputActionReference, default fallback input bindings, and PlayerInput component messages.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Configuration")]
    [Tooltip("Horizontal movement speed in units per second.")]
    [SerializeField] private float moveSpeed = 6f;

    [Tooltip("Smoothing time for horizontal velocity acceleration/deceleration.")]
    [SerializeField] private float smoothTime = 0.05f;

    [Header("Input System Setup")]
    [Tooltip("Optional: Link the 'Move' action from your .inputactions asset (e.g., Player/Move). If left unassigned, fallback keyboard (A/D, Arrows) and gamepad inputs will be used automatically.")]
    [SerializeField] private InputActionReference moveActionReference;

    [Header("Sprite / Visuals")]
    [Tooltip("Automatically flip SpriteRenderer horizontally depending on moving left or right.")]
    [SerializeField] private bool autoFlipSprite = true;
    [SerializeField] private Animator _animator;

    // Component References
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    // Input & Physics State
    private InputAction fallbackMoveAction;
    private Vector2 moveInput;
    private float smoothVelocityX;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Prevent the player from tumbling over when colliding
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        // If no InputActionReference was assigned in the Inspector, initialize a fallback action
        if (moveActionReference == null)
        {
            SetupFallbackInput();
        }
    }

    private void SetupFallbackInput()
    {
        fallbackMoveAction = new InputAction("FallbackMove", InputActionType.Value, expectedControlType: "Vector2");

        // WASD composite
        fallbackMoveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");

        // Arrow keys composite
        fallbackMoveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/upArrow")
            .With("Down", "<Keyboard>/downArrow")
            .With("Left", "<Keyboard>/leftArrow")
            .With("Right", "<Keyboard>/rightArrow");

        // Gamepad bindings
        fallbackMoveAction.AddBinding("<Gamepad>/leftStick");
        fallbackMoveAction.AddBinding("<Gamepad>/dpad");
    }

    private void OnEnable()
    {
        if (moveActionReference != null && moveActionReference.action != null)
        {
            moveActionReference.action.Enable();
        }
        else
        {
            fallbackMoveAction?.Enable();
        }
    }

    private void OnDisable()
    {
        if (moveActionReference != null && moveActionReference.action != null)
        {
            moveActionReference.action.Disable();
        }
        else
        {
            fallbackMoveAction?.Disable();
        }
    }

    private void Update()
    {
        ReadInput();
        HandleSpriteFlipping();
    }

    private void FixedUpdate()
    {
        ApplyMovement();
    }

    /// <summary>
    /// Reads horizontal input vector from either the assigned InputActionReference or the fallback action.
    /// </summary>
    private void ReadInput()
    {
        if (moveActionReference != null && moveActionReference.action != null)
        {
            moveInput = moveActionReference.action.ReadValue<Vector2>();
        }
        else if (fallbackMoveAction != null)
        {
            moveInput = fallbackMoveAction.ReadValue<Vector2>();
        }
    }

    /// <summary>
    /// Applies bidirectional movement along the X axis while preserving gravity/jumping along the Y axis.
    /// </summary>
    private void ApplyMovement()
    {
        float targetSpeed = moveInput.x * moveSpeed;
        Vector2 currentVelocity = GetVelocity();

        // Smoothly damp horizontal velocity towards target speed
        float targetVelocityX = Mathf.SmoothDamp(currentVelocity.x, targetSpeed, ref smoothVelocityX, smoothTime);

        SetVelocity(new Vector2(targetVelocityX, currentVelocity.y));
        if(moveInput.x !=0)
        {
            _animator.SetBool("isRunning", true);
        }
        else
        {
            _animator.SetBool("isRunning", false);
        }
    }

    /// <summary>
    /// Flips the SpriteRenderer horizontally based on current movement direction.
    /// </summary>
    private void HandleSpriteFlipping()
    {
        if (!autoFlipSprite || spriteRenderer == null) return;

        if (moveInput.x > 0.01f)
        {
            spriteRenderer.flipX = false;
        }
        else if (moveInput.x < -0.01f)
        {
            spriteRenderer.flipX = true;
        }
    }

    /// <summary>
    /// Linear velocity getter compatible with Unity 6 (rb.linearVelocity) and earlier versions (rb.velocity).
    /// </summary>
    private Vector2 GetVelocity()
    {
#if UNITY_6000_0_OR_NEWER
        return rb.linearVelocity;
#else
        return rb.velocity;
#endif
    }

    /// <summary>
    /// Linear velocity setter compatible with Unity 6 (rb.linearVelocity) and earlier versions (rb.velocity).
    /// </summary>
    private void SetVelocity(Vector2 velocity)
    {
#if UNITY_6000_0_OR_NEWER
        rb.linearVelocity = velocity;
#else
        rb.velocity = velocity;
#endif
    }

    #region PlayerInput Component Messages
    /// <summary>
    /// Receives input message if using a PlayerInput component set to "Send Messages".
    /// </summary>
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }
    #endregion
}
