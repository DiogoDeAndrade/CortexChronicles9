using UnityEngine;
using NaughtyAttributes;

[RequireComponent(typeof(Rigidbody2D))]
public class MovementPlatformer : MonoBehaviour
{
    public enum JumpBehaviour { None = 0, Fixed = 1, Variable = 2 };
    public enum GlideBehaviour { None = 0, Enabled = 1, Timer = 2 };

    [SerializeField]
    private Vector2 speed = new Vector2(100, 100);
    [SerializeField, InputAxis]
    private string horizontalAxis = "Horizontal";
    [SerializeField]
    private float gravityScale = 1.0f;
    [SerializeField]
    private bool useTerminalVelocity = false;
    [SerializeField]
    private float terminalVelocity = 100.0f;
    [SerializeField]
    private float coyoteTime = 0.0f;
    [SerializeField]
    private JumpBehaviour jumpBehaviour = JumpBehaviour.None;
    [SerializeField]
    private int maxJumpCount = 1;
    [SerializeField]
    private float jumpBufferingTime = 0.1f;
    [SerializeField]
    private float jumpHoldMaxTime = 0.1f;
    [SerializeField]
    private string jumpButton = "Jump";
    [SerializeField]
    private bool enableAirControl = true;
    [SerializeField]
    private Collider2D airCollider;
    [SerializeField]
    private Collider2D groundCollider;
    [SerializeField]
    private Collider2D groundCheckCollider;
    [SerializeField]
    private LayerMask groundLayerMask;
    [SerializeField]
    private bool useAnimator = false;
    [SerializeField]
    private Animator animator;
    [SerializeField, AnimatorParam("animator", AnimatorControllerParameterType.Float)]
    private string horizontalVelocityParameter;
    [SerializeField, AnimatorParam("animator", AnimatorControllerParameterType.Float)]
    private string absoluteHorizontalVelocityParameter;
    [SerializeField, AnimatorParam("animator", AnimatorControllerParameterType.Float)]
    private string verticalVelocityParameter;
    [SerializeField, AnimatorParam("animator", AnimatorControllerParameterType.Float)]
    private string absoluteVerticalVelocityParameter;
    [SerializeField, AnimatorParam("animator", AnimatorControllerParameterType.Bool)]
    private string isGroundedParameter;

    public bool isGrounded { get; private set; }
    private SpriteRenderer spriteRenderer;
    protected Rigidbody2D rb;
    private int currentJumpCount;
    private bool prevJumpKey = false;
    private float jumpBufferingTimer = 0.0f;
    private float jumpTime;
    private float coyoteTimer;
    private bool actualIsGrounded;
    private Vector2 lastDelta;

    const float epsilonZero = 1e-3f;

    public Vector2 GetSpeed() => speed;
    public void SetSpeed(Vector2 speed) { this.speed = speed; }

    public void SetGravityScale(float v) { gravityScale = v; }
    public float GetGravityScale() => gravityScale;

    public void SetMaxJumpCount(int v) { maxJumpCount = v; }

    public void SetJumpHoldTime(float v) { jumpHoldMaxTime = v; }
    public float GetJumpHoldTime() => jumpHoldMaxTime;

    protected void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (rb)
        {
            rb.gravityScale = 0.0f;
        }
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }
    }

    protected void MoveDelta(Vector3 delta)
    {
        if (rb != null)
        {
            rb.velocity = delta / Time.deltaTime;
        }
        else
        {
            transform.position = transform.position + delta;
        }
        lastDelta = delta;
    }

    protected void StopMovement()
    {
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }
    }

    void FixedUpdate()
    {
        UpdateGroundState();

        // Jump buffering
        if ((jumpBehaviour != JumpBehaviour.None) && (jumpBufferingTimer > 0))
        {
            jumpBufferingTimer -= Time.fixedDeltaTime;
            if (isGrounded)
            {
                Jump();
            }
        }

        // Fixed height jump
        if (jumpBehaviour == JumpBehaviour.Fixed)
        {
            bool isJumpPressed = GetJumpPressed();
            if ((isJumpPressed) && (!prevJumpKey))
            {
                jumpBufferingTimer = jumpBufferingTime;

                if ((isJumpPressed) && (!prevJumpKey))
                {
                    if ((isGrounded) && (currentJumpCount == maxJumpCount))
                    {
                        Jump();
                    }
                    else if (currentJumpCount > 0)
                    {
                        Jump();
                    }
                }
            }
            prevJumpKey = isJumpPressed;
        }
        else
        {
            bool isJumpPressed = GetJumpPressed();
            if (isJumpPressed)
            {
                if (!prevJumpKey)
                {
                    jumpBufferingTimer = jumpBufferingTime;

                    if ((isGrounded) && (currentJumpCount == maxJumpCount))
                    {
                        Jump();
                    }
                    else if (currentJumpCount > 0)
                    {
                        Jump();
                    }
                }
                else if ((Time.time - jumpTime) < jumpHoldMaxTime)
                {
                    rb.velocity = new Vector2(rb.velocity.x, speed.y);
                }
            }
            else
            {
                // Jump button was released, so it doesn't count anymore as being pressed
                jumpTime = -float.MaxValue;
            }
            prevJumpKey = isJumpPressed;
        }

        bool limitFallSpeed = false;
        float maxFallSpeed = float.MaxValue;

        if (useTerminalVelocity)
        {
            limitFallSpeed = true;
            maxFallSpeed = terminalVelocity;
        }

        if (limitFallSpeed)
        {
            var currentVelocity = rb.velocity;
            if (currentVelocity.y < -maxFallSpeed)
            {
                currentVelocity.y = -maxFallSpeed;
                rb.velocity = currentVelocity;
            }
        }
    }

    void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, speed.y);
        jumpBufferingTimer = 0.0f;
        coyoteTimer = 0;
        jumpTime = Time.time;
        currentJumpCount--;
    }

    bool GetJumpPressed()
    {
        if ((jumpButton != "") && (Input.GetButton(jumpButton))) return true;

        return false;
    }
    void Update()
    {
        if (coyoteTimer > 0)
        {
            coyoteTimer -= Time.deltaTime;
        }

        float deltaX = 0.0f;

        UpdateGroundState();

        if ((enableAirControl) || (isGrounded))
        {
            if (horizontalAxis != "") deltaX = Input.GetAxis(horizontalAxis) * speed.x;
            rb.velocity = new Vector2(deltaX, rb.velocity.y);
        }

        // Need to check with actual is grounded or else coyote time will make the jump count reset immediately after flying off
        if (actualIsGrounded)
        {
            rb.gravityScale = 0.0f;
            currentJumpCount = maxJumpCount;
            if (airCollider) airCollider.enabled = false;
            if (groundCollider) groundCollider.enabled = true;
        }
        else
        {
            rb.gravityScale = gravityScale;
            if (airCollider) airCollider.enabled = true;
            if (groundCollider) groundCollider.enabled = false;
        }

        var currentVelocity = rb.velocity;

        if ((useAnimator) && (animator))
        {
            if (horizontalVelocityParameter != "") animator.SetFloat(horizontalVelocityParameter, currentVelocity.x);
            if (absoluteHorizontalVelocityParameter != "") animator.SetFloat(absoluteHorizontalVelocityParameter, Mathf.Abs(currentVelocity.x));
            if (verticalVelocityParameter != "") animator.SetFloat(verticalVelocityParameter, currentVelocity.y);
            if (absoluteVerticalVelocityParameter != "") animator.SetFloat(absoluteVerticalVelocityParameter, Mathf.Abs(currentVelocity.y));
            if (isGroundedParameter != "") animator.SetBool(isGroundedParameter, actualIsGrounded);
        }

        if ((deltaX > epsilonZero) && (transform.right.x < 0.0f)) transform.rotation *= Quaternion.Euler(0, 180, 0);
        else if ((deltaX < -epsilonZero) && (transform.right.x > 0.0f)) transform.rotation *= Quaternion.Euler(0, 180, 0);
    }

    void UpdateGroundState()
    {
        if (groundCheckCollider)
        {
            ContactFilter2D contactFilter = new ContactFilter2D();
            contactFilter.useLayerMask = true;
            contactFilter.layerMask = groundLayerMask;

            Collider2D[] results = new Collider2D[128];

            int n = Physics2D.OverlapCollider(groundCheckCollider, contactFilter, results);
            if (n > 0)
            {
                actualIsGrounded = true;
                isGrounded = true;
                return;
            }
            else
            {
                actualIsGrounded = false;
                if (rb.velocity.y > 0)
                {
                    coyoteTimer = 0;
                }
            }
        }

        if (actualIsGrounded)
        {
            coyoteTimer = coyoteTime;
        }

        actualIsGrounded = false;

        if (coyoteTimer > 0)
        {
            isGrounded = true;
            return;
        }

        isGrounded = false;
    }
}