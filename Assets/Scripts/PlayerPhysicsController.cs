using Mirror;
using UnityEngine;

public class PlayerPhysicsController : NetworkBehaviour
{
    [SerializeField] private float maxSpeed = 7;
    [SerializeField] private float jumpTakeOffSpeed = 7;
    [SerializeField] private float minGroundNormalY = 0.65f;
    [SerializeField] private float gravityModifier = 1.0f;

    private bool isMoveUpDown;
    private Vector2 gravity;
    private Vector2 targetVelocity;
    private bool isGrounded;
    private Vector2 groundNormal;
    private Rigidbody2D rb2d;
    private Vector2 velocity;
    private ContactFilter2D contactFilter;
    private RaycastHit2D[] hitBuffer = new RaycastHit2D[16];

    private const float minMoveDistance = 0.001f;
    private const float shellRadius = 0.01f;

    private BoxCollider2D _collider;
    private bool[] _rayCasts = new bool[8];
    [SerializeField] private float _raycastDistance = 0.1f;

    private float inputHorizontal = 0;
    private float inputVertical = 0;
    private bool inputJump = false;

    private float oldInputHorizontal = 0;
    private float oldInputVertical = 0;
    private bool oldInputJump = false;

    void OnEnable()
    {
        //if (!isServer) return;
        rb2d = GetComponent<Rigidbody2D>();
        _collider = GetComponent<BoxCollider2D>();
        SetGravity(Physics2D.gravity);
    }

    public void SetGravity(Vector2 newGravity)
    {
        gravity = newGravity;
        velocity = Vector2.zero;
        groundNormal = -gravity.normalized;
    }

    void Start()
    {
        //if (!isServer) return;

        contactFilter.useTriggers = false;
        contactFilter.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));
        contactFilter.useLayerMask = true;
    }

    [Command]
    void InputCommand(bool jump, float horizontal, float vertical)
    {
        inputHorizontal = horizontal;
        inputVertical = vertical;
        inputJump = jump;
    }

    void Update()
    {
        if(isLocalPlayer)
        {
            inputHorizontal = -Input.GetAxis("Horizontal");
            inputVertical = Input.GetAxis("Vertical");
            inputJump = Input.GetButtonDown("Jump");

            if(inputHorizontal != oldInputHorizontal || inputVertical != oldInputVertical || inputJump != oldInputJump)
            {
                InputCommand(inputJump, inputHorizontal, inputVertical);
                oldInputHorizontal = inputHorizontal;
                oldInputVertical = inputVertical;
                oldInputJump = inputJump;
            }
        }
        if (!isServer) return;

        ChangeGravityVector();

        Debug.Log("Gravity " + gravity);
        Debug.Log("TargetVelocity " + targetVelocity);
        Debug.Log("Velocity " + velocity);
        Debug.Log("isGrounded " + isGrounded);
        Debug.Log("GroundNormal " + groundNormal);

        Debug.Log("up    " + (_rayCasts[0] || _rayCasts[1]).ToString());
        Debug.Log("down  " + (_rayCasts[2] || _rayCasts[3]).ToString());
        Debug.Log("right " + (_rayCasts[4] || _rayCasts[5]).ToString());
        Debug.Log("left  " + (_rayCasts[6] || _rayCasts[7]).ToString());

        isMoveUpDown = gravity.x == 0;
        targetVelocity = Vector2.zero;
        ComputeVelocity();
    }

    void FixedUpdate()
    {
        if (!isServer) return;

        velocity += gravityModifier * gravity * Time.deltaTime;
        if (isMoveUpDown)
            velocity.x = targetVelocity.x;
        else
            velocity.y = targetVelocity.y;
        isGrounded = false;
        Vector2 deltaPosition = velocity * Time.deltaTime;
        Vector2 moveAlongGround = new Vector2(groundNormal.y, -groundNormal.x);
        Vector2 move = isMoveUpDown ? moveAlongGround * deltaPosition.x : moveAlongGround * deltaPosition.y;
        Movement(move);
        move = isMoveUpDown ? Vector2.up * deltaPosition.y : Vector3.right * deltaPosition.x;
        Movement(move);// for jump
        velocity = new Vector2(Mathf.Clamp(velocity.x, -maxSpeed, maxSpeed), Mathf.Clamp(velocity.y, -maxSpeed, maxSpeed));
    }

    void Movement(Vector2 move)
    {
        var moveDistance = move.magnitude;
        if (moveDistance > minMoveDistance)
        {
            int count = rb2d.Cast(move, contactFilter, hitBuffer, moveDistance + shellRadius);
            isGrounded = count != 0;
            for (int i = 0; i < count; ++i)
            {
                var modifiedDistance = hitBuffer[i].distance - shellRadius;
                if (modifiedDistance < moveDistance)
                    moveDistance = modifiedDistance;
            }
        }
        rb2d.position += move.normalized * moveDistance;
    }

    private void ChangeGravityVector()
    {
        var half = 0.5f;
        _rayCasts[0] = (bool)Physics2D.Raycast(_collider.bounds.center + (transform.up + transform.right) * half, transform.up, _raycastDistance);
        _rayCasts[1] = (bool)Physics2D.Raycast(_collider.bounds.center + (transform.up - transform.right) * half, transform.up, _raycastDistance);
        _rayCasts[2] = (bool)Physics2D.Raycast(_collider.bounds.center - (transform.up + transform.right) * half, -transform.up, _raycastDistance);
        _rayCasts[3] = (bool)Physics2D.Raycast(_collider.bounds.center - (transform.up - transform.right) * half, -transform.up, _raycastDistance);
        _rayCasts[4] = (bool)Physics2D.Raycast(_collider.bounds.center + (transform.right + transform.up) * half, transform.right, _raycastDistance);
        _rayCasts[5] = (bool)Physics2D.Raycast(_collider.bounds.center + (transform.right - transform.up) * half, transform.right, _raycastDistance);
        _rayCasts[6] = (bool)Physics2D.Raycast(_collider.bounds.center - (transform.right + transform.up) * half, -transform.right, _raycastDistance);
        _rayCasts[7] = (bool)Physics2D.Raycast(_collider.bounds.center - (transform.right - transform.up) * half, -transform.right, _raycastDistance);
        {
            Debug.DrawRay(_collider.bounds.center + (transform.up + transform.right) * half, transform.up * _raycastDistance, Color.red);
            Debug.DrawRay(_collider.bounds.center + (transform.up - transform.right) * half, transform.up * _raycastDistance, Color.red);
            Debug.DrawRay(_collider.bounds.center - (transform.up + transform.right) * half, -transform.up * _raycastDistance, Color.green);
            Debug.DrawRay(_collider.bounds.center - (transform.up - transform.right) * half, -transform.up * _raycastDistance, Color.green);
            Debug.DrawRay(_collider.bounds.center + (transform.right + transform.up) * half, transform.right * _raycastDistance, Color.blue);
            Debug.DrawRay(_collider.bounds.center + (transform.right - transform.up) * half, transform.right * _raycastDistance, Color.blue);
            Debug.DrawRay(_collider.bounds.center - (transform.right + transform.up) * half, -transform.right * _raycastDistance, Color.magenta);
            Debug.DrawRay(_collider.bounds.center - (transform.right - transform.up) * half, -transform.right * _raycastDistance, Color.magenta);
        }

        var isHorizontalInput = inputHorizontal != 0;
        var isVerticalInput = inputVertical != 0;
        if (!isHorizontalInput && !isVerticalInput)
        {
            CheckGravityHorizontal();
            CheckGravityVertical();
        }
        else
        {
            if (isHorizontalInput)
            {
                CheckGravityHorizontal();
            }
            if (isVerticalInput)
            {
                CheckGravityVertical();
            }
        }
    }

    void CheckGravityHorizontal()
    {
        var g = -Physics2D.gravity.y;
        if (_rayCasts[0] || _rayCasts[1])
        {
            SetGravity(transform.up * g);
        }
        if (_rayCasts[2] || _rayCasts[3])
        {
            SetGravity(-transform.up * g);
        }
    }

    void CheckGravityVertical()
    {
        var g = -Physics2D.gravity.y;
        if (_rayCasts[4] || _rayCasts[5])
        {
            SetGravity(transform.right * g);
        }
        if (_rayCasts[6] || _rayCasts[7])
        {
            SetGravity(-transform.right * g);
        }
    }

    protected void ComputeVelocity()
    {
        var move = new Vector2(gravity.y * inputHorizontal, gravity.x * inputVertical);
        move.Normalize();
        var jumpSign = isMoveUpDown ? Mathf.Sign(-gravity.y) : Mathf.Sign(-gravity.x);
        if (inputJump && isGrounded)
        {
            if (isMoveUpDown)
                velocity.y = jumpTakeOffSpeed * jumpSign;
            else
                velocity.x = jumpTakeOffSpeed * jumpSign;
        }
        targetVelocity = move * maxSpeed;
    }
}
