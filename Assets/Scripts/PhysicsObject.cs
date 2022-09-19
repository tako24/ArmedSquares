using UnityEngine;

public class PhysicsObject : MonoBehaviour
{
    public float minGroundNormalY = 0.65f;
    public float gravityModifier = 1.0f;

    protected bool isMoveUpDown;
    protected Vector2 gravity;
    protected Vector2 targetVelocity;
    protected bool isGrounded;
    protected Vector2 groundNormal;
    protected Rigidbody2D rb2d;
    protected Vector2 velocity;
    protected ContactFilter2D contactFilter;
    protected RaycastHit2D[] hitBuffer = new RaycastHit2D[16];

    protected const float minMoveDistance = 0.001f;
    protected const float shellRadius = 0.01f;

    void OnEnable()
    {
        rb2d = GetComponent<Rigidbody2D>();
        _collider = GetComponent<BoxCollider2D>();
        SetGravity(Physics2D.gravity);
    }

    public void SetGravity(Vector2 newGravity)
    {
        gravity = newGravity;
        if (isMoveUpDown)
        {
            velocity.x = 0;
            velocity.y = gravity.y;
        }
        else
        {
            velocity.x = gravity.x;
            velocity.y = 0;
        }
        groundNormal = -gravity.normalized;
    }

    void Start()
    {
        contactFilter.useTriggers = false;
        contactFilter.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));
        contactFilter.useLayerMask = true;
    }

    void Update()
    {
        ChangeGravityVector();

        for (int i = 0; i < count; ++i)
            Debug.Log("  normal[" + i + "] = " + hitBuffer[i].normal);

        Debug.Log("Gravity " + gravity);
        Debug.Log("TargetVelocity " + targetVelocity);
        Debug.Log("Velocity " + velocity);
        Debug.Log("isGrounded " + isGrounded);
        Debug.Log("GroundNormal " + groundNormal);

        Debug.Log("up    " + ((bool)_rayCasts[0] || (bool)_rayCasts[1]).ToString());
        Debug.Log("down  " + ((bool)_rayCasts[2] || (bool)_rayCasts[3]).ToString());
        Debug.Log("right " + ((bool)_rayCasts[4] || (bool)_rayCasts[5]).ToString());
        Debug.Log("left  " + ((bool)_rayCasts[6] || (bool)_rayCasts[7]).ToString());

        isMoveUpDown = gravity.x == 0;
        targetVelocity = Vector2.zero;
        ComputeVelocity();
    }

    protected virtual void ComputeVelocity()
    {
    }

    void FixedUpdate()
    {
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
        velocity = new Vector2(Mathf.Clamp(velocity.x, -7, 7), Mathf.Clamp(velocity.y, -7, 7));
    }

    int count = 0;
    void Movement(Vector2 move)
    {
        var moveDistance = move.magnitude;
        if (moveDistance > minMoveDistance)
        {
            count = rb2d.Cast(move, contactFilter, hitBuffer, moveDistance + shellRadius);
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

    BoxCollider2D _collider;
    private RaycastHit2D[] _rayCasts = new RaycastHit2D[8];
    [SerializeField] private float _raycastDistance = 0.1f;
    private void ChangeGravityVector()
    {
        var half = 0.5f;
        var m1 = 1;// 0.95f;
        _rayCasts[0] = Physics2D.Raycast(_collider.bounds.center + (transform.up + transform.right * m1) * half, transform.up, _raycastDistance);
        _rayCasts[1] = Physics2D.Raycast(_collider.bounds.center + (transform.up - transform.right * m1) * half, transform.up, _raycastDistance);
        _rayCasts[2] = Physics2D.Raycast(_collider.bounds.center - (transform.up + transform.right * m1) * half, -transform.up, _raycastDistance);
        _rayCasts[3] = Physics2D.Raycast(_collider.bounds.center - (transform.up - transform.right * m1) * half, -transform.up, _raycastDistance);
        _rayCasts[4] = Physics2D.Raycast(_collider.bounds.center + (transform.right + transform.up * m1) * half, transform.right, _raycastDistance);
        _rayCasts[5] = Physics2D.Raycast(_collider.bounds.center + (transform.right - transform.up * m1) * half, transform.right, _raycastDistance);
        _rayCasts[6] = Physics2D.Raycast(_collider.bounds.center - (transform.right + transform.up * m1) * half, -transform.right, _raycastDistance);
        _rayCasts[7] = Physics2D.Raycast(_collider.bounds.center - (transform.right - transform.up * m1) * half, -transform.right, _raycastDistance);
        {
            Debug.DrawRay(_collider.bounds.center + (transform.up + transform.right * m1) * half, transform.up * _raycastDistance, Color.red);
            Debug.DrawRay(_collider.bounds.center + (transform.up - transform.right * m1) * half, transform.up * _raycastDistance, Color.red);
            Debug.DrawRay(_collider.bounds.center - (transform.up + transform.right * m1) * half, -transform.up * _raycastDistance, Color.green);
            Debug.DrawRay(_collider.bounds.center - (transform.up - transform.right * m1) * half, -transform.up * _raycastDistance, Color.green);
            Debug.DrawRay(_collider.bounds.center + (transform.right + transform.up * m1) * half, transform.right * _raycastDistance, Color.blue);
            Debug.DrawRay(_collider.bounds.center + (transform.right - transform.up * m1) * half, transform.right * _raycastDistance, Color.blue);
            Debug.DrawRay(_collider.bounds.center - (transform.right + transform.up * m1) * half, -transform.right * _raycastDistance, Color.magenta);
            Debug.DrawRay(_collider.bounds.center - (transform.right - transform.up * m1) * half, -transform.right * _raycastDistance, Color.magenta);
        }

        var h = Input.GetAxis("Horizontal") != 0;
        var v = Input.GetAxis("Vertical") != 0;

        if (!h && !v)
        {
            CheckGravityHorizontal();
            CheckGravityVertical();
        }
        else
        {
            if (h)
            {
                CheckGravityHorizontal();
            }
            if (v)
            {
                CheckGravityVertical();
            }
        }
    }

    void CheckGravityHorizontal()
    {
        var g = -Physics2D.gravity.y;
        if ((bool)_rayCasts[0] || (bool)_rayCasts[1])
        {
            SetGravity(transform.up * g);
        }
        if ((bool)_rayCasts[2] || (bool)_rayCasts[3])
        {
            SetGravity(-transform.up * g);
        }
    }

    void CheckGravityVertical()
    {
        var g = -Physics2D.gravity.y;
        if ((bool)_rayCasts[4] || (bool)_rayCasts[5])
        {
            SetGravity(transform.right * g);
        }
        if ((bool)_rayCasts[6] || (bool)_rayCasts[7])
        {
            SetGravity(-transform.right * g);
        }
    }
}
